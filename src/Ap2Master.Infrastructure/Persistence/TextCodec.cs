using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Ap2Master.Infrastructure.Persistence;

public static class TextCodec
{
    public const int MinLength = 64;
    public const string Prefix = "pack:";

    public static bool IsToken(string value)
        => value.Length == Prefix.Length + 64
           && value.StartsWith(Prefix, StringComparison.Ordinal)
           && value.Skip(Prefix.Length).All(IsHex);

    public static string Hash(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    public static string Token(string hash) => Prefix + hash;

    public static byte[]? TryCompress(string value)
    {
        if (value.Length < MinLength || IsToken(value)) return null;
        var raw = Encoding.UTF8.GetBytes(value);
        using var output = new MemoryStream();
        using (var brotli = new BrotliStream(output, CompressionLevel.SmallestSize, leaveOpen: true))
            brotli.Write(raw);
        var payload = output.ToArray();
        return payload.Length + 16 >= raw.Length ? null : payload;
    }

    public static string Decompress(byte[] payload)
    {
        using var input = new MemoryStream(payload);
        using var brotli = new BrotliStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();
        brotli.CopyTo(output);
        return Encoding.UTF8.GetString(output.ToArray());
    }

    private static bool IsHex(char c)
        => c is >= '0' and <= '9' or >= 'a' and <= 'f';
}

public sealed class MaterialText
{
    public string Id { get; set; } = "";
    public byte[] Payload { get; set; } = [];
}

public sealed class MaterialLibrary
{
    public static MaterialLibrary Shared { get; } = new();

    private readonly object _gate = new();
    private readonly Dictionary<string, string> _plain = new(StringComparer.Ordinal);
    private readonly Dictionary<string, byte[]> _pending = new(StringComparer.Ordinal);
    private string _database = "";

    public void Bind(string databaseName)
    {
        lock (_gate)
        {
            if (_database == databaseName) return;
            _database = databaseName;
            _plain.Clear();
            _pending.Clear();
        }
    }

    public async Task WarmAsync(AppDbContext db, CancellationToken cancellationToken)
    {
        var rows = await db.Set<MaterialText>().AsNoTracking().ToListAsync(cancellationToken);
        lock (_gate)
        {
            foreach (var row in rows)
                _plain[row.Id] = TextCodec.Decompress(row.Payload);
        }
    }

    public string? Encode(string? value)
    {
        if (string.IsNullOrEmpty(value) || TextCodec.IsToken(value)) return value;
        var payload = TextCodec.TryCompress(value);
        if (payload is null) return value;
        var hash = TextCodec.Hash(value);
        lock (_gate)
        {
            if (_plain.ContainsKey(hash)) return TextCodec.Token(hash);
            _plain[hash] = value;
            _pending[hash] = payload;
        }
        return TextCodec.Token(hash);
    }

    public string? Decode(string? value)
    {
        if (string.IsNullOrEmpty(value) || !TextCodec.IsToken(value)) return value;
        var hash = value[TextCodec.Prefix.Length..];
        lock (_gate)
            return _plain.TryGetValue(hash, out var text) ? text : value;
    }

    public List<MaterialText> TakePending()
    {
        lock (_gate)
        {
            var items = _pending.Select(kv => new MaterialText { Id = kv.Key, Payload = kv.Value }).ToList();
            _pending.Clear();
            return items;
        }
    }
}
