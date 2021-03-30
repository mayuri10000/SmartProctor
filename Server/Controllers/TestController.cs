using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartProctor.Server.Services;

namespace SmartProctor.Server.Controllers
{
    // This controller is made just for testing in development process and will be removed
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IUserServices _userServices;

        public TestController(IWebHostEnvironment hostEnvironment, IUserServices userServices)
        {
            _webHostEnvironment = hostEnvironment;
            _userServices = userServices;
        }
        
        [HttpPost]
        public ActionResult Post(IFormFile file)
        {
            if (User.Identity?.Name == null)
            {
                return BadRequest();
            }

            var uid = User.Identity.Name;

            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!".bmp/.jpg/.jpeg/.png/.gif".Contains(extension))
            {
                return BadRequest();
            }

            var fileName = "img/avatars/" +
                           Utils.MD5Helper.HashPassword(uid, DateTime.Now.ToString("yyyyMMddHHmmss")) 
                           + extension;
            var path = Path.Combine(_webHostEnvironment.WebRootPath, fileName);
            
            var fs = new FileStream(path, FileMode.Create);
            file.CopyTo(fs);
            fs.Close();

            var u = _userServices.GetObject(uid);

            u.Avatar = fileName;
            _userServices.SaveObject(u);

            return Ok();
        }
    }
}