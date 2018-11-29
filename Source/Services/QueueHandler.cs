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
        private QuestionIdsBatchRequestModel batch_query;
        private QuestionIdsRequestModel concept_query;
        private IGraphFunctions graphfunctions;
        private QuestionIdsResponseModel questionidlist;
        private QuestionIdsBatchResponseModel questionidbatchlist;
        public QueueHandler (QueueBuilder _queues, IGraphFunctions _graphfunctions) {
            queues = _queues;
            graphfunctions = _graphfunctions;
            this.ContributerLearningPlanQueueHandler ();
            this.QuestionBatchRequestHandler ();
        }

        public void ContributerLearningPlanQueueHandler () {
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
        public void QuestionBatchRequestHandler () {
            var channel = queues.connection.CreateModel ();
            var consumer = new AsyncEventingBasicConsumer (channel);
            consumer.Received += async (model, ea) => {
                Console.WriteLine ("Recieved Request for Questions");
                var body = ea.Body;
                batch_query = (QuestionIdsBatchRequestModel) body.DeSerialize (typeof (QuestionIdsBatchRequestModel));
                this.questionidbatchlist = new QuestionIdsBatchResponseModel (batch_query.username);
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
                concept_query = (QuestionIdsRequestModel) body.DeSerialize (typeof (QuestionIdsRequestModel));
                this.questionidlist = new QuestionIdsResponseModel (batch_query.username);
                this.questionidlist.questionids = (graphfunctions.GetQuestionIds (concept_query.username, concept_query.tech, concept_query.concept));
                channel.BasicAck (ea.DeliveryTag, false);
                channel.BasicPublish ("KnowldegeGraphExchange", "Routing Key", null, this.questionidlist.Serialize ());
                var routingKey = ea.RoutingKey;
                Console.WriteLine (" - Routing Key <{0}>", routingKey);
                Console.WriteLine ("- Delivery Tag <{0}>", ea.DeliveryTag);
                await Task.Yield ();
            };
            channel.BasicConsume ("QuizEngine_KnowledgeGraph", false, consumer);
        }
    }
}