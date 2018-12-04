using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KnowledgeGraph.Models;
using KnowledgeGraph.Persistence;
using KnowledgeGraph.RabbitMQModels;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace KnowledgeGraph.Services {
    public class QueueHandler {
        public QueueBuilder queues;
        private LearningPlan learningPlan;
        private QuestionBatchRequest batch_query;
        private QuestionRequest question_query;
        private ConceptRequest concept_query;
        private ConceptResponse concept_list;
        private IGraphFunctions graphfunctions;
        private QuestionIdsResponse questionidlist;
        private QuestionBatchResponse questionidbatchlist;
        public QueueHandler (QueueBuilder _queues, IGraphFunctions _graphfunctions) {
            queues = _queues;
            graphfunctions = _graphfunctions;
            this.QuestionBatchRequestHandler();
            this.QuestionRequestHandler();
            this.QuestionBatchRequestHandler ();
        }
        public void QuestionBatchRequestHandler () {
            var channel = queues.connection.CreateModel ();
            var consumer = new AsyncEventingBasicConsumer (channel);
            consumer.Received += async (model, ea) => {
                Console.WriteLine ("Recieved Request for Questions");
                var body = ea.Body;
                batch_query = (QuestionBatchRequest) body.DeSerialize (typeof (QuestionBatchRequest));
                this.questionidbatchlist = new QuestionBatchResponse (batch_query.username);
                this.questionidbatchlist.questionids = (graphfunctions.GetQuestionBatchIds (batch_query.username, batch_query.tech, batch_query.concepts));
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
                this.questionidlist = new QuestionIdsResponse (batch_query.username);
                this.questionidlist.questionids = (graphfunctions.GetQuestionIds (question_query.username, question_query.tech, question_query.concept));
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
                this.concept_list = new ConceptResponse(concept_query.username);
                this.concept_list.concepts.AddRange (graphfunctions.GetConceptFromTechnology(concept_query.tech));
                channel.BasicAck (ea.DeliveryTag, false);
                channel.BasicPublish ("KnowldegeGraphExchange", "Routing Key", null, this.concept_list.Serialize ());
                var routingKey = ea.RoutingKey;
                Console.WriteLine (" - Routing Key <{0}>", routingKey);
                Console.WriteLine ("- Delivery Tag <{0}>", ea.DeliveryTag);
                await Task.Yield ();
            };
            channel.BasicConsume ("QuizEngine_KnowledgeGraph", false, consumer);
        }
    }
}