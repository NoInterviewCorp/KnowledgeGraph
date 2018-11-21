using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using KnowledgeGraph.Database.Models;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;

namespace KnowledgeGraph.Services {
    public class QueueBuilder {
        public static Question questions;
        private static ConnectionFactory _factory;
        public static IConnection _connection;
        public static IModel _model;
        private const string ExchangeNme = "KnowledgeExchange";
        public QueueBuilder () {
            _factory = new ConnectionFactory {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest",
                DispatchConsumersAsync = true
            };
            _connection = _factory.CreateConnection ();
            _model = _connection.CreateModel ();
            _model.ExchangeDeclare ("KnowldegeGraphExchange", "topic");
            _model.QueueDeclare ("Contributer_KnowledgeGraph", false, false, false, null);
            _model.QueueDeclare ("QuizEngine_KnowledgeGraph", false, false, false, null);
            _model.QueueDeclare ("KnowledgeGraph_IDs", false, false, false, null);
            _model.QueueDeclare ("Contributer_Questions", false, false, false, null);
            _model.QueueBind ("Contributer_KnowledgeGraph", ExchangeNme, "Models.LearningPlan");
            _model.QueueBind ("QuizEngine_KnowledgeGraph", ExchangeNme, "Models.Technology");
            _model.QueueBind ("KnowledgeGraph_IDs", ExchangeNme, "Models.QuestionId");
            _model.QueueBind ("Contributer_Questions", ExchangeNme, "Models.Queation");
        }
        public void GetQuestions (byte[] message, string RoutingKey) {
            IBasicProperties props = _model.CreateBasicProperties ();
            props.Expiration = "10000";
            _model.BasicPublish (ExchangeNme, RoutingKey, props, message);
        }
        public void FetchQuestions () {
            var channel = _connection.CreateModel ();
            var consumer = new AsyncEventingBasicConsumer (channel);
            consumer.Received += async (model, ea) => {
                var body = ea.Body;
                var json = Encoding.Default.GetString (body);
                questions = JsonConvert.DeserializeObject<Question>(json);
                var routingKey = ea.RoutingKey;
                Console.WriteLine (" - Routing Key <{0}>", routingKey);
                channel.BasicAck (ea.DeliveryTag, false);
                await Task.Yield ();
            };
        }
    }
}