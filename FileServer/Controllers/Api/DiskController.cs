using FileServer.Filters;
using FileServer.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FileServer.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    [ServerAuthorization]
    public class DiskController : ControllerBase
    {
        internal static long GetFreeSpace()
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            //// Get the max size drive
            foreach (DriveInfo drive in allDrives)
            {
                if (drive.Name == "/")
                {
                    return drive.AvailableFreeSpace;
                }
            }
            return -1;
        }
        internal static long GetTotalSpace()
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            foreach (DriveInfo drive in allDrives)
            {
                if (drive.Name == "/")
                {
                    return drive.TotalSize;
                }
            }
            return -1;
        }
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new GetFreeDiskSpaceResponse
            {
                Message = "This is free disk space",
                Value = GetFreeSpace()
            });
        }
    }
}