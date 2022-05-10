using Gremlin.Net.Driver;
using Gremlin.Net.Structure.IO.GraphSON;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.WebSockets;

namespace gremlin_cosmosdb_first_project.Connection
{
    public class CosmosDB
    {
        public const string GraphSON2MimeType = "application/vnd.gremlin-v2.0+json";
        public static IConfigurationRoot Configuration { get; set; }
        private GremlinServer Server { get; set; }
        private static bool EnableSSL
        {
            get
            {
                if (Configuration["EnableSSL"] == null)
                {
                    return true;
                }

                if (!bool.TryParse(Configuration["EnableSSL"], out bool value))
                {
                    throw new ArgumentException("Invalid env var: EnableSSL is not a boolean");
                }

                return value;
            }
        }
        private static int Port
        {
            get
            {
                if (Configuration["Port"] == null)
                {
                    return 443;
                }

                if (!int.TryParse(Configuration["Port"], out int port))
                {
                    throw new ArgumentException("Invalid env var: Port is not an integer");
                }

                return port;
            }
        }

        public CosmosDB(IConfigurationRoot _configuration)
        {
            Configuration = _configuration;
        }

        public GremlinServer ConnectServer()
        {
            string Host = Configuration["CosmosDB:Host"] ?? throw new ArgumentException("Missing env var: Host");
            string PrimaryKey = Configuration["CosmosDB:PrimaryKey"] ?? throw new ArgumentException("Missing env var: PrimaryKey");
            string Database = Configuration["CosmosDB:DatabaseName"] ?? throw new ArgumentException("Missing env var: DatabaseName");
            string Container = Configuration["CosmosDB:ContainerName"] ?? throw new ArgumentException("Missing env var: ContainerName");
            string containerLink = "/dbs/" + Database + "/colls/" + Container;

            Console.WriteLine($"Connecting to: host: {Host}, port: {Port}, container: {containerLink}, ssl: {EnableSSL}");

            Server = new GremlinServer(Host, Port, enableSsl: EnableSSL,
                                                    username: containerLink,
                                                    password: PrimaryKey);

            return Server;
        }

        public GremlinClient ConnectClient()
        {
            ConnectionPoolSettings connectionPoolSettings = new ConnectionPoolSettings()
            {
                MaxInProcessPerConnection = 10,
                PoolSize = 30,
                ReconnectionAttempts = 3,
                ReconnectionBaseDelay = TimeSpan.FromMilliseconds(500)
            };

            var webSocketConfiguration =
                new Action<ClientWebSocketOptions>(options =>
                {
                    options.KeepAliveInterval = TimeSpan.FromSeconds(10);
                });

            return new GremlinClient(Server,
                                           new GraphSON2Reader(),
                                           new GraphSON2Writer(),
                                           GraphSON2MimeType,
                                           connectionPoolSettings,
                                           webSocketConfiguration);
        }
    }
}
