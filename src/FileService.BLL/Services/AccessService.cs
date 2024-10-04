using AutoMapper;
using FileService.BLL.Interfaces;
using FileService.BLL.Models;
using FileService.BLL.Models.Short;
using FileService.DAL.Data;
using FileService.DAL.Entities;
using FileService.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.BLL.Services
{
    public class AccessService : IAccessService
    {
        private readonly IIdentityService _identityService;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public AccessService(IIdentityService identityService, IMapper mapper, IUnitOfWork unitOfWork)
        {
            _identityService = identityService;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<AccessModel>> GetAccessors(uint folderId)
        {
            var userAccesses = await _unitOfWork.UserAccessRepository.GetFolderAccessors(folderId);
            return userAccesses.Select(ua => _mapper.Map<AccessModel>(ua));
        }

        public async Task<AccessModel> GiveAccess(uint folderId, AccessShortModel model)
        {
            var user = await _identityService.GetUserByEmail(model.Email) ?? throw new KeyNotFoundException();
            
            var folder = await _unitOfWork.FolderRepository.GetFolderByIdAsync(folderId) ?? throw new KeyNotFoundException();

            var userAccess = new UserAccess()
            {
                FolderId = folder.Id,
                UserId = user.Id,
                AccessFlags = model.Permission,
                User = user,
                Folder = folder
            };

            _unitOfWork.UserAccessRepository.AddUserAccess(userAccess);

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<AccessModel>(userAccess);
        }

        public async Task<bool> GetAccessVerification(Guid userId, uint folderId, AccessPermission requiredPermission)
        {
            var user = await _unitOfWork.UserRepository.GetUserById(userId) ?? throw new UnauthorizedAccessException();

            var folderPath = await _unitOfWork.FolderRepository.GetOuterFoldersAsync(folderId);

            return user.UserAccesses
                .Where(ua => folderPath.Select(f => f.Id).Contains(ua.FolderId))
                .Any(ua => ua.AccessFlags.HasFlag(AccessPermission.Owner) || ua.AccessFlags.HasFlag(requiredPermission));
        }
        public async Task<AccessModel> UpdateAccess(uint folderId, AccessShortModel model)
        {
            var user = await _unitOfWork.UserRepository.GetUserByEmail(model.Email) ?? throw new InvalidOperationException();

            var currAccess = await _unitOfWork.UserAccessRepository.GetUserAccessAsync(folderId, user.IdentityGuid) ?? throw new KeyNotFoundException();

            if (currAccess.AccessFlags.HasFlag(AccessPermission.Owner) ^ model.Permission.HasFlag(AccessPermission.Owner))
            {
                throw new InvalidOperationException("Change of ownership is not allowed");
            }

            currAccess.AccessFlags = model.Permission;

            _unitOfWork.UserAccessRepository.UpdateUserAccess(currAccess);

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<AccessModel>(currAccess);
        }


        public async Task DeleteAccess(uint folderId, string email)
        {
            var user = await _unitOfWork.UserRepository.GetUserByEmail(email);

            if (user == null)
            {
                return;
            }

            var userAccess = user.UserAccesses.FirstOrDefault(ua => ua.FolderId == folderId);

            if(userAccess != null)
            {
                if(userAccess.AccessFlags.HasFlag(AccessPermission.Owner))
                {
                    throw new InvalidOperationException("Deletion of ownership is not allowed");
                }

                _unitOfWork.UserAccessRepository.DeleteUserAccess(userAccess);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<FolderModel>> GetAccessibleFoldersAsync(Guid userId)
        {
            var user = await _unitOfWork.UserRepository.GetUserById(userId) ?? throw new UnauthorizedAccessException();
            return user.UserAccesses.Select(ua => _mapper.Map<FolderModel>(ua.Folder));
        }

        public async Task<IEnumerable<FolderShortModel>> GetFolderAccessiblePath(Guid userId, uint folderId)
        {
            var folderPath = (await _unitOfWork.FolderRepository.GetOuterFoldersAsync(folderId)).ToList();
            
            if (folderPath == null || folderPath.Count == 0)
            {
                throw new DirectoryNotFoundException();
            }

            var user = await _unitOfWork.UserRepository.GetUserById(userId) ?? throw new UnauthorizedAccessException();

            for (int i = 0; i < folderPath.Count; i++)
            {
                if (!user.UserAccesses.Any(ua => ua.FolderId == folderPath[i].Id))
                {
                    folderPath.Remove(folderPath[i]);
                }
                else
                {
                    break;
                }
            }

            return folderPath.Select(f => _mapper.Map<FolderShortModel>(f));
        }

    }
}
