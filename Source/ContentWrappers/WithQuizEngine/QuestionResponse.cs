using System.Collections.Generic;

namespace KnowledgeGraph.RabbitMQModels
{
    public class QuestionIdsResponse
    {
        public string Username { get; set; } 
        public Dictionary<string,List<string>> IdRequestDictionary;
        public QuestionIdsResponse(string _username)
        {
            Username = _username;
            IdRequestDictionary = new Dictionary<string, List<string>>();
        }
    }
}