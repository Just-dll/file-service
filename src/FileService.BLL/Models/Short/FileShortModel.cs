using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.BLL.Models.Short
{
    public class FileShortModel
    {
        public uint Id { get; set; }
        public required string Name { get; set; }
        public required string Size { get; set; }
        public DateOnly CreationDate { get; set; }
    }
}
