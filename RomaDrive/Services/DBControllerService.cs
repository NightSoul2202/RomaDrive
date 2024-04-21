using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RomaDrive.Data;
using RomaDrive.Models;
using System.Security.Claims;

namespace RomaDrive.Services
{
    public class DBControllerService
    {
        private readonly ApplicationDbContext _fileContext;
        private readonly UserManager<IdentityUser> _userManager;

        public DBControllerService(ApplicationDbContext fileContext, UserManager<IdentityUser> userManager)
        {
            _fileContext = fileContext;
            _userManager = userManager;
        }

        public List<FileModel> GetFiles(ClaimsPrincipal currentUser)
        {
            return _fileContext.Files.Where(file => file.UserId == _userManager.GetUserId(currentUser)).ToList();
        }

        //public FileModel GetFile(int id)
        //{
        //    return _fileContext.Files.FirstOrDefault(x => x.IdFile == id);
        //}

        public bool AddFile(FileModel file)
        {
            try
            {
                _fileContext.Files.Add(file);
                _fileContext.SaveChanges();
                return true;
            }
            catch (DbUpdateException)
            {
                return false;
            }
        }

        //public bool UpdateFile(FileModel file)
        //{
        //    try
        //    {
        //        _fileContext.Entry(file).State = EntityState.Modified;
        //        _fileContext.SaveChanges();
        //        return true;
        //    }
        //    catch (DbUpdateException)
        //    {
        //        return false;
        //    }
        //}

        public bool DeleteFile(FileModel file)
        {
            if (file != null)
            {
                _fileContext.Files.Remove(file);
                _fileContext.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
