using System.ComponentModel.DataAnnotations;

namespace FileServer.Models.Request
{
    public class CreateFileRequest
    {
        [Required]
        public IFormFile File { get; set; }
        [MaxLength(128)]
        public string[] Authorizations { get; set; }
    }
}