using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BrowserBackEnd.HTTPValidation
{
    public class UploadFilePieceByteArrayBody : IValidatableObject
    {
        [Required]
        public string FileName { get; set; }
        public byte[] PieceData { get; set; }
        public int PieceNumber { get; set; }
        public string id { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return new List<ValidationResult>();
        }
    }
}
