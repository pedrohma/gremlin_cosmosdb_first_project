using Gremlin.Net.Driver;
using gremlin_cosmosdb_first_project.Connection;
using gremlin_cosmosdb_first_project.Requests;
using gremlin_cosmosdb_first_project.Util;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace gremlin_cosmosdb_first_project
{
    internal class Program
    {
        public static IConfigurationRoot Configuration { get; set; }
        static void Main(string[] args)
        {
            string path = Path.GetFullPath(@"..\..\..\");
            var builder = new ConfigurationBuilder()
                .SetBasePath(path)
                .AddJsonFile("appsettings.json", false);
            Configuration = builder.Build();

            Dictionary<string, string> gremlinQueries = new Dictionary<string, string>
            {
                { "Cleanup",        "g.V().drop()" },
                { "AddVertex 1",    "g.addV('person').property('id', 'pedro').property('firstName', 'Pedro').property('age', 18).property('city', 'Los Angeles').property('partitionKey', 'pk')" },
                { "AddVertex 2",    "g.addV('person').property('id', 'nick').property('firstName', 'Nick').property('lastName', 'Pop').property('age', 35).property('city', 'San Francisco').property('partitionKey', 'pk')" },
                { "AddVertex 3",    "g.addV('person').property('id', 'john').property('firstName', 'John').property('lastName', 'Depp').property('city', 'San Diego').property('partitionKey', 'pk')" },
                { "AddVertex 4",    "g.addV('person').property('id', 'mark').property('firstName', 'Mark').property('lastName', 'Hoppus').property('partitionKey', 'pk')" },
                { "AddEdge 1",      "g.V('pedro').addE('knows').to(g.V('nick'))" },
                { "AddEdge 2",      "g.V('pedro').addE('knows').to(g.V('john'))" },
                { "AddEdge 3",      "g.V('john').addE('knows').to(g.V('mark'))" },
                { "UpdateVertex",   "g.V('pedro').property('age', 22)" },
                { "Filter Range",   "g.V().hasLabel('person').has('age', gt(40))" },
                { "Project",        "g.V().hasLabel('person').values('city')" },
                { "Sort",           "g.V().hasLabel('person').order().by('firstName', decr)" },
                { "Traverse",       "g.V('pedro').out('knows').hasLabel('person')" },
                { "Traverse 2x",    "g.V('pedro').out('knows').hasLabel('person').out('knows').hasLabel('person')" },
                { "Loop",           "g.V('pedro').repeat(out()).until(has('id', 'mark')).path()" },
                { "DropEdge",       "g.V('pedro').outE('knows').where(inV().has('id', 'nick')).drop()" },
                { "DropVertex",     "g.V('pedro').drop()" }
            };

            CosmosDB db = new CosmosDB(Configuration);

            db.ConnectServer();

            foreach (var query in gremlinQueries)
            {
                var client = db.ConnectClient();

                Console.WriteLine(string.Format("Running this query: {0}: {1}", query.Key, query.Value));

                ResultSet<dynamic> resultSet = new CosmosRequests().SubmitRequest(client, query).Result;

                if (resultSet.Count > 0)
                {
                    Console.WriteLine("\tResult:");
                    foreach (var result in resultSet)
                    {
                        if (result.GetType() == typeof(string))
                        {
                            Console.WriteLine(result.ToString());
                        }
                        else
                        {
                            // The vertex results are formed as Dictionaries with a nested dictionary for their properties
                            foreach (var resp in result)
                            {
                                if (resp.Value.GetType() == typeof(string))
                                {
                                    var output = JsonConvert.SerializeObject(resp);
                                    Console.WriteLine($"\t{output}");
                                }
                            }
                            Console.WriteLine();
                        }
                    }
                    CosmosUtil.PrintStatusAttributes(resultSet.StatusAttributes);
                    Console.WriteLine();
                }
            }

        }
    }
}
