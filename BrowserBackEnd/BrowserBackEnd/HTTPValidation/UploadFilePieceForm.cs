using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BrowserBackEnd.HTTPValidation
{
    public class UploadFilePieceForm
    {
        public string FileName { get; set; }
        public IFormFile PieceData { get; set; }
        public int PieceNumber { get; set; }
    }
}
