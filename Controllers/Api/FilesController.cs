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
    private Dictionary<string, string> GetNodes()
    {
        return configuration.GetSection("Nodes").GetChildren().ToDictionary(x => x.Key, x => x.Value);
    }
    private string GetUrl(string fileName)
    {
        return $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}/file/{fileName}";
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
        string url = GetUrl(fileName);
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
                    AvalibleSpace = DiskController.GetFreeSpace(),
                    TotalSpace = DiskController.GetTotalSpace()
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
        NodeSpace nodeSpace = context.NodeSpace.OrderByDescending(n => n.AvalibleSpace).FirstOrDefault(n => n.AvalibleSpace > request.File.Length);
        if (nodeSpace == null)
        {
            return Accepted(new StandardResponse
            {
                Message = "Server space is full, contact to administrator for an upgrade"
            });
        }
        HttpClient client = service.GetService<HttpClient>();
        MultipartFormDataContent content = new MultipartFormDataContent();
        content.Add(new StringContent(url), "url");
        content.Add(new StreamContent(stream), "file");
        Dictionary<string, string> nodes = GetNodes();
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
    [HttpPut("{oldFileName}")]
    public async Task<IActionResult> UpdateFile([FromForm] CreateFileRequest request, [FromRoute] string oldFileName)
    {
        AppFile file = context.AppFile.FirstOrDefault(f => f.Name == oldFileName);
        if (file == null)
        {
            return NotFound();
        }
        Stream stream = request.File.OpenReadStream();
        string hash = await SHA512Hash(stream),
        extension = "." + request.File.FileName.Split(".").Last(),
        newFileName = hash + extension,
        path = Path.Combine(environment.ContentRootPath, "wwwroot", oldFileName);
        if (oldFileName == newFileName)
        {
            return Accepted(new StandardResponse
            {
                Message = "Same file can not be acecpted"
            });
        }
        string url = GetUrl(newFileName);
        if (DiskController.GetFreeSpace() > request.File.Length)
        {
            string selfNode = configuration.GetValue<string>("SelfNode");
            using (Stream fileStream = new FileStream(path, FileMode.Create))
            {
                await request.File.CopyToAsync(fileStream);
                context.NodeSpace.Update(new NodeSpace
                {
                    Node = selfNode,
                    AvalibleSpace = DiskController.GetFreeSpace(),
                    TotalSpace = DiskController.GetTotalSpace()
                });
                await context.SaveChangesAsync();
                return Ok(new CreateFileResponse
                {
                    Message = "Create file sucessfully",
                    Name = newFileName,
                    Url = url
                });
            }
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
    public async Task<IActionResult> DeleteFile([FromRoute] string fileName, [FromServices] IServiceProvider service)
    {
        AppFile file = context.AppFile.FirstOrDefault(f => f.Name == fileName);
        if (file == null)
        {
            return NotFound();
        }
        string selfNode = configuration.GetValue<string>("SelfNode");
        if (selfNode == file.Node)
        {
            string path = Path.Combine(environment.ContentRootPath, "wwwroot", fileName);
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
                context.AppFile.Remove(new AppFile
                {
                    Name = fileName,
                    Node = selfNode
                });
                await context.SaveChangesAsync();
                return Ok(new StandardResponse
                {
                    Message = "Delete file sucessfully"
                });
            }
        }
        Dictionary<string, string> nodes = GetNodes();
        HttpClient client = service.GetService<HttpClient>();
        HttpResponseMessage message = await client.DeleteAsync($"{nodes[file.Node]}/api/files/{fileName}");
        if (message.IsSuccessStatusCode)
        {
            return Ok(new StandardResponse
            {
                Message = "Delete file sucessfully"
            });
        }
        return BadRequest();
    }
}
