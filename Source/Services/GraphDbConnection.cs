using System;
using Microsoft.Extensions.Options;
using Neo4jClient;

namespace KnowledgeGraph.Services {
    public class GraphDbConnection : IDisposable {
        public IGraphClient graph{ get; }
        // public IBoltGraphClient boltGraphClient { get; set; }
        public GraphDbConnection (IOptions<Neo4jSettings> options) {
            var data = options.Value;
            // boltGraphClient = new BoltGraphClient(new Uri("localhost:7687"), "neo4j", "asdfgh12345");
            try{graph = new GraphClient (new Uri (data.ConnectionString), data.UserId, data.Password);
            graph.Connect();}
	    catch(Exception e){
	     	Console.WriteLine("-------------------------------------------------------------------------");
		Console.WriteLine(e.Message);
		Console.WriteLine(e.StackTrace);
		Console.WriteLine("-------------------------------------------------------------------------");
	    }
        }

        public void Dispose () {
            graph.Dispose();
        }
    }
}
