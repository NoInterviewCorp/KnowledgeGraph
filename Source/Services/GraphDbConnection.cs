using System;
using Neo4jClient;

namespace KnowledgeGraph.Services {
    public class GraphDbConnection : IDisposable {
        public IGraphClient graph{ get; }
        public IBoltGraphClient boltGraphClient { get; set; }
        public GraphDbConnection () {
            graph = new GraphClient (new Uri ("http://localhost:7474/db/data"), "neo4j", "qwertyuiop");
            graph.Connect();
        }

        public void Dispose () {
            graph.Dispose();
        }
    }
}