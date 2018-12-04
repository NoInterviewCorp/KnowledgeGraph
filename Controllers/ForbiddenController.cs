using System.Threading.Tasks;
using KnowledgeGraph.Database;
using Microsoft.AspNetCore.Mvc;
using KnowledgeGraph.Models;

namespace KnowledgeGraph.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ForbiddenController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public IActionResult Get()
        {
            return Forbid();
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            return Forbid();
        }

        // POST api/values
        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] LearningPlanWrapper lp)
        {
            await Task.Yield();
            return Forbid();
        }

        // PUT api/values/5
        [HttpPut()]
        public async Task<IActionResult> Put([FromBody] ResourceWrapper value)
        {
            await Task.Yield();
            return Forbid();
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id) { }
    }
}