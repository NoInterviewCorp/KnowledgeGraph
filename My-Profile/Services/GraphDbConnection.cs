using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using My_Profile.Services;
using My_Profile.Controllers;
using My_Profile;
namespace My_Profile.Services
{

    public class GraphDbConnection : IDisposable
    {
        public GraphClient client;
        public GraphDbConnection()
        {
            // = new GraphClient(new Uri("http://localhost:7688/db/data"), "neo4j", "qwertyuiop");
            // .Connect();
            client = new GraphClient(new Uri("http://neo4jp:11008/db/data"), "neo4j", "Vfunny@123");
            client.Connect();
        }
        public void Dispose()
        {
            client.Dispose();
        }
    }
}
