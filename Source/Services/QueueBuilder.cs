using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using KnowledgeGraph.Models;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace KnowledgeGraph.Services {
    public class QueueBuilder {
        public List<Question> questions;
        private ConnectionFactory _factory;
        public IConnection connection;
        public IModel model;
        public const string ExchangeNme = "KnowldegeGraphExchange";
        public QueueBuilder ()
        {
            _factory = new ConnectionFactory {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest",
                DispatchConsumersAsync = true
            };
            connection = _factory.CreateConnection ();
            model = connection.CreateModel ();
            model.ExchangeDeclare ("KnowldegeGraphExchange", "topic");
            model.QueueDeclare ("Contributer_KnowledgeGraph_LearningPlan", false, false, false, null);
            model.QueueDeclare ("Contributer_KnowledgeGraph_Resources", false, false, false, null);
            model.QueueDeclare ("Contributer_QuizEngine_Questions", false, false, false, null);
            model.QueueDeclare ("QuizEngine_KnowledgeGraph_Query", false, false, false, null);
            model.QueueDeclare ("QuizEngine_KnowledgeGraph_QuizUpdate", false, false, false, null);
            model.QueueDeclare ("KnowledgeGraph_Contributer_Ids", false, false, false, null);
            model.QueueDeclare ("QuizEngine_UserProfile_UserData", false, false, false, null);
            model.QueueBind ("Contributer_KnowledgeGraph_LearningPlan", ExchangeNme, "AddLearningPlan");
            model.QueueBind ("QuizEngine_KnowledgeGraph", ExchangeNme, "Models.Technology");
            model.QueueBind ("KnowledgeGraph_IDs", ExchangeNme, "Models.QuestionId");
            model.QueueBind ("Contributer_Questions", ExchangeNme, "Models.Queation");
        }
    }
}