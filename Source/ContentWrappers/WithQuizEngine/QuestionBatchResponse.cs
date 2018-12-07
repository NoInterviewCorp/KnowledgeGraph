using System.Collections.Generic;
using KnowledgeGraph.Models;
namespace KnowledgeGraph.RabbitMQModels
{
    public class GraphBatchResponse
    {
        public string Username { get; set; }
        public List<string> IdRequestList = new List<string>();

        public GraphBatchResponse()
        {
        }

        public GraphBatchResponse(string _username)
        {
            Username = _username;
        }
    }
}