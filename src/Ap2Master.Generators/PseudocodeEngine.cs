using System.Globalization;
using System.Text;

namespace Ap2Master.Generators;

public sealed class PseudocodeEngine : Ap2Master.Application.Abstractions.IPseudocodeEngine
{
    public Ap2Master.Application.Abstractions.PseudocodeResult Run(string source, IReadOnlyDictionary<string, object?> inputs)
    {
        try
        {
            var vm = new Interpreter(source, inputs, trace: false);
            vm.Execute();
            return new Ap2Master.Application.Abstractions.PseudocodeResult(true, vm.ReturnValue, vm.Variables, null, vm.Output);
        }
        catch (Exception ex)
        {
            return new Ap2Master.Application.Abstractions.PseudocodeResult(false, null, new Dictionary<string, object?>(), ex.Message, Array.Empty<string>());
        }
    }

    public IReadOnlyList<Ap2Master.Application.Abstractions.PseudocodeStep> Trace(string source, IReadOnlyDictionary<string, object?> inputs)
    {
        var vm = new Interpreter(source, inputs, trace: true);
        vm.Execute();
        return vm.Steps;
    }

    private sealed class Interpreter
    {
        private readonly string[] _lines;
        private readonly bool _trace;
        private int _ip;
        public Dictionary<string, object?> Variables { get; } = new(StringComparer.OrdinalIgnoreCase);
        public List<string> Output { get; } = [];
        public object? ReturnValue { get; private set; }
        public List<Ap2Master.Application.Abstractions.PseudocodeStep> Steps { get; } = [];

        public Interpreter(string source, IReadOnlyDictionary<string, object?> inputs, bool trace)
        {
            _lines = source.Replace("\r", "").Split('\n');
            _trace = trace;
            foreach (var kv in inputs) Variables[kv.Key] = kv.Value;
        }

        public void Execute()
        {
            var guard = 0;
            while (_ip < _lines.Length)
            {
                if (++guard > 100000) throw new InvalidOperationException("Endlosschleife erkannt.");
                var raw = _lines[_ip];
                var line = raw.Trim();
                if (line.Length == 0 || line.StartsWith("//") || line.StartsWith("#"))
                {
                    _ip++;
                    continue;
                }
                if (_trace) Steps.Add(new Ap2Master.Application.Abstractions.PseudocodeStep(_ip + 1, line, new Dictionary<string, object?>(Variables, StringComparer.OrdinalIgnoreCase)));
                ExecuteLine(line);
            }
        }

        private void ExecuteLine(string line)
        {
            var u = line.ToUpperInvariant();
            if (u.StartsWith("SET ") || u.StartsWith("VAR "))
            {
                var rest = line[(line.IndexOf(' ') + 1)..];
                var eq = rest.Split('=', 2);
                var name = eq[0].Replace(":", " ").Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];
                Variables[name] = eq.Length > 1 ? Eval(eq[1].Trim()) : 0;
                _ip++;
                return;
            }
            if (u.StartsWith("PRINT "))
            {
                Output.Add(Convert.ToString(Eval(line[6..].Trim()), CultureInfo.InvariantCulture) ?? "");
                _ip++;
                return;
            }
            if (u.StartsWith("RETURN "))
            {
                ReturnValue = Eval(line[7..].Trim());
                _ip = _lines.Length;
                return;
            }
            if (u.StartsWith("IF "))
            {
                var cond = ExtractCondition(line, "IF", "THEN");
                if (!IsTrue(Eval(cond))) SkipBlock("IF ", "ENDIF", alsoStopAtElse: true);
                _ip++;
                return;
            }
            if (u == "ELSE")
            {
                SkipBlock("IF ", "ENDIF", alsoStopAtElse: false);
                _ip++;
                return;
            }
            if (u is "ENDIF" or "ENDWHILE" or "ENDFOR")
            {
                if (u == "ENDWHILE" || u == "ENDFOR") JumpBackTo(u == "ENDWHILE" ? "WHILE " : "FOR ");
                else _ip++;
                return;
            }
            if (u.StartsWith("WHILE "))
            {
                var cond = ExtractCondition(line, "WHILE", "DO");
                if (!IsTrue(Eval(cond.Replace(" DO", "", StringComparison.OrdinalIgnoreCase)))) SkipBlock("WHILE ", "ENDWHILE", alsoStopAtElse: false);
                _ip++;
                return;
            }
            if (u.StartsWith("FOR "))
            {
                // FOR i FROM a TO b
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var name = parts[1];
                var from = Convert.ToInt32(Eval(parts[3]), CultureInfo.InvariantCulture);
                var to = Convert.ToInt32(Eval(parts[5]), CultureInfo.InvariantCulture);
                if (!Variables.ContainsKey(name + "#for"))
                {
                    Variables[name] = from;
                    Variables[name + "#for"] = _ip;
                    if (from > to)
                    {
                        Variables.Remove(name + "#for");
                        SkipBlock("FOR ", "ENDFOR", alsoStopAtElse: false);
                    }
                }
                else
                {
                    var v = Convert.ToInt32(Variables[name], CultureInfo.InvariantCulture) + 1;
                    Variables[name] = v;
                    if (v > to)
                    {
                        Variables.Remove(name + "#for");
                        SkipBlock("FOR ", "ENDFOR", alsoStopAtElse: false);
                    }
                }
                _ip++;
                return;
            }

