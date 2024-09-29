using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.BLL.Models
{
    public class FolderArchiveModel
    {
        public string FolderName { get; set; }
        public byte[] ArchiveData { get; set; }
    }
}
