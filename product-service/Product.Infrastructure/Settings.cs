namespace Product.Infrastructure
{
    public class Settings
    {
        public MongoSettings Mongo { get; set; }
    }

    public class MongoSettings
    {
        public string Database { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string ConnectionString => $@"mongodb://{User}:{Password}@{Host}:{Port}";
    }
}