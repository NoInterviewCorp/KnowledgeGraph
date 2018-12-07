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
        public const string ExchangeName = "KnowledgeGraphExchange";
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
            Model.ExchangeDeclare(ExchangeName, "topic");

            Model.QueueDeclare("QuizEngine_KnowledgeGraph_Result", false, false, false, null);
            Model.QueueDeclare("Contributer_KnowledgeGraph_LearningPlan", false, false, false, null);
            Model.QueueDeclare("Contributer_KnowledgeGraph_Resources", false, false, false, null);
            Model.QueueDeclare("Contributer_QuizEngine_Questions", false, false, false, null);
            Model.QueueDeclare("Profile_KnowledgeGraph_User", false, false, false, null);
            Model.QueueDeclare("Profile_KnowledgeGraph_LearningPlanRatingWrapper", false, false, false, null);
            Model.QueueDeclare("Profile_KnowledgeGraph_LearningPlanSubscriptionWrapper", false, false, false, null);
            Model.QueueDeclare("Profile_KnowledgeGraph_ResourceRatingWrapper", false, false, false, null);
            Model.QueueDeclare("Profile_KnowledgeGraph_QuestionAmbiguityWrapper", false, false, false, null);
            Model.QueueDeclare("QuizEngine_KnowledgeGraph_QuestionBatch", false, false, false, null);
            Model.QueueDeclare("QuizEngine_Profile_QuizData", false, false, false, null);
            Model.QueueDeclare("KnowledgeGraph_Contributer_Ids", false, false, false, null);

            Model.QueueBind("KnowledgeGraph_Contributer_Ids", ExchangeName, "Request.Question");
            Model.QueueBind("Contributer_KnowledgeGraph_LearningPlan", ExchangeName, "Models.LearningPlan");
            Model.QueueBind("Contributer_KnowledgeGraph_Resources", ExchangeName, "Models.Resource");
            Model.QueueBind("Contributer_QuizEngine_Questions", ExchangeName, "Send.Question");
            Model.QueueBind("Profile_KnowledgeGraph_User", "KnowledgeGraphExchange", "Users");
            Model.QueueBind("Profile_KnowledgeGraph_LearningPlanRatingWrapper", ExchangeName, "Send.LearningPlanRating");
            Model.QueueBind("Profile_KnowledgeGraph_LearningPlanSubscriptionWrapper", ExchangeName, "Send.LearningPlanSubscription");
            Model.QueueBind("Profile_KnowledgeGraph_ResourceRatingWrapper", ExchangeName, "Send.ResourceFeedBack");
            Model.QueueBind("Profile_KnowledgeGraph_QuestionAmbiguityWrapper", ExchangeName, "Send.QuestionFeedBack");
            Model.QueueBind("QuizEngine_KnowledgeGraph_QuestionBatch", ExchangeName, "Question.Batch");
            Model.QueueBind("QuizEngine_KnowledgeGraph_Result", ExchangeName, "Result.Update");
            Model.QueueBind("QuizEngine_Profile_QuizData",ExchangeName,"User.QuizData");
        }
    }
}
