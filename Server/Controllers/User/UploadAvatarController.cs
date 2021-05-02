using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartProctor.Server.Services;

namespace SmartProctor.Server.Controllers.User
{
    /// <summary>
    /// Controller used for uploading avatar image for the user, this will be used by a file upload input element,
    /// so it should return status in HTTP status code.
    /// </summary>
    [ApiController]
    [Route("api/user/[controller]")]
    public class UploadAvatarController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IUserServices _userServices;

        public UploadAvatarController(IWebHostEnvironment hostEnvironment, IUserServices userServices)
        {
            _webHostEnvironment = hostEnvironment;
            _userServices = userServices;
        }
        
        [HttpPost]
        public ActionResult Post(IFormFile file)
        {
            if (User.Identity?.Name == null)
            {
                // Not logged in 
                return BadRequest();
            }

            var uid = User.Identity.Name;

            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!".bmp/.jpg/.jpeg/.png/.gif".Contains(extension))
            {
                // Illegal file extension.
                return BadRequest();
            }

            // Generate file name, file name will be generate with the user ID and time.
            var fileName = "img/avatars/" +
                           Utils.MD5Helper.HashPassword(uid, DateTime.Now.ToString("yyyyMMddHHmmss")) 
                           + extension;
            var path = Path.Combine(_webHostEnvironment.WebRootPath, fileName);
            
            var fs = new FileStream(path, FileMode.Create);
            file.CopyTo(fs);
            fs.Close();

            var u = _userServices.GetObject(uid);

            // Update the avatar path in user database
            u.Avatar = fileName;
            _userServices.SaveObject(u);

            return Ok();
        }
    }
}