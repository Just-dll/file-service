using FileService.BLL.Models.Short;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.BLL.Models
{
    public class AccessModel
    {
        public required string FolderName { get; set; }
        public required string Permissions { get; set; }
        public required UserShortModel User { get; set; }
    }
}