using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartProctor.Server.Services;
using SmartProctor.Server.Utils;
using SmartProctor.Shared.Responses;

namespace SmartProctor.Server.Controllers.Exam
{
    [ApiController]
    [Route("api/exam/[controller]")]
    public class UploadEventAttachmentController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IUserServices _userServices;

        public UploadEventAttachmentController(IWebHostEnvironment webHostEnvironment, IUserServices userServices)
        {
            _webHostEnvironment = webHostEnvironment;
            _userServices = userServices;
        }
        
        [HttpPost]
        public BaseResponseModel Post(IFormFile file)
        {
            if (User.Identity?.Name == null)
            {
                return ErrorCodes.CreateSimpleResponse(ErrorCodes.NotLoggedIn);
            }

            var uid = User.Identity.Name;

            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!".bmp/.jpg/.jpeg/.png/.gif".Contains(extension))
            {
                return ErrorCodes.CreateSimpleResponse(ErrorCodes.UnknownError);
            }

            var fileName = "img/event/" +
                           Utils.MD5Helper.HashPassword(uid, DateTime.Now.ToString("yyyyMMddHHmmss")) 
                           + extension;
            var path = Path.Combine(_webHostEnvironment.WebRootPath, fileName);
            
            var fs = new FileStream(path, FileMode.Create);
            file.CopyTo(fs);
            fs.Close();
            
            return new UploadEventAttachmentResponseModel
            {
                Code = ErrorCodes.Success,
                FileName = fileName
            };
        }
    }
}