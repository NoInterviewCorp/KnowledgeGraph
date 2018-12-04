using System.Collections.Generic;

namespace KnowledgeGraph.RabbitMQModels
{
    public class QuestionIdsResponse
    {
        public string username { get; set; } 
        public Dictionary<string,List<string>> questionids;
        public QuestionIdsResponse(string _username)
        {
            username = _username;
            questionids.Clear();
        }
    }
}