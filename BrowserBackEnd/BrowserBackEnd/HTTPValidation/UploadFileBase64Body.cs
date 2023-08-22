using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BrowserBackEnd.HTTPValidation
{
    public class UploadFileBase64Body : IValidatableObject
    {
        [Required]
        public string FileName { get; set; }
        public string PieceData { get; set; }
        public int PieceNumber { get; set; }
        public string id { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return new List<ValidationResult>();
        }
    }
}
