using System;
using Neo4jClient;

namespace KnowledgeGraph.Services {
    public class GraphDbConnection : IDisposable {
        public IGraphClient graph{ get; }
        public IBoltGraphClient boltGraphClient { get; set; }
        public GraphDbConnection () {
            boltGraphClient = new BoltGraphClient(new Uri("localhost:7687"), "neo4j", "asdfgh12345");
            graph = new GraphClient (new Uri ("http://localhost:7474/db/data"), "neo4j", "asdfgh12345");
            graph.Connect();
        }

        public void Dispose () {
            graph.Dispose();
        }
    }
}