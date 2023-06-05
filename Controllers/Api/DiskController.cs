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
            string maxFreeSpaceDrive = string.Empty;
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            //// Get the max size drive
            foreach (DriveInfo drive in allDrives)
            {
                if (drive.Name == "/run/user/1000")
                {
                    return drive.AvailableFreeSpace;
                    //Console.WriteLine(drive.TotalFreeSpace);
                }
            }
            return -1;
        }
        internal static long GetTotalSpace()
        {
            string maxFreeSpaceDrive = string.Empty;
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            //// Get the max size drive
            foreach (DriveInfo drive in allDrives)
            {
                if (drive.Name == "/run/user/1000")
                {
                    return drive.TotalSize;
                    //Console.WriteLine(drive.TotalFreeSpace);
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