using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.BLL.Models
{
    public class FileModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Size { get; set; }
        public required string FilePath { get; set; }
        public DateTime CreationDate { get; set; }
        // public DateTime ModificationDate { get; set; }
        // public string? ModifierName { get; set; }
        public bool IsValid()
        {
            // Check if the folder name contains any invalid characters.
            return !Path.GetInvalidPathChars().Any(c => this.Name.Contains(c));
        }
    }
}
