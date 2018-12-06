using System.Collections.Generic;

namespace KnowledgeGraph.RabbitMQModels
{
    public class QuestionIdsResponse
    {
        public string Username { get; set; }
        public List<string> IdRequestList = new List<string>();
        public QuestionIdsResponse()
        {

        }
        public QuestionIdsResponse(string _username)
        {
            Username = _username;
        }
    }
}