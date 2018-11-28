namespace KnowledgeGraph.RabbitMQModels {
    public class QuestionsRequestModel {
        public string username { get; set; }
        public string tech { get; set; }
        public string concept;
        public QuestionsRequestModel (string _username, string _tech, string _concept) {
            username = _username;
            tech = _tech;
            concept = _concept;
        }
    }
}