            if (line.Contains('=') && !u.StartsWith("IF"))
            {
                var eq = line.Split('=', 2);
                Variables[eq[0].Trim()] = Eval(eq[1].Trim());
                _ip++;
                return;
            }

            throw new InvalidOperationException($"Unbekannte Anweisung in Zeile {_ip + 1}: {line}");
        }

        private void SkipBlock(string openPrefix, string closeToken, bool alsoStopAtElse)
        {
            var depth = 0;
            _ip++;
            while (_ip < _lines.Length)
            {
                var u = _lines[_ip].Trim().ToUpperInvariant();
                if (u.StartsWith(openPrefix, StringComparison.Ordinal))
                    depth++;
                else if (alsoStopAtElse && depth == 0 && u == "ELSE")
                    return;
                else if (u == closeToken)
                {
                    if (depth == 0) return;
                    depth--;
                }
                _ip++;
            }
        }

        private void JumpBackTo(string token)
        {
            for (var i = _ip - 1; i >= 0; i--)
            {
                if (_lines[i].Trim().StartsWith(token, StringComparison.OrdinalIgnoreCase))
                {
                    _ip = i;
                    return;
                }
            }
            _ip++;
        }

        private static string ExtractCondition(string line, string start, string end)
        {
            var s = line.Trim();
            var i = s.IndexOf(start, StringComparison.OrdinalIgnoreCase) + start.Length;
            var j = s.ToUpperInvariant().LastIndexOf(end, StringComparison.Ordinal);
            return j > i ? s[i..j].Trim() : s[i..].Trim();
        }

        private object? Eval(string expr)
        {
            expr = expr.Trim().TrimEnd(';');
            if (expr.Length >= 2 && expr[0] == '"' && expr[^1] == '"') return expr[1..^1];
            if (bool.TryParse(expr, out var b)) return b;
            if (int.TryParse(expr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n)) return n;
            if (Variables.TryGetValue(expr, out var v)) return v;

            foreach (var op in new[] { ">=", "<=", "!=", "==", ">", "<", "+", "-", "*", "/" })
            {
                var idx = SplitOp(expr, op);
                if (idx < 0) continue;
                var left = Eval(expr[..idx].Trim());
                var right = Eval(expr[(idx + op.Length)..].Trim());
                return Apply(left, right, op);
            }

            if (expr.Contains('[') && expr.Contains(']'))
            {
                var name = expr[..expr.IndexOf('[')].Trim();
                var close = expr.IndexOf(']');
                var idxExpr = expr[(expr.IndexOf('[') + 1)..close];
                if (expr[(close + 1)..].Trim().Length == 0)
                {
                    var idx = Convert.ToInt32(Eval(idxExpr), CultureInfo.InvariantCulture);
                    if (Variables[name] is IList<int> list) return list[idx];
                    if (Variables[name] is int[] arr) return arr[idx];
                }
            }

            if (expr.Contains('.') && Variables.TryGetValue(expr.Split('.')[0], out var obj))
                return obj;

            throw new InvalidOperationException($"Ausdruck nicht auswertbar: {expr}");
        }

        private static int SplitOp(string expr, string op)
        {
            var depth = 0;
            for (var i = expr.Length - 1; i >= 0; i--)
            {
                if (expr[i] == ']') depth++;
                if (expr[i] == '[') depth--;
                if (depth == 0 && i + op.Length <= expr.Length && expr.Substring(i, op.Length) == op)
                    return i;
            }
            return -1;
        }

        private static object Apply(object? left, object? right, string op)
        {
            var l = Convert.ToDouble(left, CultureInfo.InvariantCulture);
            var r = Convert.ToDouble(right, CultureInfo.InvariantCulture);
            return op switch
            {
                "+" => l + r,
                "-" => l - r,
                "*" => l * r,
                "/" => r == 0 ? throw new DivideByZeroException() : l / r,
                ">" => l > r,
                "<" => l < r,
                ">=" => l >= r,
                "<=" => l <= r,
                "==" => Math.Abs(l - r) < 0.0000001,
                "!=" => Math.Abs(l - r) > 0.0000001,
                _ => throw new InvalidOperationException(op)
            };
        }

        private static bool IsTrue(object? v) => v is bool b ? b : Convert.ToDouble(v, CultureInfo.InvariantCulture) != 0;
    }
}
