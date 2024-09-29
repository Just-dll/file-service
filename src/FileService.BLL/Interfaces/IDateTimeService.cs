
namespace FileService.BLL.Interfaces; 

public interface IDateTimeService
{
    public DateTime DateTimeNow { get; }
    public DateOnly DateOnlyNow { get; }
}