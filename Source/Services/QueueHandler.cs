using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KnowledeGraph.ContentWrapper;
using KnowledeGraph.Models;
using KnowledgeGraph.Database;
using KnowledgeGraph.Database.Persistence;
using KnowledgeGraph.Models;
using KnowledgeGraph.RabbitMQModels;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace KnowledgeGraph.Services
{
    public class QueueHandler
    {
        public QueueBuilder queues;
        private IGraphFunctions graphfunctions;
        private GraphBatchResponse questionidbatchlist;

        public QueueHandler(QueueBuilder _queues, IGraphFunctions _graphfunctions)
        {
            queues = _queues;
            graphfunctions = _graphfunctions;
            HandleLearningPlanFromQueue();
            HandleResourceFromQueue();
            HandleLearningPlanInfoRequest();
            HandleReportGeneration();
            HandleUserSubscriptionRequest();
            HandlerPopularPlanRequest();
            ListenForUser();
            ListenForLeaningPlanRating();
            ListenForResourceFeedBack();
            ListenForLeaningPlanSubscriber();
            ListenForLeaningPlanUnSubscriber();
            ListenForQuestionFeedBack();
            QuestionBatchRequestHandler();
            UpdateResultHandler();
            RecommendedResourceHandler();
            Console.WriteLine("------");
            //  this.QuizEngineQueueHandler();
        }

        public void HandleLearningPlanFromQueue()
        {
            var channel = queues.connection.CreateModel();
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                Console.WriteLine("-----------------------------------------------------------------------");
                Console.WriteLine("Consuming from the queue");
                channel.BasicAck(ea.DeliveryTag, false);
                var body = ea.Body;
                var learningPlan = (LearningPlanWrapper)body.DeSerialize(typeof(LearningPlanWrapper));
                await graphfunctions.CreateLearningPlanAndRelationshipsAsync(learningPlan);
                var routingKey = ea.RoutingKey;
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
                channel.BasicAck(ea.DeliveryTag, false);
                var body = ea.Body;
                var resource = (ResourceWrapper)body.DeSerialize(typeof(ResourceWrapper));
                await graphfunctions.CreateResourceAndRelationships(resource);
                var routingKey = ea.RoutingKey;
                Console.WriteLine("-----------------------------------------------------------------------");
                Console.WriteLine(" - Routing Key <{0}>", routingKey);
                await Task.Yield();
            };
            Console.WriteLine("Consuming from Contributor's Knowledge Graph");
            channel.BasicConsume("Contributer_KnowledgeGraph_Resources", false, consumer);
        }

        public void HandleLearningPlanInfoRequest()
        {
            var channel = queues.connection.CreateModel();
            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.Received += async (model, ea) =>
            {
                List<LearningPlanInfo> response = new List<LearningPlanInfo>();
                Console.WriteLine("---------------------------------------------------------------------");
                Console.WriteLine("Recieved Request for LearningPlanInfo");
                var body = ea.Body;
                var props = ea.BasicProperties;
                var replyProps = channel.CreateBasicProperties();
                replyProps.CorrelationId = props.CorrelationId;
                List<string> messages = new List<string>();
                try
                {
                    messages = (List<string>)body.DeSerialize(typeof(List<string>));
                    Console.WriteLine("Recieved request for Learningplan with " + messages.Count + " IDS");
                    response = await graphfunctions.GetLearningPlanInfoAsync(messages);
                }
                catch (Exception e)
                {
                    ConsoleWriter.ConsoleAnException(e);
                }
                finally
                {
                    // Serialize Response
                    var responseBytes = response.Serialize();
                    Console.WriteLine("Publishing back with correlationtid" + props.CorrelationId);
                    channel.BasicPublish(
                        exchange: queues.ExchangeName,
                        routingKey: "Response.LP",
                        basicProperties: replyProps,
                        body: responseBytes
                    );

                    channel.BasicAck(
                        deliveryTag: ea.DeliveryTag,
                        multiple: false
                    );
                }
                await Task.Yield();
            };
            channel.BasicConsume("AverageRating_TotalSubs_Request", false, consumer);
            Console.WriteLine(" [x] Awaiting RPC requests for LearningPlanInfo");
        }

        public void HandleReportGeneration()
        {
            var channel = queues.connection.CreateModel();
            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.Received += async (model, ea) =>
            {
                UserReport response = new UserReport();
                Console.WriteLine("---------------------------------------------------------------------");
                Console.WriteLine("Recieved Request for Report Generation");
                var body = ea.Body;
                var props = ea.BasicProperties;
                var replyProps = channel.CreateBasicProperties();
                replyProps.CorrelationId = props.CorrelationId;
                string messages = "";
                try
                {
                    messages = (string)body.DeSerialize(typeof(string));
                    Console.WriteLine("Recieved request of Report for " + messages);
                    response = await graphfunctions.GenerateUserReport(messages);
                }
                catch (Exception e)
                {
                    ConsoleWriter.ConsoleAnException(e);
                }
                finally
                {
                    // Serialize Response
                    var responseBytes = response.Serialize();
                    Console.WriteLine("Publishing back with correlationtid" + props.CorrelationId);
                    channel.BasicPublish(
                        exchange: queues.ExchangeName,
                        routingKey: props.ReplyTo,
                        basicProperties: replyProps,
                        body: responseBytes
                    );

                    channel.BasicAck(
                        deliveryTag: ea.DeliveryTag,
                        multiple: false
                    );
                }
                await Task.Yield();
            };
            channel.BasicConsume("UserReport_Request", false, consumer);
            Console.WriteLine(" [x] Awaiting RPC requests for UserReportGeneration");
        }

        public void HandleUserSubscriptionRequest()
        {
            var channel = queues.connection.CreateModel();
            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.Received += async (model, ea) =>
            {
                var response = new List<string>();
                var lpInfo = new List<LearningPlanInfo>();
                Console.WriteLine("---------------------------------------------------------------------");
                Console.WriteLine("Recieved Request for User's Subscription");
                var body = ea.Body;
                var props = ea.BasicProperties;
                var replyProps = channel.CreateBasicProperties();
                replyProps.CorrelationId = props.CorrelationId;
                string messages = "";
                try
                {
                    messages = (string)body.DeSerialize(typeof(string));
                    Console.WriteLine("Recieved request of Subscription for " + messages);
                    response.AddRange(await graphfunctions.SubscribeLearningPlanAndRelationshipsAsync1(messages));
                    lpInfo.AddRange(await graphfunctions.GetLearningPlanInfoAsync(response));
                    Console.WriteLine("recieved " + lpInfo.Count + " subscripiton plans");
                }
                catch (Exception e)
                {
                    ConsoleWriter.ConsoleAnException(e);
                }
                finally
                {
                    // Serialize Response
                    var responseBytes = lpInfo.Serialize();
                    Console.WriteLine("Publishing back with correlationtid -> " + props.CorrelationId);
                    channel.BasicPublish(
                        exchange: queues.ExchangeName,
                        routingKey: props.ReplyTo,
                        basicProperties: replyProps,
                        body: responseBytes
                    );

                    channel.BasicAck(
                        deliveryTag: ea.DeliveryTag,
                        multiple: false
                    );
                }
                await Task.Yield();
            };
            channel.BasicConsume("Contributer_KnowledgeGraph_Subscriptions", false, consumer);
            Console.WriteLine(" [x] Awaiting RPC requests for UserSubscriptionRequest");
        }

        public void HandlerPopularPlanRequest()
        {
            var channel = queues.connection.CreateModel();
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var lpInfo = new List<LearningPlanInfo>();
                var response = new List<string>();
                Console.WriteLine("---------------------------------------------------------------------");
                Console.WriteLine("Recieved Request for Popular plans");
                var body = ea.Body;
                var props = ea.BasicProperties;
                var replyProps = channel.CreateBasicProperties();
                replyProps.CorrelationId = props.CorrelationId;
                string messages = "";
                try
                {
                    messages = (string)body.DeSerialize(typeof(string));
                    Console.WriteLine("Recieved request of Popular plans for " + messages);
                    response.AddRange(await graphfunctions.PopularLearningPlanAndRelationshipsAsync1(messages));
                    lpInfo.AddRange(await graphfunctions.GetLearningPlanInfoAsync(response));
                    Console.WriteLine("recieved " + lpInfo.Count + " popular plans");
                }
                catch (Exception e)
                {
                    ConsoleWriter.ConsoleAnException(e);
                }
                finally
                {
                    // Serialize Response
                    var responseBytes = lpInfo.Serialize();
                    Console.WriteLine("Publishing back with correlationtid -> " + props.CorrelationId);
                    channel.BasicPublish(
                        exchange: queues.ExchangeName,
                        routingKey: props.ReplyTo,
                        basicProperties: replyProps,
                        body: responseBytes
                    );

                    channel.BasicAck(
                        deliveryTag: ea.DeliveryTag,
                        multiple: false
                    );
                }
                await Task.Yield();
            };
            channel.BasicConsume("Contributer_KnowledgeGraph_PopularPlans", false, consumer);
            Console.WriteLine(" [x] Awaiting RPC requests for UserSubscriptionRequest");
        }

        public void QuestionBatchRequestHandler()
        {
            Console.WriteLine("In Question Batch Request Handler");
            var channel = queues.connection.CreateModel();
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                Console.WriteLine("Recieved Request for Questions");
                try
                {
                    channel.BasicAck(ea.DeliveryTag, false);
                    var body = ea.Body;
                    var batch_query = (QuestionBatchRequest)body.DeSerialize(typeof(QuestionBatchRequest));
                    this.questionidbatchlist = new GraphBatchResponse(batch_query.Username);
                    var questionQuery = graphfunctions.GetQuestionBatchIds(batch_query.Username, batch_query.Tech, batch_query.Concepts);
                    Console.WriteLine("Question query returned for " + questionQuery.Count + " quesitons");
                    this.questionidbatchlist.IdRequestList.AddRange(questionQuery);
                    foreach (var v in questionidbatchlist.IdRequestList)
                    {
                        Console.WriteLine(v);
                    }
                    channel.BasicPublish("KnowledgeGraphExchange", "Request.Question", null, this.questionidbatchlist.Serialize());
                    var routingKey = ea.RoutingKey;
                    Console.WriteLine(" - Routing Key <{0}>", routingKey);
                    Console.WriteLine("- Delivery Tag <{0}>", ea.DeliveryTag);
                    await Task.Yield();
                }
                catch (Exception e)
                {
                    ConsoleWriter.ConsoleAnException(e);
                }
            };
            channel.BasicConsume("QuizEngine_KnowledgeGraph_QuestionBatch", false, consumer);
        }
        public void ListenForUser()
        {
            var channel = queues.connection.CreateModel();
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                Console.WriteLine("Consuming from the queue");
                Console.WriteLine("-----------------------------------------------------------------------");
                channel.BasicAck(ea.DeliveryTag, false);
                var body = ea.Body;
                var user = (UserWrapper)body.DeSerialize(typeof(UserWrapper));
                await graphfunctions.UserAndRelationshipsAsync(user);
                // var message = Encoding.UTF8.GetString(body);
                // var user = JsonConvert.DeserializeObject<User>(message);
                Console.WriteLine("User Name is {0} " + user.UserId);
                var routingKey = ea.RoutingKey;
                Console.WriteLine("-----------------------------------------------------------------------");
                Console.WriteLine(" - Routing Key <{0}>", routingKey);
                await Task.Yield();

            };
            Console.WriteLine("Consuming from Profile's Knowledge Graph");
            channel.BasicConsume("Profile_KnowledgeGraph_User", false, consumer);
        }
        public void ListenForLeaningPlanRating()
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
                    var learningPlanRatingWrapper = (LearningPlanRatingWrapper)body.DeSerialize(typeof(LearningPlanRatingWrapper));
                    await graphfunctions.RatingLearningPlanAndRelationshipsAsync(learningPlanRatingWrapper);
                    Console.WriteLine("User Name is {0} ");
                    var routingKey = ea.RoutingKey;
                    Console.WriteLine("-----------------------------------------------------------------------");
                    Console.WriteLine(" - Routing Key <{0}>", routingKey);
                    await Task.Yield();
                }
                catch (Exception e)
                {
                    ConsoleWriter.ConsoleAnException(e);
                }

            };

            Console.WriteLine("Consuming from Profile's Knowledge Graph");
            channel.BasicConsume("Profile_KnowledgeGraph_LearningPlanRatingWrapper", false, consumer);
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
                    channel.BasicAck(ea.DeliveryTag, false);
                    var body = ea.Body;
                    //  var user = (User)body.DeSerialize(typeof(User));
                    var resourceRatingWrapper = (ResourceRatingWrapper)body.DeSerialize(typeof(ResourceRatingWrapper));
                    await graphfunctions.RatingResourceAndRelationshipsAsync(resourceRatingWrapper);
                    // var message = Encoding.UTF8.GetString(body);
                    //  var LP = JsonConvert.DeserializeObject<LearningPlanFeedBack>(message);
                    Console.WriteLine("User Name is {0} ");
                    var routingKey = ea.RoutingKey;
                    Console.WriteLine("-----------------------------------------------------------------------");
                    Console.WriteLine(" - Routing Key <{0}>", routingKey);
                    await Task.Yield();
                }
                catch (Exception e)
                {
                    ConsoleWriter.ConsoleAnException(e);
                }

            };

            Console.WriteLine("Consuming from Profile's Knowledge Graph");
            channel.BasicConsume("Profile_KnowledgeGraph_ResourceRatingWrapper", false, consumer);

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
                    channel.BasicAck(ea.DeliveryTag, false);
                    var body = ea.Body;
                    //  var user = (User)body.DeSerialize(typeof(User));
                    var learningPlanFeedBack = (LearningPlanSubscriptionWrapper)body.DeSerialize(typeof(LearningPlanSubscriptionWrapper));
                    await graphfunctions.SubscribeLearningPlanAndRelationshipsAsync(learningPlanFeedBack);
                    // var message = Encoding.UTF8.GetString(body);
                    //  var LP = JsonConvert.DeserializeObject<LearningPlanFeedBack>(message);
                    Console.WriteLine("User Name is {0} ");
                    var routingKey = ea.RoutingKey;
                    Console.WriteLine("-----------------------------------------------------------------------");
                    Console.WriteLine(" - Routing Key <{0}>", routingKey);
                    await Task.Yield();
                }
                catch (Exception e)
                {
                    ConsoleWriter.ConsoleAnException(e);
                }

            };

            Console.WriteLine("Consuming from Profile's Knowledge Graph");
            channel.BasicConsume("Profile_KnowledgeGraph_LearningPlanSubscriptionWrapper", false, consumer);
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
                    channel.BasicAck(ea.DeliveryTag, false);
                    var body = ea.Body;
                    //  var user = (User)body.DeSerialize(typeof(User));
                    var learningPlanFeedBack = (LearningPlanSubscriptionWrapper)body.DeSerialize(typeof(LearningPlanSubscriptionWrapper));
                    await graphfunctions.UnSubscribeLearningPlanAndRelationshipsAsync(learningPlanFeedBack);
                    // var message = Encoding.UTF8.GetString(body);
                    //  var LP =    JsonConvert.DeserializeObject<LearningPlanFeedBack>(message);
                    Console.WriteLine("User Name is {0} ");
                    var routingKey = ea.RoutingKey;
                    Console.WriteLine("-----------------------------------------------------------------------");
                    Console.WriteLine(" - Routing Key <{0}>", routingKey);
                    await Task.Yield();
                }
                catch (Exception e)
                {
                    ConsoleWriter.ConsoleAnException(e);
                }

            };

            Console.WriteLine("Consuming from Profile's Knowledge Graph");
            channel.BasicConsume("Profile_KnowledgeGraph_LearningPlanSubscriptionWrapper", false, consumer);
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
                    channel.BasicAck(ea.DeliveryTag, false);
                    var body = ea.Body;
                    //  var user = (User)body.DeSerialize(typeof(User));
                    var questionAmbiguityWrapper = (QuestionAmbiguityWrapper)body.DeSerialize(typeof(QuestionAmbiguityWrapper));
                    await graphfunctions.ReportQuestionAndRelationshipsAsync(questionAmbiguityWrapper);
                    // var message = Encoding.UTF8.GetString(body);
                    //  var LP = JsonConvert.DeserializeObject<LearningPlanFeedBack>(message);
                    Console.WriteLine("User Name is {0} ");
                    var routingKey = ea.RoutingKey;
                    Console.WriteLine("-----------------------------------------------------------------------");
                    Console.WriteLine(" - Routing Key <{0}>", routingKey);
                    await Task.Yield();
                }
                catch (Exception e)
                {
                    ConsoleWriter.ConsoleAnException(e);
                }

            };
            Console.WriteLine("Consuming from Profile's Knowledge Graph");
            channel.BasicConsume("Profile_KnowledgeGraph_QuestionAmbiguityWrapper", false, consumer);
        }
        public void UpdateResultHandler()
        {
            var channel = queues.connection.CreateModel();
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                Console.WriteLine("Listening to Results");
                try
                {
                    channel.BasicAck(ea.DeliveryTag, false);
                    var body = ea.Body;
                    var result_query = (ResultWrapper)body.DeSerialize(typeof(ResultWrapper));
                    Console.WriteLine(result_query.Username);
                    Console.WriteLine(result_query.Concept, result_query.Bloom);
                    graphfunctions.IncreaseIntensityOnConcept(result_query.Username, result_query.Concept, result_query.Bloom);
                    var routingKey = ea.RoutingKey;
                    Console.WriteLine(" - Routing Key <{0}>", routingKey);
                    Console.WriteLine("- Delivery Tag <{0}>", ea.DeliveryTag);
                    await Task.Yield();
                }
                catch (Exception e)
                {
                    ConsoleWriter.ConsoleAnException(e);
                }
            };
            channel.BasicConsume("QuizEngine_KnowledgeGraph_Result", false, consumer);
        }
        public void RecommendedResourceHandler()
        {
            var channel = queues.connection.CreateModel();
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                Console.WriteLine("Listening to Results");
                try
                {
                    channel.BasicAck(ea.DeliveryTag, false);
                    var body = ea.Body;
                    var result_query = (string)body.DeSerialize(typeof(string));
                    var resource_query = new RecommendedResourceWrapper(result_query);
                    resource_query.ResourceIds.AddRange(graphfunctions.RecommendResource(result_query));
                    var routingKey = ea.RoutingKey;
                    Console.WriteLine(" - Routing Key <{0}>", routingKey);
                    Console.WriteLine("- Delivery Tag <{0}>", ea.DeliveryTag);
                    queues.Model.BasicPublish(
                        exchange: "KnowledgeGraphExchange",
                        routingKey: "Request.Resource",
                        basicProperties: null,
                        body: resource_query.Serialize()
                    );
                    await Task.Yield();
                }
                catch (Exception e)
                {
                    ConsoleWriter.ConsoleAnException(e);
                }
            };
            channel.BasicConsume("QuizEngine_KnowledgeGraph_RecommendResource", false, consumer);
        }
    }
}