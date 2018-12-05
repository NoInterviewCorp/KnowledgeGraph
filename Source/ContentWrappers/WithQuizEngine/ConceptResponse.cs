using System.Collections.Generic;

namespace KnowledgeGraph.RabbitMQModels {
    public class ConceptResponse {
        public string username { get; set; }
        public List<string> concepts;
        public ConceptResponse()
        {
            
        }
        public ConceptResponse(string _username)
        {
            username = _username;
            concepts = new List<string>();
        }
    }
}