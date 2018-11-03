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
        [HttpPost("Upload")]

        public async Task<IActionResult> Upload(IFormFile file)
        {

            try
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
            }
        }



    }
}
