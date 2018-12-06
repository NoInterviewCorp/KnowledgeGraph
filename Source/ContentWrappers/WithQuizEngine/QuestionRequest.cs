namespace KnowledgeGraph.RabbitMQModels
{
    public class QuestionRequest
    {
        public string Username { get; set; }
        public string Tech { get; set; }
        public string Concept;
        public QuestionRequest()
        {

        }
        public QuestionRequest(string _username, string _tech, string _concept)
        {
            Username = _username;
            Tech = _tech;
            Concept = _concept;
        }
    }
}