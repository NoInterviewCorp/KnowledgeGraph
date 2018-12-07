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
        public string ExchangeName { get; } = "KnowledgeGraphExchange";
        private List<QueueBinder> queueBinds;
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

            queueBinds = new List<QueueBinder>();

            queueBinds.Add(new QueueBinder("KnowledgeGraph_Contributer_Ids", "Request.Question"));
            queueBinds.Add(new QueueBinder("Contributer_KnowledgeGraph_LearningPlan", "Models.LearningPlan"));
            queueBinds.Add(new QueueBinder("Contributer_KnowledgeGraph_Resources", "Models.Resource"));
            queueBinds.Add(new QueueBinder("Contributer_QuizEngine_Questions", "Send.Question"));
            queueBinds.Add(new QueueBinder("Profile_KnowledgeGraph_User", "Users"));
            queueBinds.Add(new QueueBinder("Profile_KnowledgeGraph_LearningPlanRatingWrapper", "Send.LearningPlanRating"));
            queueBinds.Add(new QueueBinder("Profile_KnowledgeGraph_LearningPlanSubscriptionWrapper", "Send.LearningPlanSubscription"));
            queueBinds.Add(new QueueBinder("Profile_KnowledgeGraph_ResourceRatingWrapper", "Send.ResourceFeedBack"));
            queueBinds.Add(new QueueBinder("Profile_KnowledgeGraph_QuestionAmbiguityWrapper", "Send.QuestionFeedBack"));
            queueBinds.Add(new QueueBinder("QuizEngine_KnowledgeGraph_Concepts", "Question.Batch"));
            queueBinds.Add(new QueueBinder("QuizEngine_KnowledgeGraph_QuestionBatch", "Request.Concepts"));
            queueBinds.Add(new QueueBinder("Contributer_QuizEngine_Questions", "Send.Question"));
            queueBinds.Add(new QueueBinder("AverageRating_TotalSubs_Request", "Request.LP"));
            queueBinds.Add(new QueueBinder("AverageRating_TotalSubs_Response", "Response.LP"));
            queueBinds.Add(new QueueBinder("UserReport_Request","Request.Report"));
            queueBinds.Add(new QueueBinder("UserReport_Response","Response.Report"));
        }
        private void DeclareAndBindAQueue()
        {
            foreach (QueueBinder q in queueBinds)
            {
                Model.QueueDeclare(q.QueueName, false, false, false, null);
                Model.QueueBind(q.QueueName, ExchangeName, q.Routingkey);
            }
        }

        struct QueueBinder
        {
            public QueueBinder(string queueName, string routingkey)
            {
                QueueName = queueName;
                Routingkey = routingkey;
            }
            public string QueueName;
            public string Routingkey;
        }
    }
}
