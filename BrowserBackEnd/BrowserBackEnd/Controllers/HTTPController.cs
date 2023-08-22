using BrowserBackEnd.HTTPValidation;
using BrowserBackEnd.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BrowserBackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HTTPController : ControllerBase
    {
        private readonly IUploadService _uploadService;
        public HTTPController(IUploadService uploadService)
        {
            _uploadService = uploadService;
        }

        [HttpPost]
        [Route("StartUpload")]
        public IActionResult StartUpload([FromBody] StartUploadBody body)
        {
             _uploadService.StartUpload(body);
            return Ok();
        }

        [HttpPost]
        [Route("UploadFilePieceArray")]
        public async Task<IActionResult> UploadFilePieceArray([FromBody] UploadFilePieceArrayBody body)
        {
            await _uploadService.UploadFilePieceArray(body);
            return Ok();
        }

        [HttpPost]
        [Route("UploadFilePieceByteArray")]
        public async Task<IActionResult> UploadFilePieceByteArray([FromBody] UploadFilePieceByteArrayBody body)
        {
            await _uploadService.UploadFilePieceByteArray(body);

            return Ok();
        }

        [HttpPost]
        [Route("UploadFilePieceBase64")]
        public async Task<IActionResult> UploadFilePieceBase64([FromBody] UploadFileBase64Body body)
        {
            await _uploadService.UploadFilePieceBase64(body);
            return Ok();
        }

        [HttpPost]
        [Route("UploadFilePieceForm")]
        public async Task<IActionResult> UploadFilePieceForm([FromForm] UploadFilePieceForm body)
        {
            await _uploadService.UploadFilePieceForm(body);
            return Ok();
        }

        [HttpPost]
        [Route("FinishUpload")]
        public IActionResult FinishUpload([FromBody] FinishUploadBody body)
        {
            _uploadService.FinishUpload(body);
            return Ok();
        }

        [HttpGet]
        [Route("")]
        public IActionResult Check()
        {
            return Ok("Works");
        }
    }

    
}
