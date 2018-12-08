using System.Collections.Generic;

namespace KnowledgeGraph.RabbitMQModels
{
    public class RecommendedResourceWrapper
    {
        public string Username { get; set; }
        public List<string> ResourceIds { get; set; }
        public RecommendedResourceWrapper(string username)
        {
            Username  = username;
            ResourceIds = new List<string>();
        }
    }
}