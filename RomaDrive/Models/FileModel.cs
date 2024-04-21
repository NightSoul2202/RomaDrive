using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RomaDrive.Models
{
    public class FileModel
    {
        [Key]
        public int IdFile { get; set; }

        public string Title { get; set; }

        public string Path { get; set; }

        [ForeignKey(nameof(UserId))]
        public IdentityUser identityUser { get; set; }
        public string UserId { get; set; }
    }
}
