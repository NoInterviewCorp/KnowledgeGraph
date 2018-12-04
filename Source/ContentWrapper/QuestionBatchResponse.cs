using System.Collections.Generic;
using KnowledgeGraph.Models;
namespace KnowledgeGraph.RabbitMQModels {
    public class GraphBatchResponse {
        public string username { get; set; }
        public Dictionary<string, List<string>> questionids;
        public GraphBatchResponse (string _username) {
            username = _username;
            questionids.Clear ();
        }
    }
}