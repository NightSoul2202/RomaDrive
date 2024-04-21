using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RomaDrive.Models;
using System.Diagnostics;

namespace RomaDrive.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private const string UploadDirectory = "C:\\UploadedFiles\\DoneFile";
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(ILogger<HomeController> logger, UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            string directoryPath = $"{UploadDirectory}\\{_userManager.GetUserId(this.User)}";
            ViewBag.DirectoryPath = directoryPath;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
