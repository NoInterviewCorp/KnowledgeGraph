using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using KnowledgeGraph.Database;
using KnowledgeGraph.Database.Models;
using KnowledgeGraph.Database.Persistence;
// using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace KnowledgeGraph.Services
{
    public class QueueHandler
    {
        public QueueBuilder queues;
        // private LearningPlanWrapper learningPlan;
        // private ResourceWrapper resource;
        private QuizEngineQuery query;
        private IGraphFunctions graphfunctions;
        private List<int> IDs;

        public QueueHandler(QueueBuilder _queues, IGraphFunctions _graphfunctions)
        {
            queues = _queues;
            graphfunctions = _graphfunctions;
            this.HandleLearningPlanFromQueue();
            this.HandleResourceFromQueue();
            this.QuizEngineQueueHandler();
        }

        public void HandleLearningPlanFromQueue()
        {
            var channel = queues.connection.CreateModel();
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                Console.WriteLine("-----------------------------------------------------------------------");
                Console.WriteLine("Consuming from the queue");
                var body = ea.Body;
                var  learningPlan = (LearningPlanWrapper)body.DeSerialize(typeof(LearningPlanWrapper));
                await graphfunctions.CreateLearningPlanAndRelationshipsAsync(learningPlan);
                var routingKey = ea.RoutingKey;
                channel.BasicAck(ea.DeliveryTag, false);
                Console.WriteLine("-----------------------------------------------------------------------");
                Console.WriteLine(" - Routing Key <{0}>", routingKey);
                await Task.Yield();
            };
            Console.WriteLine("Consuming from Contributor's Knowledge Graph");
            channel.BasicConsume("Contributer_KnowledgeGraph_LearningPlan", false, consumer);
        }

        public void HandleResourceFromQueue()
        {
            var channel = queues.connection.CreateModel();
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                Console.WriteLine("Consuming from the queue");
                Console.WriteLine("-----------------------------------------------------------------------");
                var body = ea.Body;
                var resource = (ResourceWrapper)body.DeSerialize(typeof(ResourceWrapper));
                await graphfunctions.CreateResourceAndRelationships(resource);
                var routingKey = ea.RoutingKey;
                channel.BasicAck(ea.DeliveryTag, false);
                Console.WriteLine("-----------------------------------------------------------------------");
                Console.WriteLine(" - Routing Key <{0}>", routingKey);
                await Task.Yield();
            };
            Console.WriteLine("Consuming from Contributor's Knowledge Graph");
            channel.BasicConsume("Contributer_KnowledgeGraph_Resource", false, consumer);
        }

        public void QuizEngineQueueHandler()
        {
            var channel = queues.connection.CreateModel();
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                Console.WriteLine("Recieved Request for Questions");
                var body = ea.Body;
                query = (QuizEngineQuery)body.DeSerialize(typeof(QuizEngineQuery));
                IDs.Clear();
                IDs.AddRange(graphfunctions.GetQuestionIds(query.tech, query.username));
                channel.BasicAck(ea.DeliveryTag, false);
                channel.BasicPublish("KnowldegeGraphExchange", "Models.QuestionId", null, IDs.Serialize());
                var routingKey = ea.RoutingKey;
                Console.WriteLine(" - Routing Key <{0}>", routingKey);
                Console.WriteLine("- Delivery Tag <{0}>", ea.DeliveryTag);
                await Task.Yield();
            };
            channel.BasicConsume("QuizEngine_KnowledgeGraph", false, consumer);
        }
    }
}