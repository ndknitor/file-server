using FileServer.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using FileServer.Models.Entities;

namespace FileServer.Controllers;

[ApiController]
[Route("[controller]")]
public class FileController : ControllerBase
{
    private readonly IHostEnvironment environment;
    private readonly FileServerContext context;
    private readonly IConfiguration configuration;
    public FileController(IHostEnvironment environment, FileServerContext context, IConfiguration configuration)
    {
        this.configuration = configuration;
        this.context = context;
        this.environment = environment;
    }
    [EnableCors]
    [HttpGet("{fileName}")]
    public async Task<IActionResult> GetFile([FromRoute] string fileName, [FromServices] IServiceProvider service)
    {
        string selfNode = configuration.GetValue<string>("SelfNode");
        AppFile file = context.AppFile.FirstOrDefault(f => f.Name == fileName);
        if (file == null)
        {
            return NotFound();
        }
        if (file.Node == selfNode)
        {
            string path = Path.Combine(environment.ContentRootPath, "wwwroot", fileName);
            if (System.IO.File.Exists(path))
            {
                return PhysicalFile(path, MimeMapping.MimeUtility.GetMimeMapping(path));
            }
            return NotFound();
        }
        Dictionary<string, string> nodes = configuration.GetSection("Nodes").GetChildren().ToDictionary(x => x.Key, x => x.Value);
        HttpClient client = service.GetService<HttpClient>();
        Stream fileStream = await client.GetStreamAsync($"{nodes[file.Node]}/file/{fileName}");
        return File(fileStream, MimeMapping.MimeUtility.GetMimeMapping(fileName));
    }
}
