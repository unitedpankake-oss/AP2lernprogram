namespace Ap2Master.Infrastructure.Persistence;

public sealed record MongoDatabaseOptions(string ConnectionString, string DatabaseName);

public static class MongoSettings
{
    public const string ConnectionString =
        "mongodb://ivanhovorun_db_user:yKLudiAHaSYQXC9D@ac-j74dz4y-shard-00-00.5loxfvy.mongodb.net:27017,ac-j74dz4y-shard-00-01.5loxfvy.mongodb.net:27017,ac-j74dz4y-shard-00-02.5loxfvy.mongodb.net:27017/?ssl=true&replicaSet=atlas-rreaj9-shard-0&authSource=admin&retryWrites=true&w=majority";

    public const string DatabaseName = "ap2master";
}
