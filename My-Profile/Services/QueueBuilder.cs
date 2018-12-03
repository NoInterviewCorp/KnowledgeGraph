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

namespace My_Profile.Services
{
     
    public class QueueBuilder
    {
      //  public List<User> users;
        private ConnectionFactory Factory;
        public IConnection Connection { get; set; }
        public IModel Model { get; set; }
        public string ExchangeNme
        {
            get { return "KnowldegeGraphExchange"; }
        }
        // private UserContext dbConnection;

        public QueueBuilder()
        {
            // this.dbConnection = dbConnection;

            Factory = new ConnectionFactory
            {
                // HostName = "172.23.238.173",
                HostName = "localhost",
                // Port = 8080,
               //  UserName = "achausername",
               //  Password = "strongpassword",
                DispatchConsumersAsync = true
            };

            Connection = Factory.CreateConnection();
            Model = Connection.CreateModel();
            Model.ExchangeDeclare("KnowldegeGraphExchange", "topic");
            Model.QueueDeclare("Profile_KnowledgeGraph_User", false, false, false, null);
            Model.QueueDeclare("Profile_KnowledgeGraph_LearningPlanFeedBack", false, false, false, null);
            Model.QueueDeclare("Profile_KnowledgeGraph_ResourceFeedBack", false, false, false, null);
            Model.QueueDeclare("Profile_KnowledgeGraph_QuestionFeedBack", false, false, false, null);
            // For getting average rating

            Model.QueueBind("Profile_KnowledgeGraph_User", "KnowldegeGraphExchange", "Users");
            Model.QueueBind("Profile_KnowledgeGraph_LearningPlanFeedBack", ExchangeNme, "Send.LearningPlanFeedBack");
            Model.QueueBind("Profile_KnowledgeGraph_ResourceFeedBack", ExchangeNme, "Send.ResourceFeedBack");
            Model.QueueBind("Profile_KnowledgeGraph_QuestionFeedBack", ExchangeNme, "Send.QuestionFeedBack");
           // ListenForUser();
        }
    }
}