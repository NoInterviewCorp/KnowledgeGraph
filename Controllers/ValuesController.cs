using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KnowledgeGraph.Database.Models;
using KnowledgeGraph.Services;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;

namespace KnowledgeGraph.Controllers {
    [Route ("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase {
        QueueBuilder queue;
        QueueHandler handler;
        public ValuesController (QueueHandler _handler, QueueBuilder _queue) {
            handler = _handler;
            queue = _queue;
        }
        // GET api/values
        [HttpGet]
        public IActionResult Get () {
            return Ok ();
        }

        // GET api/values/5
        [HttpGet ("{id}")]
        public ActionResult<string> Get (int id) {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public IActionResult Post ([FromBody] LearningPlan lp) {
            var channel = queue.connection.CreateModel ();
            channel.BasicPublish ("KnowldegeGraphExchange", "Models.LearningPlan", null, lp.Serialize ());
            return Ok (lp);
        }

        // PUT api/values/5
        [HttpPut ("{id}")]
        public void Put (int id, [FromBody] string value) { }

        // DELETE api/values/5
        [HttpDelete ("{id}")]
        public void Delete (int id) { }
    }
}