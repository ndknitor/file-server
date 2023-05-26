using System.Diagnostics;
using System.Security.Cryptography;
using FileServer.Models.Request;
using FileServer.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FileServer.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpGet("{identity}")]
    public IActionResult Getuser(string identity)
    {
        return Ok();
    }
    [HttpPost]
    public IActionResult CreateUser()
    {
        return Ok();
    }
    [HttpPut("{identity}")]
    public IActionResult UpdateUser([FromRoute] string identity)
    {
        return Ok();
    }
    [HttpDelete("{identity}")]
    public IActionResult Delete([FromRoute] string identity)
    {
        return Ok();
    }
}