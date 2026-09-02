namespace Ap2Master.Domain.ProjectManagement;

public sealed record NetworkActivity(
    string Id,
    string Name,
    int Duration,
    IReadOnlyList<string> Predecessors);

public sealed record NetworkActivityResult(
    string Id,
    string Name,
    int Duration,
    int Faz,
    int Fez,
    int Saz,
    int Sez,
    int GesamtPuffer,
    int FreierPuffer,
    bool IsCritical);

public sealed record NetworkPlanResult(
    IReadOnlyList<NetworkActivityResult> Activities,
    IReadOnlyList<string> CriticalPath,
    int ProjectDuration);

public static class NetworkPlanCalculator
{
    public static NetworkPlanResult Calculate(IReadOnlyList<NetworkActivity> activities)
    {
        var map = activities.ToDictionary(a => a.Id, StringComparer.OrdinalIgnoreCase);
        foreach (var a in activities)
        {
            foreach (var p in a.Predecessors)
            {
                if (!map.ContainsKey(p))
                    throw new InvalidOperationException($"Unbekannter Vorgänger '{p}' bei Vorgang '{a.Id}'.");
            }
        }

        var order = Topo(activities);
        var faz = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var fez = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var id in order)
        {
            var a = map[id];
            faz[id] = a.Predecessors.Count == 0 ? 0 : a.Predecessors.Max(p => fez[p]);
            fez[id] = faz[id] + a.Duration;
        }

        var duration = fez.Values.DefaultIfEmpty(0).Max();
        var saz = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var sez = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var id in order.AsEnumerable().Reverse())
        {
            var successors = activities.Where(a => a.Predecessors.Contains(id, StringComparer.OrdinalIgnoreCase)).ToList();
            sez[id] = successors.Count == 0 ? duration : successors.Min(s => saz[s.Id]);
            saz[id] = sez[id] - map[id].Duration;
        }

        var results = new List<NetworkActivityResult>();
        foreach (var a in activities)
        {
            var gp = saz[a.Id] - faz[a.Id];
            var successors = activities.Where(x => x.Predecessors.Contains(a.Id, StringComparer.OrdinalIgnoreCase)).ToList();
            var fp = successors.Count == 0
                ? duration - fez[a.Id]
                : successors.Min(s => faz[s.Id]) - fez[a.Id];
            results.Add(new NetworkActivityResult(
                a.Id, a.Name, a.Duration, faz[a.Id], fez[a.Id], saz[a.Id], sez[a.Id], gp, fp, gp == 0));
        }

        var critical = results.Where(r => r.IsCritical).Select(r => r.Id).ToList();
        return new NetworkPlanResult(results, critical, duration);
    }

    private static List<string> Topo(IReadOnlyList<NetworkActivity> activities)
    {
        var remaining = activities.ToDictionary(a => a.Id, a => a.Predecessors.ToHashSet(StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase);
        var result = new List<string>();
        while (remaining.Count > 0)
        {
            var next = remaining.Where(kv => kv.Value.Count == 0).Select(kv => kv.Key).OrderBy(x => x).ToList();
            if (next.Count == 0)
                throw new InvalidOperationException("Der Netzplan enthält einen Zyklus.");
            foreach (var n in next)
            {
                result.Add(n);
                remaining.Remove(n);
                foreach (var preds in remaining.Values) preds.Remove(n);
            }
        }
        return result;
    }
}
