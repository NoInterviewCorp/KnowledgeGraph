using System;
using Microsoft.Extensions.Options;
using Neo4jClient;

namespace KnowledgeGraph.Services
{
    public class GraphDbConnection : IDisposable
    {
        public IGraphClient graph { get; }
        public GraphDbConnection(IOptions<Neo4jSettings> options)
        {
            var data = options.Value;
            try
            {
                graph = new GraphClient(
                        new Uri(data.ConnectionString),
                        data.UserId,
                        data.Password
                    );
                graph.Connect();
            }
            catch (Exception e)
            {
                Console.WriteLine("-------------------------------------------------------------------------");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                Console.WriteLine("-------------------------------------------------------------------------");
            }
        }

        public void Dispose()
        {
            graph.Dispose();
        }
    }
}
