using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using KnowledgeGraph.Database.Models;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;

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
            Model.QueueDeclare("Contributer_KnowledgeGraph_LearningPlan", false, false, false, null);
            Model.QueueDeclare("Contributer_KnowledgeGraph_Resources", false, false, false, null);
            Model.QueueDeclare("Contributer_QuizEngine_Questions", false, false, false, null);
            Model.QueueDeclare("KnowledgeGraph_Contributer_Ids", false, false, false, null);
            // remove routing keys
            Model.QueueBind("Contributer_KnowledgeGraph_LearningPlan", ExchangeNme, "Models.LearningPlan");
            Model.QueueBind("Contributer_KnowledgeGraph_Resources", ExchangeNme, "Models.Resource");
            Model.QueueBind("Contributer_QuizEngine_Questions", ExchangeNme, "Send.Question");
            Model.QueueBind("KnowledgeGraph_Contributer_Ids", ExchangeNme, "Request.Question");
        }
    }
}
