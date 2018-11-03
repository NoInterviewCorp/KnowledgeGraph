using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyProfile;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Web.Http;

namespace my_profile.Controllers
{
    [Route("api/[controller]")]
    [ApiController]


    public class ValuesController : ControllerBase
    {
        IUserRepo context = null;
        public ValuesController(IUserRepo _context){
            this.context = _context;
        }
[HttpGet]
        public ActionResult<IEnumerable<User>> Get()
        {
            
           /* using( context ){
            return     Ok(context.Students.ToList());
            }*/
            var notes = context.GetAllNotes();
            if(notes.Count > 0){
                return Ok(notes);
            }
            else{
                return Ok("No Entries Available. Database is Empty");
            }
            
        }
 [HttpGet("{id:int}")]
        public ActionResult<string> Get(string id)
        {
            //return "value";
            var noteById = context.GetNote(id);
            if (noteById != null)
            {
                return Ok(noteById);
            }
            else
            {
                return NotFound($"Note with {id} not found.");
            }
        }
 [HttpGet("{text}")]
        public ActionResult<string> Get(string text , [FromQuery] string type)
        {
            //return "value";
            var noteById1 = context.GetNote(text,type);
            if (noteById1 != null)
            {
                return Ok(noteById1);
            }
            else
            {
                return NotFound($"Note with {text} not found.");
            }
        }
 [HttpPost("Uploads")]

        public async Task<IActionResult> Uploads(IFormFileCollection files)
        {

          /*   try
            {
                // Full path to file in temp location
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "./wwwroot/image",file.FileName);
                var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);

                return Ok(new { length = file.Length, name = file.Name });
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }*/
           

         long size = files.Sum(f => f.Length);

    // Full path to file in temp location
   // var filePath = Path.GetTempFileName();
    try{
    foreach (var formFile in files)
      {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "./wwwroot/image",formFile.FileName);
                var stream = new FileStream(filePath, FileMode.Create);
                await formFile.CopyToAsync(stream);

                
      }
    return Ok(new { count = files.Count });
}
 catch(Exception e)
            {
                return BadRequest(e.Message);
            }
    // Process uploaded files

    //return Ok(new { count = files.Count, path = filePath});
    }
[HttpPost]
         public IActionResult Post([FromBody] User value)
        {
           if(ModelState.IsValid){
                bool result = context.PostNote(value);
                if (result)
                {
                    return Created($"/values/{value.UserId}",value);
                }
                else
                {
                    return BadRequest("Note already exists, please try again.");
                }
            }
            return BadRequest("Invalid Format");
    
    // or
    // context.Add<Student>(std);
    
    }



    }
}
