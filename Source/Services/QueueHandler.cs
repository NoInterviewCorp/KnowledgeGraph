using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KnowledeGraph.ContentWrapper;
using KnowledgeGraph.Database;
using KnowledgeGraph.Database.Persistence;
using KnowledgeGraph.Models;
using KnowledgeGraph.RabbitMQModels;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace KnowledgeGraph.Services {
    public class QueueHandler {
        public QueueBuilder queues;
        private QuizEngineQuery query;
        private IGraphFunctions graphfunctions;
        private List<int> IDs;
        private LearningPlan lgiearningPlan;
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
            this.ListenForUser ();
            this.ListenForLeaningPlanRating ();
            this.ListenForResourceFeedBack ();
            this.ListenForLeaningPlanSubscriber ();
            this.ListenForLeaningPlanUnSubscriber ();
            this.ListenForQuestionFeedBack ();
            this.QuestionBatchRequestHandler ();
            //  this.QuizEngineQueueHandler();
        }

        public void HandleLearningPlanFromQueue () {
            var channel = queues.connection.CreateModel ();
            var consumer = new AsyncEventingBasicConsumer (channel);
            consumer.Received += async (model, ea) => {
                Console.WriteLine ("-----------------------------------------------------------------------");
                Console.WriteLine ("Consuming from the queue");
                channel.BasicAck (ea.DeliveryTag, false);
                var body = ea.Body;
                var learningPlan = (LearningPlanWrapper) body.DeSerialize (typeof (LearningPlanWrapper));
                await graphfunctions.CreateLearningPlanAndRelationshipsAsync (learningPlan);
                var routingKey = ea.RoutingKey;
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
                channel.BasicAck (ea.DeliveryTag, false);
                var body = ea.Body;
                var resource = (ResourceWrapper) body.DeSerialize (typeof (ResourceWrapper));
                await graphfunctions.CreateResourceAndRelationships (resource);
                var routingKey = ea.RoutingKey;
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
            Console.WriteLine ("Question request queue started");
            consumer.Received += async (model, ea) => {
                Console.WriteLine ("Recieved Request for Questions");
                try {
                    channel.BasicAck (ea.DeliveryTag, false);
                    var body = ea.Body;
                    query = (QuizEngineQuery) body.DeSerialize (typeof (QuizEngineQuery));
                    IDs.Clear ();
                    IDs.AddRange (graphfunctions.GetQuestionIds (query.tech, query.username));
                    channel.BasicPublish ("KnowldegeGraphExchange", "Models.QuestionId", null, IDs.Serialize ());
                    var routingKey = ea.RoutingKey;
                    Console.WriteLine (" - Routing Key <{0}>", routingKey);
                    Console.WriteLine ("- Delivery Tag <{0}>", ea.DeliveryTag);
                    await Task.Yield ();
                } catch (Exception e) {
                    Console.WriteLine ("----------------------EXCEPTION-MESSAGE------------------------------------");
                    Console.WriteLine (e.Message);
                    Console.WriteLine ("----------------------STACK-TRACE-----------------------------------------");
                    Console.WriteLine (e.StackTrace);
                    Console.WriteLine ("-------------------------INNER-EXCEPTION-----------------------------");
                    Console.WriteLine (e.InnerException);
                }
            };
            Console.WriteLine ("Consuming from the queue");
            channel.BasicConsume ("QuizEngine_KnowledgeGraph_QuestionBatch", false, consumer);
        }

        public void QuestionBatchRequestHandler () {
            Console.WriteLine("In Question Batch Request Handler");
            var channel = queues.connection.CreateModel ();
            var consumer = new AsyncEventingBasicConsumer (channel);
            consumer.Received += async (model, ea) => {
                Console.WriteLine ("Recieved Request for Questions");
                try {
                    channel.BasicAck (ea.DeliveryTag, false);
                    var body = ea.Body;
                    batch_query = (QuestionBatchRequest) body.DeSerialize (typeof (QuestionBatchRequest));
                    this.questionidbatchlist = new GraphBatchResponse (batch_query.Username);
                    this.questionidbatchlist.questionids = (graphfunctions.GetQuestionBatchIds (batch_query.Username, batch_query.Tech, batch_query.Concepts));
                    channel.BasicPublish ("KnowldegeGraphExchange", "Models.QuestionId", null, this.questionidbatchlist.Serialize ());
                    var routingKey = ea.RoutingKey;
                    Console.WriteLine (" - Routing Key <{0}>", routingKey);
                    Console.WriteLine ("- Delivery Tag <{0}>", ea.DeliveryTag);
                    await Task.Yield ();
                } catch (Exception e) {
                    Console.WriteLine ("----------------------EXCEPTION-MESSAGE------------------------------------");
                    Console.WriteLine (e.Message);
                    Console.WriteLine ("----------------------STACK-TRACE-----------------------------------------");
                    Console.WriteLine (e.StackTrace);
                    Console.WriteLine ("-------------------------INNER-EXCEPTION-----------------------------");
                    Console.WriteLine (e.InnerException);
                }
            };
            channel.BasicConsume ("QuizEngine_KnowledgeGraph_QuestionBatch", false, consumer);
        }
        public void QuestionRequestHandler () {
            var channel = queues.connection.CreateModel ();
            var consumer = new AsyncEventingBasicConsumer (channel);
            consumer.Received += async (model, ea) => {
                Console.WriteLine ("Recieved Request for Questions");
                try {
                    channel.BasicAck (ea.DeliveryTag, false);
                    var body = ea.Body;
                    question_query = (QuestionRequest) body.DeSerialize (typeof (QuestionRequest));
                    this.questionidlist = new QuestionIdsResponse (batch_query.Username);
                    this.questionidlist.IdRequestDictionary = (graphfunctions.GetQuestionIds (question_query.Username, question_query.Tech, question_query.Concept));
                    channel.BasicPublish ("KnowldegeGraphExchange", "Routing Key", null, this.questionidlist.Serialize ());
                    var routingKey = ea.RoutingKey;
                    Console.WriteLine (" - Routing Key <{0}>", routingKey);
                    Console.WriteLine ("- Delivery Tag <{0}>", ea.DeliveryTag);
                    await Task.Yield ();
                } catch (Exception e) {
                    Console.WriteLine ("----------------------EXCEPTION-MESSAGE------------------------------------");
                    Console.WriteLine (e.Message);
                    Console.WriteLine ("----------------------STACK-TRACE-----------------------------------------");
                    Console.WriteLine (e.StackTrace);
                    Console.WriteLine ("-------------------------INNER-EXCEPTION-----------------------------");
                    Console.WriteLine (e.InnerException);
                }
            };
            channel.BasicConsume ("QuizEngine_KnowledgeGraph", false, consumer);
        }
        public void ConceptRequestHandler () {
            var channel = queues.connection.CreateModel ();
            var consumer = new AsyncEventingBasicConsumer (channel);
            consumer.Received += async (model, ea) => {
                Console.WriteLine ("Recieved Request for Concepts");
                try {
                    var body = ea.Body;
                    channel.BasicAck (ea.DeliveryTag, false);
                    concept_query = (ConceptRequest) body.DeSerialize (typeof (ConceptRequest));
                    this.concept_list = new ConceptResponse (concept_query.Username);
                    this.concept_list.concepts.AddRange (graphfunctions.GetConceptFromTechnology (concept_query.Tech));
                    channel.BasicPublish ("KnowldegeGraphExchange", "Routing Key", null, this.concept_list.Serialize ());
                    var routingKey = ea.RoutingKey;
                    Console.WriteLine (" - Routing Key <{0}>", routingKey);
                    Console.WriteLine ("- Delivery Tag <{0}>", ea.DeliveryTag);
                    await Task.Yield ();
                } catch (Exception e) {
                    Console.WriteLine ("----------------------EXCEPTION-MESSAGE------------------------------------");
                    Console.WriteLine (e.Message);
                    Console.WriteLine ("----------------------STACK-TRACE-----------------------------------------");
                    Console.WriteLine (e.StackTrace);
                    Console.WriteLine ("-------------------------INNER-EXCEPTION-----------------------------");
                    Console.WriteLine (e.InnerException);
                }
            };
            channel.BasicConsume ("QuizEngine_KnowledgeGraph_Concepts", false, consumer);
        }
        public void ListenForUser () {
            var channel = queues.connection.CreateModel ();
            var consumer = new AsyncEventingBasicConsumer (channel);
            consumer.Received += async (model, ea) => {
                Console.WriteLine ("Consuming from the queue");
                Console.WriteLine ("-----------------------------------------------------------------------");
                channel.BasicAck (ea.DeliveryTag, false);
                var body = ea.Body;
                var user = (User) body.DeSerialize (typeof (User));
                // var message = Encoding.UTF8.GetString(body);
                // var user = JsonConvert.DeserializeObject<User>(message);
                Console.WriteLine ("User Name is {0} " + user.FullName);
                var routingKey = ea.RoutingKey;
                Console.WriteLine ("-----------------------------------------------------------------------");
                Console.WriteLine (" - Routing Key <{0}>", routingKey);
                await Task.Yield ();

            };
            Console.WriteLine ("Consuming from Profile's Knowledge Graph");
            channel.BasicConsume ("Profile_KnowledgeGraph_User", false, consumer);
        }
        public void ListenForLeaningPlanRating () {
            var channel = queues.connection.CreateModel ();
            var consumer = new AsyncEventingBasicConsumer (channel);
            consumer.Received += async (model, ea) => {
                Console.WriteLine ("Consuming from the queue");
                Console.WriteLine ("-----------------------------------------------------------------------");
                try {
                    var body = ea.Body;
                    //  var user = (User)body.DeSerialize(typeof(User));
                    var learningPlanRatingWrapper = (LearningPlanRatingWrapper) body.DeSerialize (typeof (LearningPlanRatingWrapper));
                    await graphfunctions.RatingLearningPlanAndRelationshipsAsync (learningPlanRatingWrapper);
                    // var message = Encoding.UTF8.GetString(body);
                    //  var LP = JsonConvert.DeserializeObject<LearningPlanFeedBack>(message);
                    Console.WriteLine ("User Name is {0} ");
                    var routingKey = ea.RoutingKey;
                    Console.WriteLine ("-----------------------------------------------------------------------");
                    Console.WriteLine (" - Routing Key <{0}>", routingKey);
                    await Task.Yield ();
                } catch (Exception e) {
                    Console.WriteLine ("----------------------EXCEPTION-MESSAGE------------------------------------");
                    Console.WriteLine (e.Message);
                    Console.WriteLine ("----------------------STACK-TRACE-----------------------------------------");
                    Console.WriteLine (e.StackTrace);
                    Console.WriteLine ("-------------------------INNER-EXCEPTION-----------------------------");
                    Console.WriteLine (e.InnerException);
                    // return null;
                }

            };

            Console.WriteLine ("Consuming from Profile's Knowledge Graph");
            channel.BasicConsume ("Profile_KnowledgeGraph_LearningPlanFeedBack", false, consumer);
        }
        public void ListenForResourceFeedBack () {
            var channel = queues.connection.CreateModel ();
            var consumer = new AsyncEventingBasicConsumer (channel);
            consumer.Received += async (model, ea) => {
                Console.WriteLine ("Consuming from the queue");
                Console.WriteLine ("-----------------------------------------------------------------------");
                try {
                    channel.BasicAck (ea.DeliveryTag, false);
                    var body = ea.Body;
                    //  var user = (User)body.DeSerialize(typeof(User));
                    var resourceRatingWrapper = (ResourceRatingWrapper) body.DeSerialize (typeof (ResourceRatingWrapper));
                    await graphfunctions.RatingResourceAndRelationshipsAsync (resourceRatingWrapper);
                    // var message = Encoding.UTF8.GetString(body);
                    //  var LP = JsonConvert.DeserializeObject<LearningPlanFeedBack>(message);
                    Console.WriteLine ("User Name is {0} ");
                    var routingKey = ea.RoutingKey;
                    Console.WriteLine ("-----------------------------------------------------------------------");
                    Console.WriteLine (" - Routing Key <{0}>", routingKey);
                    await Task.Yield ();
                } catch (Exception e) {
                    Console.WriteLine ("----------------------EXCEPTION-MESSAGE------------------------------------");
                    Console.WriteLine (e.Message);
                    Console.WriteLine ("----------------------STACK-TRACE-----------------------------------------");
                    Console.WriteLine (e.StackTrace);
                    Console.WriteLine ("-------------------------INNER-EXCEPTION-----------------------------");
                    Console.WriteLine (e.InnerException);
                    // return null;
                }

            };

            Console.WriteLine ("Consuming from Profile's Knowledge Graph");
            channel.BasicConsume ("Profile_KnowledgeGraph_ResourceFeedBack", false, consumer);

        }
        public void ListenForLeaningPlanSubscriber () {
            var channel = queues.connection.CreateModel ();
            var consumer = new AsyncEventingBasicConsumer (channel);
            consumer.Received += async (model, ea) => {
                Console.WriteLine ("Consuming from the queue");
                Console.WriteLine ("-----------------------------------------------------------------------");
                try {
                    channel.BasicAck (ea.DeliveryTag, false);
                    var body = ea.Body;
                    //  var user = (User)body.DeSerialize(typeof(User));
                    var learningPlanFeedBack = (LearningPlanSubscriptionWrapper) body.DeSerialize (typeof (LearningPlanSubscriptionWrapper));
                    await graphfunctions.SubscribeLearningPlanAndRelationshipsAsync (learningPlanFeedBack);
                    // var message = Encoding.UTF8.GetString(body);
                    //  var LP = JsonConvert.DeserializeObject<LearningPlanFeedBack>(message);
                    Console.WriteLine ("User Name is {0} ");
                    var routingKey = ea.RoutingKey;
                    Console.WriteLine ("-----------------------------------------------------------------------");
                    Console.WriteLine (" - Routing Key <{0}>", routingKey);
                    await Task.Yield ();
                } catch (Exception e) {
                    Console.WriteLine ("----------------------EXCEPTION-MESSAGE------------------------------------");
                    Console.WriteLine (e.Message);
                    Console.WriteLine ("----------------------STACK-TRACE-----------------------------------------");
                    Console.WriteLine (e.StackTrace);
                    Console.WriteLine ("-------------------------INNER-EXCEPTION-----------------------------");
                    Console.WriteLine (e.InnerException);
                    // return null;
                }

            };

            Console.WriteLine ("Consuming from Profile's Knowledge Graph");
            channel.BasicConsume ("Profile_KnowledgeGraph_LearningPlanFeedBack", false, consumer);
        }
        public void ListenForLeaningPlanUnSubscriber () {
            var channel = queues.connection.CreateModel ();
            var consumer = new AsyncEventingBasicConsumer (channel);
            consumer.Received += async (model, ea) => {
                Console.WriteLine ("Consuming from the queue");
                Console.WriteLine ("-----------------------------------------------------------------------");
                try {
                    channel.BasicAck (ea.DeliveryTag, false);
                    var body = ea.Body;
                    //  var user = (User)body.DeSerialize(typeof(User));
                    var learningPlanFeedBack = (LearningPlanSubscriptionWrapper) body.DeSerialize (typeof (LearningPlanSubscriptionWrapper));
                    await graphfunctions.UnSubscribeLearningPlanAndRelationshipsAsync (learningPlanFeedBack);
                    // var message = Encoding.UTF8.GetString(body);
                    //  var LP = JsonConvert.DeserializeObject<LearningPlanFeedBack>(message);
                    Console.WriteLine ("User Name is {0} ");
                    var routingKey = ea.RoutingKey;
                    Console.WriteLine ("-----------------------------------------------------------------------");
                    Console.WriteLine (" - Routing Key <{0}>", routingKey);
                    await Task.Yield ();
                } catch (Exception e) {
                    Console.WriteLine ("----------------------EXCEPTION-MESSAGE------------------------------------");
                    Console.WriteLine (e.Message);
                    Console.WriteLine ("----------------------STACK-TRACE-----------------------------------------");
                    Console.WriteLine (e.StackTrace);
                    Console.WriteLine ("-------------------------INNER-EXCEPTION-----------------------------");
                    Console.WriteLine (e.InnerException);
                    // return null;
                }

            };

            Console.WriteLine ("Consuming from Profile's Knowledge Graph");
            channel.BasicConsume ("Profile_KnowledgeGraph_LearningPlanFeedBack", false, consumer);
        }
        public void ListenForQuestionFeedBack () {
            var channel = queues.connection.CreateModel ();
            var consumer = new AsyncEventingBasicConsumer (channel);
            consumer.Received += async (model, ea) => {
                Console.WriteLine ("Consuming from the queue");
                Console.WriteLine ("-----------------------------------------------------------------------");
                try {
                    channel.BasicAck (ea.DeliveryTag, false);
                    var body = ea.Body;
                    //  var user = (User)body.DeSerialize(typeof(User));
                    var questionAmbiguityWrapper = (QuestionAmbiguityWrapper) body.DeSerialize (typeof (QuestionAmbiguityWrapper));
                    await graphfunctions.ReportQuestionAndRelationshipsAsync (questionAmbiguityWrapper);
                    // var message = Encoding.UTF8.GetString(body);
                    //  var LP = JsonConvert.DeserializeObject<LearningPlanFeedBack>(message);
                    Console.WriteLine ("User Name is {0} ");
                    var routingKey = ea.RoutingKey;
                    Console.WriteLine ("-----------------------------------------------------------------------");
                    Console.WriteLine (" - Routing Key <{0}>", routingKey);
                    await Task.Yield ();
                } catch (Exception e) {
                    Console.WriteLine ("----------------------EXCEPTION-MESSAGE------------------------------------");
                    Console.WriteLine (e.Message);
                    Console.WriteLine ("----------------------STACK-TRACE-----------------------------------------");
                    Console.WriteLine (e.StackTrace);
                    Console.WriteLine ("-------------------------INNER-EXCEPTION-----------------------------");
                    Console.WriteLine (e.InnerException);
                    // return null;
                }

            };
            Console.WriteLine ("Consuming from Profile's Knowledge Graph");
            channel.BasicConsume ("Profile_KnowledgeGraph_QuestionFeedBack", false, consumer);
        }
        public void UpdateResultHandler () {
            var channel = queues.connection.CreateModel ();
            var consumer = new AsyncEventingBasicConsumer (channel);
            consumer.Received += async (model, ea) => {
                Console.WriteLine ("Recieved Request for Concepts");
                try {
                    channel.BasicAck (ea.DeliveryTag, false);
                    var body = ea.Body;
                    var result_query = (ResultWrapper) body.DeSerialize (typeof (ResultWrapper));
                    graphfunctions.IncreaseIntensityOnConcept (result_query.Username, result_query.Concept, result_query.Bloom);
                    var routingKey = ea.RoutingKey;
                    Console.WriteLine (" - Routing Key <{0}>", routingKey);
                    Console.WriteLine ("- Delivery Tag <{0}>", ea.DeliveryTag);
                    await Task.Yield ();
                } catch (Exception e) {
                    Console.WriteLine ("----------------------EXCEPTION-MESSAGE------------------------------------");
                    Console.WriteLine (e.Message);
                    Console.WriteLine ("----------------------STACK-TRACE-----------------------------------------");
                    Console.WriteLine (e.StackTrace);
                    Console.WriteLine ("-------------------------INNER-EXCEPTION-----------------------------");
                    Console.WriteLine (e.InnerException);
                }
            };
            channel.BasicConsume ("QuizEngine_KnowledgeGraph_Result", false, consumer);
        }
    }
}