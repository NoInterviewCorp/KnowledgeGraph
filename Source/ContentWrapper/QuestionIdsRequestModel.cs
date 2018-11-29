namespace KnowledgeGraph.RabbitMQModels {
    public class QuestionIdsRequestModel {
        public string username { get; set; }
        public string tech { get; set; }
        public string concept;
        public QuestionIdsRequestModel (string _username, string _tech, string _concept) {
            username = _username;
            tech = _tech;
            concept = _concept;
        }
    }
}