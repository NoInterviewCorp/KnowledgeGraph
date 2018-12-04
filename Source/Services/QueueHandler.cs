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
        private QuestionBatchResponse questionidbatchlist;

        public QueueHandler (QueueBuilder _queues, IGraphFunctions _graphfunctions) {
            queues = _queues;
            graphfunctions = _graphfunctions;
            this.HandleLearningPlanFromQueue ();
            this.HandleResourceFromQueue ();
            this.ListenForUser();
            this.ListenForLeaningPlanFeedBack();
            this.ListenForResourceFeedBack();
            this.ListenForLeaningPlanSubscriber();
            this.ListenForLeaningPlanUnSubscriber();
            this.ListenForQuestionFeedBack();
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
            channel.BasicConsume ("QuizEngine_KnowledgeGraph", false, consumer);
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
                this.concept_list = new ConceptResponse (concept_query.username);
                this.concept_list.concepts.AddRange (graphfunctions.GetConceptFromTechnology (concept_query.tech));
                channel.BasicAck (ea.DeliveryTag, false);
                channel.BasicPublish ("KnowldegeGraphExchange", "Routing Key", null, this.concept_list.Serialize ());
                var routingKey = ea.RoutingKey;
                Console.WriteLine (" - Routing Key <{0}>", routingKey);
                Console.WriteLine ("- Delivery Tag <{0}>", ea.DeliveryTag);
                await Task.Yield ();
            };
            channel.BasicConsume ("QuizEngine_KnowledgeGraph", false, consumer);
        }
    public void ListenForUser()
        {
            var channel = queues.connection.CreateModel();
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                Console.WriteLine("Consuming from the queue");
                Console.WriteLine("-----------------------------------------------------------------------");

                var body = ea.Body;
                  var user = (User)body.DeSerialize(typeof(User));
               // var message = Encoding.UTF8.GetString(body);
               // var user = JsonConvert.DeserializeObject<User>(message);
                Console.WriteLine("User Name is {0} " + user.FullName);
                var routingKey = ea.RoutingKey;
                channel.BasicAck(ea.DeliveryTag, false);
                Console.WriteLine("-----------------------------------------------------------------------");
                Console.WriteLine(" - Routing Key <{0}>", routingKey);
                await Task.Yield();


            };
            Console.WriteLine("Consuming from Profile's Knowledge Graph");
            channel.BasicConsume("Profile_KnowledgeGraph_User", false, consumer);
        }
        public void ListenForLeaningPlanFeedBack()
        {
            var channel = queues.connection.CreateModel();
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                Console.WriteLine("Consuming from the queue");
                Console.WriteLine("-----------------------------------------------------------------------");
                try
                {
                    var body = ea.Body;
                    //  var user = (User)body.DeSerialize(typeof(User));
                    var learningPlanFeedBack = (LearningPlanFeedBack)body.DeSerialize(typeof(LearningPlanFeedBack));
                    await graphfunctions.RatingLearningPlanAndRelationshipsAsync(learningPlanFeedBack);
                    // var message = Encoding.UTF8.GetString(body);
                    //  var LP = JsonConvert.DeserializeObject<LearningPlanFeedBack>(message);
                    Console.WriteLine("User Name is {0} ");
                    var routingKey = ea.RoutingKey;
                    channel.BasicAck(ea.DeliveryTag, false);
                    Console.WriteLine("-----------------------------------------------------------------------");
                    Console.WriteLine(" - Routing Key <{0}>", routingKey);
                    await Task.Yield();
                }
                catch (Exception e)
                {
                    Console.WriteLine("----------------------EXCEPTION-MESSAGE------------------------------------");
                    Console.WriteLine(e.Message);
                    Console.WriteLine("----------------------STACK-TRACE-----------------------------------------");
                    Console.WriteLine(e.StackTrace);
                    Console.WriteLine("-------------------------INNER-EXCEPTION-----------------------------");
                    Console.WriteLine(e.InnerException);
                    // return null;
                }

            };

            Console.WriteLine("Consuming from Profile's Knowledge Graph");
            channel.BasicConsume("Profile_KnowledgeGraph_LearningPlanFeedBack", false, consumer);
        }
        public void ListenForResourceFeedBack()
        {
            var channel = queues.connection.CreateModel();
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                Console.WriteLine("Consuming from the queue");
                Console.WriteLine("-----------------------------------------------------------------------");
                try
                {
                    var body = ea.Body;
                    //  var user = (User)body.DeSerialize(typeof(User));
                    var resourceFeedBack = (ResourceFeedBack)body.DeSerialize(typeof(ResourceFeedBack));
                    await graphfunctions.RatingResourceAndRelationshipsAsync(resourceFeedBack);
                    // var message = Encoding.UTF8.GetString(body);
                    //  var LP = JsonConvert.DeserializeObject<LearningPlanFeedBack>(message);
                    Console.WriteLine("User Name is {0} ");
                    var routingKey = ea.RoutingKey;
                    channel.BasicAck(ea.DeliveryTag, false);
                    Console.WriteLine("-----------------------------------------------------------------------");
                    Console.WriteLine(" - Routing Key <{0}>", routingKey);
                    await Task.Yield();
                }
                catch (Exception e)
                {
                    Console.WriteLine("----------------------EXCEPTION-MESSAGE------------------------------------");
                    Console.WriteLine(e.Message);
                    Console.WriteLine("----------------------STACK-TRACE-----------------------------------------");
                    Console.WriteLine(e.StackTrace);
                    Console.WriteLine("-------------------------INNER-EXCEPTION-----------------------------");
                    Console.WriteLine(e.InnerException);
                    // return null;
                }

            };

            Console.WriteLine("Consuming from Profile's Knowledge Graph");
            channel.BasicConsume("Profile_KnowledgeGraph_ResourceFeedBack", false, consumer);

        }
        public void ListenForLeaningPlanSubscriber()
        {
            var channel = queues.connection.CreateModel();
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                Console.WriteLine("Consuming from the queue");
                Console.WriteLine("-----------------------------------------------------------------------");
                try
                {
                    var body = ea.Body;
                    //  var user = (User)body.DeSerialize(typeof(User));
                    var learningPlanFeedBack = (LearningPlanFeedBack)body.DeSerialize(typeof(LearningPlanFeedBack));
                    await graphfunctions.SubscribeLearningPlanAndRelationshipsAsync(learningPlanFeedBack);
                    // var message = Encoding.UTF8.GetString(body);
                    //  var LP = JsonConvert.DeserializeObject<LearningPlanFeedBack>(message);
                    Console.WriteLine("User Name is {0} ");
                    var routingKey = ea.RoutingKey;
                    channel.BasicAck(ea.DeliveryTag, false);
                    Console.WriteLine("-----------------------------------------------------------------------");
                    Console.WriteLine(" - Routing Key <{0}>", routingKey);
                    await Task.Yield();
                }
                catch (Exception e)
                {
                    Console.WriteLine("----------------------EXCEPTION-MESSAGE------------------------------------");
                    Console.WriteLine(e.Message);
                    Console.WriteLine("----------------------STACK-TRACE-----------------------------------------");
                    Console.WriteLine(e.StackTrace);
                    Console.WriteLine("-------------------------INNER-EXCEPTION-----------------------------");
                    Console.WriteLine(e.InnerException);
                    // return null;
                }

            };

            Console.WriteLine("Consuming from Profile's Knowledge Graph");
            channel.BasicConsume("Profile_KnowledgeGraph_LearningPlanFeedBack", false, consumer);
        }
         public void ListenForLeaningPlanUnSubscriber()
        {
            var channel = queues.connection.CreateModel();
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                Console.WriteLine("Consuming from the queue");
                Console.WriteLine("-----------------------------------------------------------------------");
                try
                {
                    var body = ea.Body;
                    //  var user = (User)body.DeSerialize(typeof(User));
                    var learningPlanFeedBack = (LearningPlanFeedBack)body.DeSerialize(typeof(LearningPlanFeedBack));
                    await graphfunctions.UnSubscribeLearningPlanAndRelationshipsAsync(learningPlanFeedBack);
                    // var message = Encoding.UTF8.GetString(body);
                    //  var LP = JsonConvert.DeserializeObject<LearningPlanFeedBack>(message);
                    Console.WriteLine("User Name is {0} ");
                    var routingKey = ea.RoutingKey;
                    channel.BasicAck(ea.DeliveryTag, false);
                    Console.WriteLine("-----------------------------------------------------------------------");
                    Console.WriteLine(" - Routing Key <{0}>", routingKey);
                    await Task.Yield();
                }
                catch (Exception e)
                {
                    Console.WriteLine("----------------------EXCEPTION-MESSAGE------------------------------------");
                    Console.WriteLine(e.Message);
                    Console.WriteLine("----------------------STACK-TRACE-----------------------------------------");
                    Console.WriteLine(e.StackTrace);
                    Console.WriteLine("-------------------------INNER-EXCEPTION-----------------------------");
                    Console.WriteLine(e.InnerException);
                    // return null;
                }

            };

            Console.WriteLine("Consuming from Profile's Knowledge Graph");
            channel.BasicConsume("Profile_KnowledgeGraph_LearningPlanFeedBack", false, consumer);
        }
        public void ListenForQuestionFeedBack()
        {
             var channel = queues.connection.CreateModel();
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                Console.WriteLine("Consuming from the queue");
                Console.WriteLine("-----------------------------------------------------------------------");
                try
                {
                    var body = ea.Body;
                    //  var user = (User)body.DeSerialize(typeof(User));
                    var questionFeedBack = (QuestionFeedBack)body.DeSerialize(typeof(QuestionFeedBack));
                    await graphfunctions.ReportQuestionAndRelationshipsAsync(questionFeedBack);
                    // var message = Encoding.UTF8.GetString(body);
                    //  var LP = JsonConvert.DeserializeObject<LearningPlanFeedBack>(message);
                    Console.WriteLine("User Name is {0} ");
                    var routingKey = ea.RoutingKey;
                    channel.BasicAck(ea.DeliveryTag, false);
                    Console.WriteLine("-----------------------------------------------------------------------");
                    Console.WriteLine(" - Routing Key <{0}>", routingKey);
                    await Task.Yield();
                }
                catch (Exception e)
                {
                    Console.WriteLine("----------------------EXCEPTION-MESSAGE------------------------------------");
                    Console.WriteLine(e.Message);
                    Console.WriteLine("----------------------STACK-TRACE-----------------------------------------");
                    Console.WriteLine(e.StackTrace);
                    Console.WriteLine("-------------------------INNER-EXCEPTION-----------------------------");
                    Console.WriteLine(e.InnerException);
                    // return null;
                }

            };

            Console.WriteLine("Consuming from Profile's Knowledge Graph");
            channel.BasicConsume("Profile_KnowledgeGraph_QuestionFeedBack", false, consumer);
        }
}
}