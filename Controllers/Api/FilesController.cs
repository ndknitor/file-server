using System.Security.Cryptography;
using FileServer.Context;
using FileServer.Filters;
using FileServer.Models.Entities;
using FileServer.Models.Request;
using FileServer.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FileServer.Controllers;

[ApiController]
[Route("/api/[controller]")]
[ServerAuthorization]
public class FilesController : ControllerBase
{
    private readonly IHostEnvironment environment;
    private readonly FileServerContext context;
    private readonly IConfiguration configuration;
    public FilesController(IHostEnvironment environment, FileServerContext context, IConfiguration configuration)
    {
        this.configuration = configuration;
        this.context = context;
        this.environment = environment;
    }
    private async Task<string> SHA512Hash(Stream stream)
    {
        return BitConverter.ToString(await SHA512.Create().ComputeHashAsync(stream)).ToLower().Replace("-", null);
    }
    [HttpGet("{fileName}")]
    public async Task<IActionResult> GetFile([FromRoute] string fileName)
    {
        AppFile file = context.AppFile.FirstOrDefault(f => f.Name == fileName);
        if (file == null)
        {
            return NotFound();
        }
        return Ok(new GetFileResponse
        {
            Message = "File found",
            Name = fileName
        });
    }
    [HttpPost]
    public async Task<IActionResult> CreateFile([FromForm] CreateFileRequest request, [FromServices] IServiceProvider service)
    {
        Stream stream = request.File.OpenReadStream();
        string hash = await SHA512Hash(stream),
        extension = "." + request.File.FileName.Split(".").Last(),
        fileName = hash + extension,
        path = Path.Combine(environment.ContentRootPath, "wwwroot", fileName);
        string url = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}/file/{fileName}";
        if (context.AppFile.FirstOrDefault(f => f.Name == fileName) != null)
        {
            return Accepted(new CreateFileResponse
            {
                Message = "File already exsited",
                Name = fileName,
                Url = url
            });
        }
        if (DiskController.GetFreeSpace() > request.File.Length)
        {
            string selfNode = configuration.GetValue<string>("SelfNode");
            using (Stream fileStream = new FileStream(path, FileMode.Create))
            {
                await request.File.CopyToAsync(fileStream);
                context.NodeSpace.Update(new NodeSpace
                {
                    Node = selfNode,
                    AvalibleSpace = DiskController.GetFreeSpace()
                });
                await context.SaveChangesAsync();
                return Ok(new CreateFileResponse
                {
                    Message = "Create file sucessfully",
                    Name = fileName,
                    Url = url
                });
            }
        }
        NodeSpace nodeSpace = context.NodeSpace.FirstOrDefault(n => n.AvalibleSpace > request.File.Length);
        if (nodeSpace.AvalibleSpace == 0)
        {
            return Accepted(new StandardResponse
            {
                Message = "Server space is full, contact to administrator for an upgrade"
            });
        }
        HttpClient client = service.GetService<HttpClient>();
        Dictionary<string, string> nodes = configuration.GetSection("Nodes").GetChildren().ToDictionary(x => x.Key, x => x.Value);
        MultipartFormDataContent content = new MultipartFormDataContent();
        content.Add(new StringContent(url), "url");
        content.Add(new StreamContent(stream), "file");
        HttpResponseMessage message = await client.PostAsync(nodes[nodeSpace.Node], content);
        if (message.IsSuccessStatusCode)
        {
            return Ok(new CreateFileResponse
            {
                Message = "Create file sucessfully",
                Name = fileName,
                Url = url
            });
        }
        return BadRequest();

        // try
        // {
        //     using (Stream fileStream = new FileStream(path, FileMode.Create))
        //     {
        //         await request.File.CopyToAsync(fileStream);
        //         return Ok(new CreateFileResponse
        //         {
        //             Message = "Create file sucessfully",
        //             Name = fileName,
        //             Url = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}/file/{fileName}"
        //         });
        //     }
        // }
        // catch (IOException e)
        // {
        //     uint unsignedResult = (uint)e.HResult;
        //     if (new uint[] { 0x80000027, 0x80000070, 0x80070027, 0x80070070 }.Contains(unsignedResult))
        //     {
        //         // Full disks
        //         //HttpClient client = service.GetService<HttpClient>();
        //         return Ok();
        //     }
        //     else
        //         throw;

        // }
    }
    [HttpPut("{fileName}")]
    public async Task<IActionResult> UpdateFile([FromForm] CreateFileRequest request, [FromRoute] string oldFileName)
    {
        Stream stream = request.File.OpenReadStream();
        string hash = await SHA512Hash(stream),
        extension = "." + request.File.FileName.Split(".").Last(),
        newFileName = hash + extension,
        path = Path.Combine(environment.ContentRootPath, "wwwroot", oldFileName);
        if (oldFileName == newFileName)
        {
            return BadRequest(new StandardResponse
            {
                Message = "Same file can not be acecpted"
            });
        }
        if (!System.IO.File.Exists(path))
        {
            return BadRequest(new StandardResponse
            {
                Message = "File not found"
            });
        }
        string newPath = Path.Combine(environment.ContentRootPath, "wwwroot", newFileName);
        try
        {
            using (Stream fileStream = new FileStream(newPath, FileMode.Create))
            {
                await request.File.CopyToAsync(fileStream);

                return Ok(new CreateFileResponse
                {
                    Message = "Updated file sucessfully",
                    Name = newFileName,
                    Url = HttpContext.Request.Scheme + "://" + HttpContext.Request.Host.Value + "/file/" + newFileName
                });
            }
        }
        catch (System.Exception)
        {
            throw;
        }
    }
    [HttpDelete("{fileName}")]
    public IActionResult DeleteFile([FromRoute] string fileName)
    {
        string path = Path.Combine(environment.ContentRootPath, "wwwroot", fileName);
        if (System.IO.File.Exists(path))
        {
            System.IO.File.Delete(path);
            return Ok(new StandardResponse
            {
                Message = "Delete file sucessfully"
            });
        }
        return NotFound(new StandardResponse
        {
            Message = "File not found"
        });

    }
}
