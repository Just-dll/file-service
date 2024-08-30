using FileService.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.DAL.Data
{
    public class FileServiceDbContext : DbContext
    {
        public FileServiceDbContext() : base()
        {
            
        }
        public FileServiceDbContext(DbContextOptions<FileServiceDbContext> options) : base(options)
        {
            //Database.EnsureDeleted();
            Database.EnsureCreated();
            //Database.Migrate();
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Folder> Folders { get; set; }
        public DbSet<UserAccess> UserAccesses { get; set; }
        public DbSet<Entities.File> Files { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entities.File>(e =>
            {
                e.HasKey(f => f.Id);
                e.HasOne(f => f.Folder).WithMany(f => f.Files);
            });
            modelBuilder.Entity<Folder>(e =>
            {
                e.HasKey(f => f.Id);
                e.HasOne(f => f.OuterFolder).WithMany(f => f.InnerFolders);
            });
            modelBuilder.Entity<UserAccess>(e =>
            {
                e.HasKey(ua => new { ua.UserId, ua.FolderId });
                e.HasOne(ua => ua.Folder).WithMany(f => f.Accessors);
                e.HasOne(ua => ua.User).WithMany(u => u.UserAccesses);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
