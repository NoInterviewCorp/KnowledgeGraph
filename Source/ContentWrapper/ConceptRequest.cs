namespace KnowledgeGraph.RabbitMQModels 
{
    public class ConceptRequest 
    {
        public string Username {get;set;}
        public string Tech{get;set;}
        public ConceptRequest(string _username,string _tech)
        {
            Username = _username;
            Tech = _tech;
        }
    }
}