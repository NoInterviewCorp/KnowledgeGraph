using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KnowledgeGraph.Database;
using KnowledgeGraph.Database.Models;
using KnowledgeGraph.Services;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;

namespace KnowledgeGraph.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        QueueBuilder queue;
        QueueHandler handler;
        GraphDbConnection graph;
        // QueueHandler _handler, QueueBuilder _queue,
        public ValuesController( GraphDbConnection graph)
        {
            // handler = _handler;
            // queue = _queue;
            this.graph = graph;
        }
        // GET api/values
        [HttpGet]
        public IActionResult Get()
        {
            return Ok();
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public IActionResult Post([FromBody] LearningPlan lp)
        {
            // var channel = queue.connection.CreateModel();
            // channel.BasicPublish("KnowldegeGraphExchange", "Models.LearningPlan", null, lp.Serialize());
            return Ok(lp);
        }

        // PUT api/values/5
        [HttpPut()]
        public async Task<IActionResult> Put([FromBody] ResourceWrapper value)
        {

            var repo = new ResourceRepository(graph);
            var result = await repo.AddResourceAsync(value);
            if (result == null)
            {
                return BadRequest();
            }
            else
            {
                return Ok(result);
            }
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id) { }
    }
}