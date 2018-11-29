using System.Collections.Generic;
using KnowledgeGraph.Models;
namespace KnowledgeGraph.RabbitMQModels {
    public class QuestionIdsBatchResponseModel {
        public string username { get; set; }
        public Dictionary<string, List<string>> questionids;
        public QuestionIdsBatchResponseModel (string _username) {
            username = _username;
            questionids.Clear ();
        }
    }
}