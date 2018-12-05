using System.Collections.Generic;
using KnowledgeGraph.Models;
namespace KnowledgeGraph.RabbitMQModels {
    public class GraphBatchResponse {
        public string username { get; set; }
        public List<string> questionids = new List<string>();

        public GraphBatchResponse()
        {    
        }

        public GraphBatchResponse (string _username) 
        {
            username = _username;
            questionids.Clear ();
        }
    }
}