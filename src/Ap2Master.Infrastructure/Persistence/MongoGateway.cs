using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using MongoDB.EntityFrameworkCore.Extensions;

namespace Ap2Master.Infrastructure.Persistence;

public sealed record MongoDatabaseOptions(string DatabaseName);

public static class MongoGateway
{
    public const string DatabaseName = "ap2master";
    public const string ClientSubject = "CN=ap2master-app";
    private const string EnvVar = "AP2MASTER_MONGO_URL";
    private const string PfxEnvVar = "AP2MASTER_MONGO_PFX";
    private const string PfxPasswordEnvVar = "AP2MASTER_MONGO_PFX_PASSWORD";
    private static readonly byte[] Entropy = "Ap2Master.Mongo.v1"u8.ToArray();
    private static readonly object Gate = new();
    private static MongoClient? _client;

    public static void Apply(DbContextOptionsBuilder builder, string databaseName)
        => builder.UseMongoDB(Client(), databaseName);

    public static IMongoDatabase Database(string databaseName)
        => Client().GetDatabase(databaseName);

    public static void DropDatabase(string databaseName)
        => Client().DropDatabase(databaseName);

    public static string Redact(string? message)
    {
        if (string.IsNullOrEmpty(message)) return "";
        var text = Regex.Replace(message, @"mongodb(\+srv)?://\S+", "mongodb://***", RegexOptions.IgnoreCase);
        return Regex.Replace(text, @"//[^/\s:]+:[^@\s]+@", "//***:***@");
    }

    private static MongoClient Client()
    {
        if (_client is not null) return _client;
        lock (Gate)
        {
            if (_client is not null) return _client;
            var material = LoadMaterial();
            var settings = MongoClientSettings.FromUrl(new MongoUrl(material.Url));
            settings.ApplicationName = "Ap2Master";
            settings.MaxConnectionPoolSize = 8;
            settings.MinConnectionPoolSize = 0;
            settings.MaxConnecting = 2;
            settings.WaitQueueTimeout = TimeSpan.FromSeconds(5);
            settings.ConnectTimeout = TimeSpan.FromSeconds(8);
            settings.SocketTimeout = TimeSpan.FromSeconds(20);
            settings.ServerSelectionTimeout = TimeSpan.FromSeconds(8);
            settings.RetryWrites = true;
            if (material.Certificate is not null)
            {
                settings.UseTls = true;
                settings.Credential = MongoCredential.CreateMongoX509Credential(ClientSubject);
                settings.SslSettings = new SslSettings
                {
                    ClientCertificates = new[] { material.Certificate },
                    CheckCertificateRevocation = false
                };
            }
            _client = new MongoClient(settings);
            return _client;
        }
    }

    private static Material LoadMaterial()
    {
        var fromEnv = Environment.GetEnvironmentVariable(EnvVar);
        if (!string.IsNullOrWhiteSpace(fromEnv))
        {
            var payload = PayloadFromEnvironment(fromEnv.Trim());
            Protect(payload);
            return Parse(payload);
        }

        var path = SecretPath();
        if (!File.Exists(path))
            throw new InvalidOperationException(
                "MongoDB-Zugang ist nicht eingerichtet. Setzen Sie einmalig AP2MASTER_MONGO_URL und AP2MASTER_MONGO_PFX für diesen Windows-Benutzer.");

        var raw = ProtectedData.Unprotect(File.ReadAllBytes(path), Entropy, DataProtectionScope.CurrentUser);
        return Parse(Encoding.UTF8.GetString(raw));
    }

    private static string PayloadFromEnvironment(string url)
    {
        var pfxPath = Environment.GetEnvironmentVariable(PfxEnvVar);
        if (string.IsNullOrWhiteSpace(pfxPath))
            return url;

        if (!File.Exists(pfxPath))
            throw new InvalidOperationException("AP2MASTER_MONGO_PFX zeigt auf keine Zertifikatsdatei.");

        var password = Environment.GetEnvironmentVariable(PfxPasswordEnvVar) ?? "";
        var pfx = File.ReadAllBytes(pfxPath);
        return "v2\n" + url + "\n" + password + "\n" + Convert.ToBase64String(pfx);
    }

    private static Material Parse(string payload)
    {
        if (!payload.StartsWith("v2\n", StringComparison.Ordinal))
            return new Material(payload.Trim(), null);

        var parts = payload.Split('\n', 4);
        if (parts.Length != 4 || parts[3].Length == 0)
            throw new InvalidOperationException("Das gespeicherte MongoDB-Zertifikat ist beschädigt.");

        var certificate = new X509Certificate2(
            Convert.FromBase64String(parts[3].Trim()),
            parts[2],
            X509KeyStorageFlags.EphemeralKeySet);
        if (!certificate.HasPrivateKey)
            throw new InvalidOperationException("Das MongoDB-Clientzertifikat enthält keinen privaten Schlüssel.");
        return new Material(parts[1].Trim(), certificate);
    }

    private static void Protect(string payload)
    {
        var url = payload.StartsWith("v2\n", StringComparison.Ordinal)
            ? payload.Split('\n', 3)[1]
            : payload;
        if (!url.StartsWith("mongodb://", StringComparison.OrdinalIgnoreCase)
            && !url.StartsWith("mongodb+srv://", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("AP2MASTER_MONGO_URL ist keine MongoDB-Verbindungszeichenfolge.");

        var path = SecretPath();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var bytes = ProtectedData.Protect(Encoding.UTF8.GetBytes(payload), Entropy, DataProtectionScope.CurrentUser);
        File.WriteAllBytes(path, bytes);
    }

    private sealed record Material(string Url, X509Certificate2? Certificate);

    private static string SecretPath()
        => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Ap2Master", "mongo.protected");
}

internal static class ClusterLimiter
{
    private static readonly SemaphoreSlim Slots = new(4, 4);
    private static readonly object WindowLock = new();
    private static readonly Queue<long> Hits = new();
    private const int MaxPerSecond = 30;

    public static void Enter()
    {
        if (!Slots.Wait(TimeSpan.FromSeconds(3)))
            throw new InvalidOperationException("Zu viele Datenbankzugriffe. Bitte kurz warten.");

        lock (WindowLock)
        {
            var now = Environment.TickCount64;
            while (Hits.Count > 0 && now - Hits.Peek() > 1000)
                Hits.Dequeue();
            if (Hits.Count >= MaxPerSecond)
            {
                Slots.Release();
                throw new InvalidOperationException("Zu viele Datenbankzugriffe. Bitte kurz warten.");
            }
            Hits.Enqueue(now);
        }
    }

    public static void Exit() => Slots.Release();
}
