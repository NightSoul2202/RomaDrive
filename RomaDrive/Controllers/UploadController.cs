using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RomaDrive.Data;
using RomaDrive.Models;
using RomaDrive.Services;

namespace RomaDrive.Controllers
{
    [Authorize]
    public class UploadController : Controller
    {
        private const string UploadDirectory = "C:\\UploadedFiles";
        private string FinalFileName = "default_name";
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private bool isRewriteFile = false;

        public UploadController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [Route("Upload/IsExists")]
        public IActionResult IsExistsFile()
        {
            FinalFileName = Request.Form["fileName"];
            string filePath = $"{UploadDirectory}/DoneFile/{_userManager.GetUserId(this.User)}/{FinalFileName}";
            if (System.IO.File.Exists(filePath))
            {
                // Якщо файл існує, запитати користувача, чи хоче він перезаписати файл
                return BadRequest();
            }
            return Ok();
        }


        [HttpPost]
        [Route("Upload/Chunk")]
        public async Task<IActionResult> UploadChunk()
        {
            if (Request.Form.Files.Count > 0)
            {
                var file = Request.Form.Files[0];

                var chunkNumber = int.Parse(Request.Form["chunkNumber"]);
                var totalChunks = int.Parse(Request.Form["totalChunks"]);
                isRewriteFile = Boolean.Parse(Request.Form["isRewriteFile"]);
                FinalFileName = Request.Form["fileName"];

                string ss = $"{UploadDirectory}/DoneFile/{_userManager.GetUserId(this.User)}/{FinalFileName}";
                //if(System.IO.File.Exists(ss))
                //{
                //    string confirmationScript = "if(confirm('Файл вже існує. Чи хочете перезаписати його?')){ return true; } else { return false; }";
                //    return Content("<script type='text/javascript'>" + confirmationScript + "</script>");
                //}

                if (chunkNumber == 0)
                {
                    string[] filesWithChunk = Directory.GetFiles(UploadDirectory)
                                                      .Where(fileSearch => fileSearch.Contains("_CHUNK"))
                                                      .ToArray();

                    if (filesWithChunk.Length > 0)
                    {
                        foreach (string fileSearch in filesWithChunk)
                        {
                            System.IO.File.Delete(fileSearch);
                        }
                    }
                }

                var chunkFilePath = GetChunkFilePath(chunkNumber);
                using (var fileStream = new FileStream(chunkFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                await MergeChunk(chunkFilePath, chunkNumber, totalChunks);

                if (chunkNumber < totalChunks)
                {
                    // Delete the chunk file after merging
                    System.IO.File.Delete(chunkFilePath);
                }

                return Ok();

            }

            return BadRequest("No file uploaded.");
        }

        private string GetChunkFilePath(int chunkNumber)
        {
            return Path.Combine(UploadDirectory, $"{FinalFileName}_{chunkNumber}_CHUNK");
        }

        private async Task MergeChunk(string chunkFilePath, int chunkNumber, int totalChunks)
        {
            var pathToUserDir = Path.Combine(UploadDirectory, $"DoneFile\\{_userManager.GetUserId(this.User)}");
            var finalFilePath = Path.Combine(pathToUserDir, $"{FinalFileName}");
            if(!Directory.Exists(pathToUserDir)) {
                Directory.CreateDirectory(pathToUserDir);
            }
            if (chunkNumber == 0)
            {
                // This is the first chunk, create the final file
                using (var finalFileStream = new FileStream(finalFilePath, FileMode.Create))
                {
                    using (var chunkFileStream = new FileStream(chunkFilePath, FileMode.Open))
                    {
                        await chunkFileStream.CopyToAsync(finalFileStream);
                    }
                }
            }
            else
            {
                // Append chunk to the final file
                using (var finalFileStream = new FileStream(finalFilePath, FileMode.Append))
                {
                    using (var chunkFileStream = new FileStream(chunkFilePath, FileMode.Open))
                    {
                        await chunkFileStream.CopyToAsync(finalFileStream);
                    }
                }
            }

            if ((chunkNumber == totalChunks - 1) && !isRewriteFile)
            {
                //isRewriteFile = false;
                FileModel fileModel = new FileModel
                {
                    Title = FinalFileName,
                    Path = finalFilePath + FinalFileName,
                    UserId = _userManager.GetUserId(this.User)
                };

                var dbFile = new DBControllerService(_context, _userManager);
                dbFile.AddFile(fileModel);
            }
        }
    }
}