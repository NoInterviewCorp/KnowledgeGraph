namespace KnowledgeGraph.RabbitMQModels 
{
    public class ConceptRequestModel 
    {
        public string username {get;set;}
        public string tech{get;set;}
        public ConceptRequestModel(string _username,string _tech)
        {
            username = _username;
            tech = _tech;
        }
    }
}