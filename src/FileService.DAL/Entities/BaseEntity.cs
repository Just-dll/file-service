using System.ComponentModel.DataAnnotations;

namespace FileService.DAL.Entities
{
    public class BaseEntity
    {
        [Key]
        public uint Id { get; set; }
    }
}