namespace KnowledgeGraph.RabbitMQModels 
{
    public class ConceptRequest 
    {
        public string username {get;set;}
        public string tech{get;set;}
        public ConceptRequest(string _username,string _tech)
        {
            username = _username;
            tech = _tech;
        }
    }
}