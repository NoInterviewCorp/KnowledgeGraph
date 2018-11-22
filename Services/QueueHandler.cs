using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using KnowledgeGraph.Database.Models;
using KnowledgeGraph.Database.Persistence;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace KnowledgeGraph.Services {
    public class QueueHandler {
        public QueueBuilder queues;
        private LearningPlan learningPlan;
        private QuizEngineQuery query;
        private IGraphFunctions graphfunctions;
        private List<int> IDs;
        public QueueHandler (QueueBuilder _queues, IGraphFunctions _graphfunctions) {
            queues = _queues;
            graphfunctions = _graphfunctions;
            this.ContributerQueueHandler ();
            this.QuizEngineQueueHandler ();
        }

        public void ContributerQueueHandler () {
            var channel = queues.connection.CreateModel ();
            var consumer = new AsyncEventingBasicConsumer (channel);
            consumer.Received += async (model, ea) => {
                Console.WriteLine ("Consuming from the queue");
                channel.BasicAck (ea.DeliveryTag, false);
                var body = ea.Body;
                learningPlan = (LearningPlan) body.DeSerialize (typeof (LearningPlan));
                graphfunctions.CreateLearningPlanAndRelationships (learningPlan);
                var routingKey = ea.RoutingKey;
                Console.WriteLine (" - Routing Key <{0}>", routingKey);
                await Task.Yield ();
            };
            Console.WriteLine ("Consuming from Contributor's Knowledge Graph");
            channel.BasicConsume ("Contributer_KnowledgeGraph", false, consumer);
        }
        public void QuizEngineQueueHandler () {
            var channel = queues.connection.CreateModel ();
            var consumer = new AsyncEventingBasicConsumer (channel);
            consumer.Received += async (model, ea) => {
                Console.WriteLine ("Recieved Request for Questions");
                var body = ea.Body;
                query = (QuizEngineQuery) body.DeSerialize (typeof (QuizEngineQuery));
                IDs.Clear ();
                IDs.AddRange (graphfunctions.GetQuestionIds (query.tech, query.username));
                channel.BasicAck (ea.DeliveryTag, false);
                channel.BasicPublish ("KnowldegeGraphExchange", "Models.QuestionId", null, IDs.Serialize ());
                var routingKey = ea.RoutingKey;
                Console.WriteLine (" - Routing Key <{0}>", routingKey);
                Console.WriteLine ("- Delivery Tag <{0}>", ea.DeliveryTag);
                await Task.Yield ();
            };
            channel.BasicConsume ("QuizEngine_KnowledgeGraph", false, consumer);
        }
    }
}