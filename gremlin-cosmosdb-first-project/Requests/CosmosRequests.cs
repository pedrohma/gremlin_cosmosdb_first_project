using Gremlin.Net.Driver;
using Gremlin.Net.Driver.Exceptions;
using gremlin_cosmosdb_first_project.Util;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gremlin_cosmosdb_first_project.Requests
{
    public class CosmosRequests
    {
        public async Task<ResultSet<dynamic>> SubmitRequest(GremlinClient gremlinClient, KeyValuePair<string, string> query)
        {
            try
            {
                var result = await gremlinClient.SubmitAsync<dynamic>(query.Value);
                return result;
            }
            catch (ResponseException e)
            {
                Console.WriteLine("\tRequest Error!");

                // Print the Gremlin status code.
                Console.WriteLine($"\tStatusCode: {e.StatusCode}");

                // On error, ResponseException.StatusAttributes will include the common StatusAttributes for successful requests, as well as
                // additional attributes for retry handling and diagnostics.
                // These include:
                //  x-ms-retry-after-ms         : The number of milliseconds to wait to retry the operation after an initial operation was throttled. This will be populated when
                //                              : attribute 'x-ms-status-code' returns 429.
                //  x-ms-activity-id            : Represents a unique identifier for the operation. Commonly used for troubleshooting purposes.
                CosmosUtil.PrintStatusAttributes(e.StatusAttributes);
                Console.WriteLine($"\t[\"x-ms-retry-after-ms\"] : { CosmosUtil.GetValueAsString(e.StatusAttributes, "x-ms-retry-after-ms")}");
                Console.WriteLine($"\t[\"x-ms-activity-id\"] : { CosmosUtil.GetValueAsString(e.StatusAttributes, "x-ms-activity-id")}");

                throw;
            }
        }
    }
}
