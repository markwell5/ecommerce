namespace Product.Service.Config
{
    public class Settings
    {
        public ConnectionStrings ConnectionStrings { get; set; }
    }

    public class ConnectionStrings
    {
        public string DbConnectionString { get; set; }
        public string BrokerConnectionString { get; set; }
    }
}
