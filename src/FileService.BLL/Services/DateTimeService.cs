using FileService.BLL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.BLL.Services
{
    public class DateTimeService : IDateTimeService
    {
        public DateTime DateTimeNow => DateTime.UtcNow;

        public DateOnly DateOnlyNow => DateOnly.FromDateTime(DateTime.UtcNow);
    }
}
