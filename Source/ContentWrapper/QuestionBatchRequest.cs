using System.Collections.Generic;
namespace KnowledgeGraph.RabbitMQModels
{
    public class QuestionBatchRequest {
        public string username { get; set; }
        public string tech { get; set; }
        public List<string> concepts;
        public QuestionBatchRequest (string _username, string _tech, List<string> _concepts) {
            username = _username;
            tech = _tech;
            concepts.Clear ();
            concepts.AddRange (_concepts);
        }
    }
}