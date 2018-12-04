using System;
using Microsoft.Extensions.Options;
using Neo4jClient;

namespace KnowledgeGraph.Services
{
    public class GraphDbConnection : IDisposable
    {
        public IGraphClient graph { get; }
        public IBoltGraphClient boltGraphClient { get; set; }
        public GraphDbConnection(IOptions<Neo4jSettings> settings)
        {
            var data = settings.Value;
            graph = new GraphClient(
                new Uri(data.ConnectionString),
                data.UserId,
                data.Password
            );
            graph.Connect();
        }

        public void Dispose()
        {
            graph.Dispose();
        }
    }
}