using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RomaDrive.Data;
using RomaDrive.Services;
using System.IO;
using System.Threading.Tasks;

namespace RomaDrive.Controllers
{
    [Authorize]
    public class GetFilesController : Controller
    {
        private const string UploadDirectory = "C:\\UploadedFiles";
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public GetFilesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var dbFileController = new DBControllerService(_context, _userManager);
            var files = dbFileController.GetFiles(this.User);
            return View(files);
        }

        [HttpGet]
        public async Task<IActionResult> Download(int id)
        {
            var file = await _context.Files.FindAsync(id);
            if (file == null)
            {
                return NotFound();
            }

            var filePath = Path.Combine(UploadDirectory, $"DoneFile\\{_userManager.GetUserId(this.User)}\\{file.Title}");
            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            return File(memory, "application/octet-stream", Path.GetFileName(filePath));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var file = await _context.Files.FindAsync(id);
            if (file == null)
            {
                return NotFound();
            }

            // Видаліть файл з файлової системи
            var filePath = Path.Combine(UploadDirectory, $"DoneFile\\{_userManager.GetUserId(this.User)}\\{file.Title}");
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            var dbFile = new DBControllerService(_context, _userManager);
            // Видаліть файл з бази даних
            dbFile.DeleteFile(file);

            return RedirectToAction(nameof(Index));
        }
    }
}
