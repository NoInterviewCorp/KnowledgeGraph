using RabbitMQ.Client;
using MongoDB.Driver;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using Newtonsoft.Json;
using My_Profile;
using System.Collections.Generic;
using My_Profile.Persistence;
namespace My_Profile.Services
{

    public class QueueHandler
    {
        // public List<User> users;
        public QueueBuilder queues;
        private IGraphFunctions graphfunctions;
        // private UserContext dbConnection;

        public QueueHandler(QueueBuilder _queues, IGraphFunctions _graphfunctions)
        {
            queues = _queues;
            graphfunctions = _graphfunctions;
            this.ListenForUser();
            this.ListenForLeaningPlanFeedBack();
            this.ListenForResourceFeedBack();
            this.ListenForLeaningPlanSubscriber();
            this.ListenForLeaningPlanUnSubscriber();
            this.ListenForQuestionFeedBack();
            //  this.HandleResourceFromQueue();
            //  this.QuizEngineQueueHandler();
        }

        public void ListenForUser()
        {
            var channel = queues.Connection.CreateModel();
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                Console.WriteLine("Consuming from the queue");
                Console.WriteLine("-----------------------------------------------------------------------");

                var body = ea.Body;
                //  var user = (User)body.DeSerialize(typeof(User));
                var message = Encoding.UTF8.GetString(body);
                var user = JsonConvert.DeserializeObject<User>(message);
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
            var channel = queues.Connection.CreateModel();
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
            var channel = queues.Connection.CreateModel();
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
            var channel = queues.Connection.CreateModel();
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
            var channel = queues.Connection.CreateModel();
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
             var channel = queues.Connection.CreateModel();
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