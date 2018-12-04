using System.Collections.Generic;
using KnowledgeGraph.Models;
using RabbitMQ.Client;

namespace KnowledgeGraph.Services
{
    public class QueueBuilder
    {
        public List<Question> questions;
        private ConnectionFactory _factory;
        public IConnection connection;
        public IModel Model;
        public const string ExchangeNme = "KnowldegeGraphExchange";
        public QueueBuilder()
        {
            _factory = new ConnectionFactory
            {
                HostName = "rabbitmq",
                UserName = "achausername",
                Password = "strongpassword",
                DispatchConsumersAsync = true
            };
            connection = _factory.CreateConnection();
            Model = connection.CreateModel();
            Model.ExchangeDeclare("KnowldegeGraphExchange", "topic");

            Model.QueueDeclare("KnowledgeGraph_Contributer_Ids", false, false, false, null);
            Model.QueueDeclare("Contributer_KnowledgeGraph_LearningPlan", false, false, false, null);
            Model.QueueDeclare("Contributer_KnowledgeGraph_Resources", false, false, false, null);
            Model.QueueDeclare("Contributer_QuizEngine_Questions", false, false, false, null);
            Model.QueueDeclare("Profile_KnowledgeGraph_User", false, false, false, null);
            Model.QueueDeclare("Profile_KnowledgeGraph_LearningPlanRatingWrapper", false, false, false, null);
            Model.QueueDeclare("Profile_KnowledgeGraph_LearningPlanSubscriptionWrapper", false, false, false, null);
           // Model.QueueDeclare("Profile_KnowledgeGraph_ResourceRatingWrapper", false, false, false, null);
           // Model.QueueDeclare("Profile_KnowledgeGraph_QuestionAmbiguityWrapper", false, false, false, null);
            Model.QueueDeclare("QuizEngine_KnowledgeGraph_Concepts", false, false, false, null);
            Model.QueueDeclare("QuizEngine_KnowledgeGraph_QuestionBatch", false, false, false, null);

            Model.QueueBind("KnowledgeGraph_Contributer_Ids", ExchangeNme, "Request.Question");
            Model.QueueBind("Contributer_KnowledgeGraph_LearningPlan", ExchangeNme, "Models.LearningPlan");
            Model.QueueBind("Contributer_KnowledgeGraph_Resources", ExchangeNme, "Models.Resource");
            Model.QueueBind("Contributer_QuizEngine_Questions", ExchangeNme, "Send.Question");
            Model.QueueBind("Profile_KnowledgeGraph_User", "KnowldegeGraphExchange", "Users");
            Model.QueueBind("Profile_KnowledgeGraph_LearningPlanRatingWrapper", ExchangeNme, "Send.LearningPlanRating");
            Model.QueueBind("Profile_KnowledgeGraph_LearningPlanSubscriptionWrapper", ExchangeNme, "Send.LearningPlanSubscription");
          //  Model.QueueBind("Profile_KnowledgeGraph_ResourceRatingWrapper", ExchangeNme, "Send.ResourceFeedBack");
          //  Model.QueueBind("Profile_KnowledgeGraph_QuestionAmbiguityWrapper", ExchangeNme, "Send.QuestionFeedBack");
            Model.QueueBind("QuizEngine_KnowledgeGraph_QuestionBatch", ExchangeNme, "Question.Batch");
            Model.QueueBind("QuizEngine_KnowledgeGraph_Concepts", ExchangeNme, "Request.Concepts");
        }
    }
}