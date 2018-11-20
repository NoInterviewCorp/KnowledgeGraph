using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
namespace KnowledgeGraph.Services {
    public class QueueBuilder {
        private static ConnectionFactory _factory;
        public static IConnection _connection;
        public static IModel _model;
        private const string ExchangeNme = "LearningPlanExchange";
        public QueueBuilder () {
            _factory = new ConnectionFactory {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest",
                DispatchConsumersAsync = true
            };
            _connection = _factory.CreateConnection ();
            _model = _connection.CreateModel ();
            _model.ExchangeDeclare ("KnowldegeGraphExchange", "DataExchange");
            _model.QueueDeclare ("Contributer_KnowledgeGraph", false, false, false, null);
            _model.QueueDeclare ("QuizEngine_KnowledgeGraph", false, false, false, null);
            _model.QueueDeclare ("KnowledgeGraph_IDs", false, false, false, null);
            _model.QueueDeclare ("Contributer_Questions", false, false, false, null);
            _model.QueueBind ("Contributer_KnowledgeGraph", "KnowldegeGraphExchange","Models.LearningPlan");
        }
    }
}