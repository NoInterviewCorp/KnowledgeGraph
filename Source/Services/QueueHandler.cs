using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KnowledgeGraph.Database;
using KnowledgeGraph.Database.Persistence;
using KnowledgeGraph.Models;
using KnowledgeGraph.RabbitMQModels;
// using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace KnowledgeGraph.Services
{
    public class QueueHandler {
        public QueueBuilder queues;
        // private LearningPlanWrapper learningPlan;
        // private ResourceWrapper resource;
        private QuizEngineQuery query;
        private IGraphFunctions graphfunctions;
        private List<int> IDs;
        private LearningPlan learningPlan;
        private QuestionBatchRequest batch_query;
        private QuestionRequest question_query;
        private ConceptRequest concept_query;
        private ConceptResponse concept_list;
        private QuestionIdsResponse questionidlist;
        private GraphBatchResponse questionidbatchlist;

        public QueueHandler (QueueBuilder _queues, IGraphFunctions _graphfunctions) {
            queues = _queues;
            graphfunctions = _graphfunctions;
            this.HandleLearningPlanFromQueue ();
            this.HandleResourceFromQueue ();
            //  this.QuizEngineQueueHandler();
        }

        public void HandleLearningPlanFromQueue () {
            var channel = queues.connection.CreateModel ();
            var consumer = new AsyncEventingBasicConsumer (channel);
            consumer.Received += async (model, ea) => {
                Console.WriteLine ("-----------------------------------------------------------------------");
                Console.WriteLine ("Consuming from the queue");
                var body = ea.Body;
                var learningPlan = (LearningPlanWrapper) body.DeSerialize (typeof (LearningPlanWrapper));
                await graphfunctions.CreateLearningPlanAndRelationshipsAsync (learningPlan);
                var routingKey = ea.RoutingKey;
                channel.BasicAck (ea.DeliveryTag, false);
                Console.WriteLine ("-----------------------------------------------------------------------");
                Console.WriteLine (" - Routing Key <{0}>", routingKey);
                await Task.Yield ();
            };
            Console.WriteLine ("Consuming from Contributor's Knowledge Graph");
            channel.BasicConsume ("Contributer_KnowledgeGraph_LearningPlan", false, consumer);
        }

        public void HandleResourceFromQueue () {
            var channel = queues.connection.CreateModel ();
            var consumer = new AsyncEventingBasicConsumer (channel);
            consumer.Received += async (model, ea) => {
                Console.WriteLine ("Consuming from the queue");
                Console.WriteLine ("-----------------------------------------------------------------------");
                var body = ea.Body;
                var resource = (ResourceWrapper) body.DeSerialize (typeof (ResourceWrapper));
                await graphfunctions.CreateResourceAndRelationships (resource);
                var routingKey = ea.RoutingKey;
                channel.BasicAck (ea.DeliveryTag, false);
                Console.WriteLine ("-----------------------------------------------------------------------");
                Console.WriteLine (" - Routing Key <{0}>", routingKey);
                await Task.Yield ();
            };
            Console.WriteLine ("Consuming from Contributor's Knowledge Graph");
            channel.BasicConsume ("Contributer_KnowledgeGraph_Resources", false, consumer);
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
            channel.BasicConsume ("QuizEngine_KnowledgeGraph_QuestionBatch", false, consumer);
        }

        public void QuestionBatchRequestHandler () {
            var channel = queues.connection.CreateModel ();
            var consumer = new AsyncEventingBasicConsumer (channel);
            consumer.Received += async (model, ea) => {
                Console.WriteLine ("Recieved Request for Questions");
                var body = ea.Body;
                batch_query = (QuestionBatchRequest) body.DeSerialize (typeof (QuestionBatchRequest));
                this.questionidbatchlist = new GraphBatchResponse (batch_query.Username);
                this.questionidbatchlist.questionids = (graphfunctions.GetQuestionBatchIds (batch_query.Username, batch_query.Tech, batch_query.Concepts));
                channel.BasicAck (ea.DeliveryTag, false);
                channel.BasicPublish ("KnowldegeGraphExchange", "Models.QuestionId", null, this.questionidbatchlist.Serialize ());
                var routingKey = ea.RoutingKey;
                Console.WriteLine (" - Routing Key <{0}>", routingKey);
                Console.WriteLine ("- Delivery Tag <{0}>", ea.DeliveryTag);
                await Task.Yield ();
            };
            channel.BasicConsume ("QuizEngine_KnowledgeGraph", false, consumer);
        }
        public void QuestionRequestHandler () {
            var channel = queues.connection.CreateModel ();
            var consumer = new AsyncEventingBasicConsumer (channel);
            consumer.Received += async (model, ea) => {
                Console.WriteLine ("Recieved Request for Questions");
                var body = ea.Body;
                question_query = (QuestionRequest) body.DeSerialize (typeof (QuestionRequest));
                this.questionidlist = new QuestionIdsResponse (batch_query.Username);
                this.questionidlist.IdRequestDictionary = (graphfunctions.GetQuestionIds (question_query.Username, question_query.Tech, question_query.Concept));
                channel.BasicAck (ea.DeliveryTag, false);
                channel.BasicPublish ("KnowldegeGraphExchange", "Routing Key", null, this.questionidlist.Serialize ());
                var routingKey = ea.RoutingKey;
                Console.WriteLine (" - Routing Key <{0}>", routingKey);
                Console.WriteLine ("- Delivery Tag <{0}>", ea.DeliveryTag);
                await Task.Yield ();
            };
            channel.BasicConsume ("QuizEngine_KnowledgeGraph", false, consumer);
        }
        public void ConceptRequestHandler () {
            var channel = queues.connection.CreateModel ();
            var consumer = new AsyncEventingBasicConsumer (channel);
            consumer.Received += async (model, ea) => {
                Console.WriteLine ("Recieved Request for Concepts");
                var body = ea.Body;
                concept_query = (ConceptRequest) body.DeSerialize (typeof (ConceptRequest));
                this.concept_list = new ConceptResponse (concept_query.Username);
                this.concept_list.concepts.AddRange (graphfunctions.GetConceptFromTechnology (concept_query.Tech));
                channel.BasicAck (ea.DeliveryTag, false);
                channel.BasicPublish ("KnowldegeGraphExchange", "Routing Key", null, this.concept_list.Serialize ());
                var routingKey = ea.RoutingKey;
                Console.WriteLine (" - Routing Key <{0}>", routingKey);
                Console.WriteLine ("- Delivery Tag <{0}>", ea.DeliveryTag);
                await Task.Yield ();
            };
            channel.BasicConsume ("QuizEngine_KnowledgeGraph_Concepts", false, consumer);
        }
    }
}