using System.Collections.Generic;
namespace KnowledgeGraph.RabbitMQModels
{
    public class QuestionBatchRequest {
        public string Username { get; set; }
        public string Tech { get; set; }
        public List<string> Concepts;
        public QuestionBatchRequest (string _username, string _tech, List<string> _concepts) {
            Username = _username;
            Tech = _tech;
            Concepts.Clear ();
            Concepts.AddRange (_concepts);
        }
    }
}