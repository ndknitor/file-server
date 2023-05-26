using Microsoft.AspNetCore.Mvc;

namespace FileServer.Controllers;

[ApiController]
[Route("[controller]")]
public class FileController : ControllerBase
{
    private readonly IHostEnvironment environment;
    public FileController(IHostEnvironment environment)
    {
        this.environment = environment;
    }
    [HttpGet("{fileName}")]
    public IActionResult GetFile(string fileName)
    {
        string path = Path.Combine(environment.ContentRootPath, "wwwroot", fileName);
        if (System.IO.File.Exists(path))
        {
            return PhysicalFile(path, MimeMapping.MimeUtility.GetMimeMapping(path));
        }
        return NotFound();
    }
}
