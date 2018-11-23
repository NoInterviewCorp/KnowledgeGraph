using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using my_profile.Services;
using my_profile.Controllers;
using my_profile;
namespace my_profile.Services
{

    public class GraphDbConnection : IDisposable
    {
        public GraphClient client;
        public GraphDbConnection()
        {
            // = new GraphClient(new Uri("http://localhost:7688/db/data"), "neo4j", "qwertyuiop");
            // .Connect();
            client = new GraphClient(new Uri("http://localhost:7474/db/data"), "neo4j", "Vfunny@123");
            client.Connect();
        }
        public void Dispose()
        {
            client.Dispose();
        }
    }
}