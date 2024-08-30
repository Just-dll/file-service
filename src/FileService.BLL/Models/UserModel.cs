using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.BLL.Models
{
    public class UserModel
    {
        public required Guid Id { get; set; }
        public string? UserName { get; internal set; }
    }
}
