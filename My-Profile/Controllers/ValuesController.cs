using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Microsoft.AspNetCore.Http;
using System;
using MongoDB.Bson.Serialization.Attributes;
using System.Linq;
using MongoDB.Bson;

namespace My_Profile.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        public ValuesController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        // GET api/values
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var user = await _userRepository.GetAllUsers();
            if (user == null)
            {
                return NotFound("Null database");
            }
            else
            {

                return Ok(user);
            }

        }
        // GET: api/values/id
        [HttpGet("{_id}")]
        public async Task<IActionResult> Get(string _id)
        {
            ObjectId id = new ObjectId(_id);
            var user = await _userRepository.GetUser(id);
            if (user == null)
                return NotFound("user with this id not found");
            return Ok(user);
        }
        // POST: api/values
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]User user)
        {
            if (ModelState.IsValid)
            {
                bool result = await _userRepository.PostNote(user);
                if (!result)
                {
                    await _userRepository.Create(user);
                    return Ok(user);
                }
                else
                {
                    return BadRequest("Note already exists, please try again.");
                }
            }
            return BadRequest("Invalid Format");

        }



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
        // PUT: api/values/5
        [HttpPut("{_id}")]
        public async Task<IActionResult> Put(string _id, [FromBody]User user)
        {
            ObjectId id = new ObjectId(_id);
            if (ModelState.IsValid)
            {
                bool result = await _userRepository.FindNote(id);
                if (result)
                {
                    await _userRepository.Update(user);
                    return Ok(user);
                }
                else
                {
                    return BadRequest("Note already exists, please try again.");
                }
            }
            return BadRequest("Invalid Format");
        }
        // DELETE: api/values/5
        [HttpDelete("{_id}")]
        public async Task<IActionResult> Delete(string _id)
        {
            ObjectId id = new ObjectId(_id);

            bool result = await _userRepository.FindNote(id);
            if (result)
            {
                await _userRepository.Delete(id);
                return Ok($"note with id : {id} deleted succesfully");
            }
            else
            {
                return NotFound($"Note with {id} not found.");
            }

        }
    }
}
