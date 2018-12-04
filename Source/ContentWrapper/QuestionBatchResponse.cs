using System.Collections.Generic;
using KnowledgeGraph.Models;
namespace KnowledgeGraph.RabbitMQModels {
    public class QuestionBatchResponse {
        public string username { get; set; }
        public Dictionary<string, List<string>> questionids;
        public QuestionBatchResponse (string _username) {
            username = _username;
            questionids.Clear ();
        }
    }
}