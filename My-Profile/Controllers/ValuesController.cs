using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using my_profile;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Web.Http;
using my_profile.Services;

namespace my_profile.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // GET api/values

        GraphDbConnection client;
        public ValuesController(GraphDbConnection _client)
        {
            this.client = _client;

        }
         [HttpGet]
       public async Task<IActionResult> Get()

        {


            var results1 =await client.client.Cypher
             .Match("(user:User)")
             .Return(user => user.As<User>())
             .ResultsAsync;
            return Ok(results1);
        }
        // GET api/values/5
        ////Get user by id
        [HttpGet("user/{userid}")]
        public async Task<IActionResult> Get(int userid)
        {
            var results =await client.client.Cypher
           .Match("(user:User)")
           .Where((User user) => user.UserId == userid)
           .Return(user => user.As<User>())
           .ResultsAsync;
            return Ok(results);
        }
        //Get learningplan by id
       
       //Get resource by id
      
        // POST api/values
        [HttpPost("UserNode")]
        public IActionResult UserPost([FromBody] User newUser)
        {
            try
            {
                // save 
                client.client.Cypher
                .Merge("(user:User { UserId: {id} })")
                .OnCreate()
                .Set("user = {newUser}")
                .WithParams(new
                {
                    id = newUser.UserId,
                    newUser
                })
              .ExecuteWithoutResults();
               return Ok();
            }
            catch (Exception ex)
            {
                // return error message if there was an exception
                return BadRequest(new { message = ex.Message });
            }
            //newUser =new  List<User>();


        }
        //post learningplan
        
       // upload userprofilepic
        [HttpPost("UploadsProfilePic")]

        public async Task<IActionResult> UploadsProfilePic(IFormFileCollection files)
        {

            long size = files.Sum(f => f.Length);
            try
            {
                foreach (var formFile in files)
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "./wwwroot/image", formFile.FileName);
                    var stream = new FileStream(filePath, FileMode.Create);
                    await formFile.CopyToAsync(stream);


                }
                return Ok(new { count = files.Count });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

        }
        //Update user details
        [HttpPut("user/{id}")]
        public IActionResult Put(int id, [FromBody] User newUser)
        {
            try
            {
                client.client.Cypher
              .Match("(user:User)")  
              .Where((User user) => user.UserId == id)
               .Set("user = {newUser}")
                    .WithParams(new
                    {
                        newUser
                    })
               .ExecuteWithoutResults();
                return Ok();
            }
            catch (Exception ex)
            {
                // return error message if there was an exception
                return BadRequest(new { message = ex.Message });
            }
        }
        // DELETE api/values/5
        // delete a user
        [HttpDelete("user/{id}")]
        public void Delete(int id)
        {
            client.client.Cypher
           .Match("(user:User)")
           .Where((User user) => user.UserId == id)
           .Delete("user")

           .ExecuteWithoutResults();
        }
    }
}
