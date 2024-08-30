using AutoMapper;
using FileService.BLL.Models;
using FileService.BLL.Models.Short;
using FileService.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.BLL.MapperProfiles
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<DAL.Entities.File, FileShortModel>()
                .ForMember(fsm => fsm.CreationDate, opt => opt.MapFrom(f => DateOnly.FromDateTime(f.CreationDate)))
                .ForMember(fsm => fsm.Size, opt => opt.MapFrom(f => ConvertSize(f.Size)));

            CreateMap<DAL.Entities.File, FileModel>()
                .ForMember(fm => fm.FilePath, opt => opt.Ignore())
                .ForMember(fm => fm.Size, opt => opt.MapFrom(f => ConvertSize(f.Size)));

            CreateMap<Folder, FolderShortModel>();

            CreateMap<Folder, FolderModel>();

            CreateMap<FolderModel, Folder>()
                .ForMember(fm => fm.InnerFolders, opt => opt.Ignore())
                .ForMember(fm => fm.Files, opt => opt.Ignore());

            CreateMap<User, UserModel>()
                .ForMember(um => um.Id, opt => opt.MapFrom(u => u.IdentityGuid));

            CreateMap<UserModel, User>()
                .ForMember(um => um.UserName, opt => opt.Ignore());

            CreateMap<User, UserShortModel>()
                .ForMember(usm => usm.Id, opt => opt.MapFrom(u => u.IdentityGuid));

            CreateMap<UserAccess, AccessModel>()
                .ForMember(fam => fam.FolderName, opt => opt.MapFrom(ua => ua.Folder.Name))
                .ForMember(fam => fam.User, opt => opt.MapFrom(ua => ua.User))
                .ForMember(fam => fam.Permissions, opt => opt.MapFrom(ua => ua.AccessFlags.ToString()));

        }

        private static string ConvertSize(long size)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = size;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }


    }
}
