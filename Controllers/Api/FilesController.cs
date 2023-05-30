using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using FileServer.Models.Request;
using FileServer.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FileServer.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly IHostEnvironment environment;
    public FilesController(IHostEnvironment environment)
    {
        this.environment = environment;
    }
    private async Task<string> SHA512Hash(Stream stream)
    {
        return BitConverter.ToString(await SHA512.Create().ComputeHashAsync(stream)).ToLower().Replace("-", null);
    }
    [HttpGet("{fileName}")]
    public async Task<IActionResult> GetFile()
    {
        return Ok();
    }
    [HttpPost]
    public async Task<IActionResult> CreateFile([FromForm] CreateFileRequest request, [FromServices] IServiceProvider service)
    {
        Stream stream = request.File.OpenReadStream();
        string hash = await SHA512Hash(stream),
        extension = "." + request.File.FileName.Split(".").Last(),
        fileName = hash + extension,
        path = Path.Combine(environment.ContentRootPath, "wwwroot", fileName);
        if (System.IO.File.Exists(path))
        {
            return Accepted(new StandardResponse
            {
                Message = "File already exsited"
            });
        }
        try
        {
            using (Stream fileStream = new FileStream(path, FileMode.Create))
            {
                await request.File.CopyToAsync(fileStream);
                return Ok(new CreateFileResponse
                {
                    Message = "Create file sucessfully",
                    Name = fileName,
                    Url = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}/file/{fileName}"
                });
            }
        }
        catch (IOException e)
        {
            uint unsignedResult = (uint)e.HResult;
            if (new uint[] { 0x80000027, 0x80000070, 0x80070027, 0x80070070 }.Contains(unsignedResult))
            {

                // Full disks
                //HttpClient client = service.GetService<HttpClient>();
                return Ok();
            }
            else
                throw;

        }
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
