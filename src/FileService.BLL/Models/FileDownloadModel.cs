using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.BLL.Models
{
    public class FileDownloadModel
    {
        public required string Name { get; set; }
        public required byte[] Data { get; set; }
    }
}
