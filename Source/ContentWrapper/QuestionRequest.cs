namespace KnowledgeGraph.RabbitMQModels {
    public class QuestionRequest {
        public string username { get; set; }
        public string tech { get; set; }
        public string concept;
        public QuestionRequest (string _username, string _tech, string _concept) {
            username = _username;
            tech = _tech;
            concept = _concept;
        }
    }
}