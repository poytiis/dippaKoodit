using BrowserBackEnd.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BrowserBackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoreController : ControllerBase
    {
        private readonly IStoreService _storeService;
        public StoreController(IStoreService storeService)
        {
            _storeService = storeService;
        }

        [HttpGet]
        [Route("RunStoreComparison")]
        public async Task<IActionResult> RunStoreComparison()
        {
            var resultFile = await _storeService.RunStoreComparison();
            return Ok(resultFile);
        }

        [HttpGet]
        [Route("RunStoreLargeFilesInSQLVersusFilesystem")]
        public async Task<IActionResult> RunStoreLargeFilesInSQLVersusFilesystem()
        {
            await _storeService.RunStoreLargeFilesInSQLVersusFilesystem();
            return Ok();
        }



    }
}
