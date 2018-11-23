using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Web.Http;
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
        return new ObjectResult(await _userRepository.GetAllGames());
    }
    // GET: api/Game/name
    [HttpGet("{name}", Name = "Get")]
    public async Task<IActionResult> Get(string name)
    {
        var user = await _userRepository.GetUser(name);
        if (user == null)
            return new NotFoundResult();
        return new ObjectResult(user);
    }
    // POST: api/Game
    [HttpPost]
    public async Task<IActionResult> Post([FromBody]User user)
    {
        await _userRepository.Create(user);
        return new OkObjectResult(user);
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
    // PUT: api/Game/5
    [HttpPut("{name}")]
    public async Task<IActionResult> Put(string name, [FromBody]User user)
    {
        var userFromDb = await _userRepository.GetUser(name);
        if (userFromDb == null)
            return new NotFoundResult();
        user.Id = userFromDb.Id;
        await _userRepository.Update(user);
        return new OkObjectResult(user);
    }
    // DELETE: api/ApiWithActions/5
    [HttpDelete("{name}")]
    public async Task<IActionResult> Delete(string name)
    {
        var userFromDb = await _userRepository.GetUser(name);
        if (userFromDb == null)
            return new NotFoundResult();
        await _userRepository.Delete(name);
        return new OkResult();
    }
    }
}
