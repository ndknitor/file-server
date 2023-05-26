using System.ComponentModel.DataAnnotations;

namespace FileServer.Models.Request
{
    public class CreateFileRequest
    {
        [Required]
        public IFormFile File { get; set; }
    }
}