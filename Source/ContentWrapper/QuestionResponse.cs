using System.Collections.Generic;

namespace KnowledgeGraph.RabbitMQModels
{
    public class QuestionIdsResponseModel
    {
        public string username { get; set; } 
        public Dictionary<string,List<string>> questionids;
        public QuestionIdsResponseModel(string _username)
        {
            username = _username;
            questionids.Clear();
        }
    }
}