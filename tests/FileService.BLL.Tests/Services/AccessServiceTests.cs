using Moq;
using Xunit;
using System.Threading.Tasks;
using FileService.BLL.Services;
using FileService.DAL.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using FileService.DAL.Entities;
using AutoMapper;
using System;
using FileService.BLL.Interfaces;
using FileService.BLL.Models.Short;
using FileService.DAL.Interfaces;
using FileService.BLL.MapperProfiles;
using AutoFixture.AutoMoq;
using AutoFixture;
using FileService.BLL.Models;

namespace FileService.BLL.Tests.Services
{
    public class AccessServiceTests
    {
        private readonly Mock<IIdentityService> _identityServiceMock;
        private readonly IMapper _mapper;
        private readonly AccessService _accessService;
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Fixture _fixture;

        public AccessServiceTests()
        {
            _fixture = new Fixture();
            _identityServiceMock = new Mock<IIdentityService>();
            _mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperProfile>()));
            _unitOfWork = new Mock<IUnitOfWork>();
            _accessService = new AccessService(_identityServiceMock.Object, _mapper, _unitOfWork.Object);
        }

        #region Previous Tests
        /*[Fact]
        public async Task GetAccessors_ShouldReturnDistinctAccessors()
        {
            // Arrange
            var folderId = 1u;
            var folderPath = new List<Folder> { new Folder { Id = folderId, Name = "Test Folder" } }.AsQueryable();
            var userAccesses = new List<UserAccess>
            {
                new UserAccess { FolderId = folderId, UserId = 1, User = new User { Id = 1, IdentityGuid = Guid.NewGuid(), Email = "test1@example.com" }, AccessFlags = AccessPermission.Owner },
                new UserAccess { FolderId = folderId, UserId = 2, User = new User { Id = 2, IdentityGuid = Guid.NewGuid(), Email = "test2@example.com" }, AccessFlags = AccessPermission.Read }
            }.AsQueryable();

            // Mock DbSet for Folders
            var folderDbSetMock = CreateDbSetMock(folderPath);
            _dbContextMock.Setup(m => m.Folders).Returns(folderDbSetMock.Object);
            _dbContextMock.Setup(m => m.Folders.FromSqlRaw(It.IsAny<string>(), It.IsAny<object[]>())).Returns(folderDbSetMock.Object);

            // Mock DbSet for UserAccesses
            var userAccessDbSetMock = CreateDbSetMock(userAccesses);
            _dbContextMock.Setup(m => m.UserAccesses).Returns(userAccessDbSetMock.Object);

            // Act
            var result = await _accessService.GetAccessors(folderId);

            // Assert
            Assert.Equal(2, result.Count());  // Ensure 2 distinct accessors
        }

        [Fact]
        public async Task GiveAccess_ShouldGrantAccess_WhenUserAndFolderExist()
        {
            // Arrange
            var folderId = 1u;
            var model = new AccessShortModel { Email = "test1@example.com", Permission = AccessPermission.Read };
            var user = new User { Id = 1, IdentityGuid = Guid.NewGuid(), Email = model.Email };
            var folder = new Folder { Id = folderId, Name = "Test Folder" };

            _identityServiceMock.Setup(s => s.GetUserByEmail(model.Email)).ReturnsAsync(user);
            _dbContextMock.Setup(m => m.Folders.FindAsync(folderId)).ReturnsAsync(folder);

            var userAccessDbSetMock = new Mock<DbSet<UserAccess>>();
            _dbContextMock.Setup(m => m.UserAccesses).Returns(userAccessDbSetMock.Object);

            // Act
            var result = await _accessService.GiveAccess(folderId, model);

            // Assert
            _dbContextMock.Verify(m => m.SaveChangesAsync(default), Times.Once);
            Assert.NotNull(result);
            Assert.Equal(user.IdentityGuid, result.User.Id);
        }


        // Helper to mock DbSet behavior
        private static Mock<DbSet<T>> CreateDbSetMock<T>(IQueryable<T> data) where T : class
        {
            var dbSetMock = new Mock<DbSet<T>>();
            dbSetMock.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            return dbSetMock;
        }
        */
        #endregion
        
        #region GetAccessibleFoldersAsync

        [Fact]
        public async Task GetAccessibleFoldersAsync_InexistantUser_ThrowsUnauthorizedException()
        {
            var userId = Guid.NewGuid();

            _unitOfWork.Setup(uow => uow.UserRepository.GetUserById(userId)).ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _accessService.GetAccessibleFoldersAsync(userId));
        }

        [Fact]
        public async Task GetAccessibleFoldersAsync_UserExists_ReturnsEnumerableOfFolders()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var accesses = _fixture.Build<UserAccess>()
                .With(ua => ua.Folder, _fixture.Build<Folder>().Without(f => f.OuterFolder).Create())
                .CreateMany(10);
            var user = _fixture.Build<User>()
                .With(u => u.IdentityGuid, userId)
                .Create();

            foreach (var ua in accesses)
            {
                user.UserAccesses.Add(ua);
            }

            _unitOfWork.Setup(uow => uow.UserRepository.GetUserById(userId)).ReturnsAsync(user);

            var expected = accesses.Select(ua => ua.Folder).Select(f => new FolderModel() { Name = f.Name, Id = (int)f.Id }).ToList();

            // Act
            var actual = await _accessService.GetAccessibleFoldersAsync(userId);

            // Assert
            Assert.Equal(expected.Count, actual.Count());

            // Custom comparison for FolderModel properties
            var expectedList = expected.OrderBy(e => e.Id).ToList();
            var actualList = actual.OrderBy(a => a.Id).ToList();

            Assert.True(expectedList.SequenceEqual(actualList, new FolderModelComparer()));
        }

        public class FolderModelComparer : IEqualityComparer<FolderModel>
        {
            public bool Equals(FolderModel x, FolderModel y)
            {
                if (x == null || y == null) return false;
                return x.Id == y.Id && x.Name == y.Name;
            }

            public int GetHashCode(FolderModel obj)
            {
                return HashCode.Combine(obj.Id, obj.Name);
            }
        }

        #endregion

        // Requires additional effort!
        #region GetFolderAccessiblePath
        [Fact]
        public async Task GetFolderAccessiblePath_FolderNotFound_ThrowsDirectoryNotFoundException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            uint folderId = 1;

            _unitOfWork.Setup(uow => uow.FolderRepository.GetOuterFoldersAsync(folderId))
                       .ReturnsAsync(Enumerable.Empty<Folder>().ToList());

            // Act & Assert
            await Assert.ThrowsAsync<DirectoryNotFoundException>(async () => await _accessService.GetFolderAccessiblePath(userId, folderId));
        }

        // Add more test cases
        [Fact]
        public async Task GetFolderAccessiblePath_UserNotFound_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            uint folderId = 1;

            _unitOfWork.Setup(uow => uow.FolderRepository.GetOuterFoldersAsync(folderId))
                       .ReturnsAsync(_fixture.Build<Folder>().Without(f => f.OuterFolder).CreateMany(10).ToList());
            _unitOfWork.Setup(uow => uow.UserRepository.GetUserById(userId))
                       .ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _accessService.GetFolderAccessiblePath(userId, folderId));
        }
        #endregion

        #region GetAccessVerification

        [Fact]
        public async Task GetAccessVerification_UserNotFound_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            uint folderId = 1;
            var permission = AccessPermission.Read;

            _unitOfWork.Setup(uow => uow.UserRepository.GetUserById(userId))
                       .ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _accessService.GetAccessVerification(userId, folderId, permission));
        }

        [Fact]
        public async Task GetAccessVerification_HasAccess_ReturnsTrue()
        {
            // Arrange
            var userId = Guid.NewGuid();
            uint folderId = 1;
            var permission = AccessPermission.Read;

            var folderPath = _fixture.Build<Folder>().Without(f => f.OuterFolder).CreateMany().ToList();

            folderPath[0].Id = folderId;
            var userAccess = _fixture.Build<UserAccess>()
                                     .Without(ua => ua.Folder)
                                     .With(ua => ua.FolderId, folderId)
                                     .With(ua => ua.AccessFlags, permission)
                                     .Create();


            var user = _fixture.Build<User>()
                                .With(u => u.IdentityGuid, userId)
                                .Create();

            user.UserAccesses.Add(userAccess);

            _unitOfWork.Setup(uow => uow.FolderRepository.GetOuterFoldersAsync(folderId))
                       .ReturnsAsync(folderPath);
            _unitOfWork.Setup(uow => uow.UserRepository.GetUserById(userId))
                       .ReturnsAsync(user);

            // Act
            var result = await _accessService.GetAccessVerification(userId, folderId, permission);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task GetAccessVerification_NoAccess_ReturnsFalse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            uint folderId = 1;
            var permission = AccessPermission.Create;

            var folderPath = _fixture
                .Build<Folder>()
                .Without(f => f.OuterFolder)
                .CreateMany().ToList();
            var user = _fixture.Build<User>()
                                .With(u => u.IdentityGuid, userId)
                                .Create();

            _unitOfWork.Setup(uow => uow.FolderRepository.GetOuterFoldersAsync(folderId))
                       .ReturnsAsync(folderPath);
            _unitOfWork.Setup(uow => uow.UserRepository.GetUserById(userId))
                       .ReturnsAsync(user);

            // Act
            var result = await _accessService.GetAccessVerification(userId, folderId, permission);

            // Assert
            Assert.False(result);
        }

        #endregion


        #region GetAccessors Tests

        [Fact]
        public async Task GetAccessors_FolderHasNoAccessors_ReturnsEmptyList()
        {
            // Arrange
            uint folderId = 1;
            _unitOfWork.Setup(uow => uow.UserAccessRepository.GetFolderAccessors(folderId))
                       .ReturnsAsync([]);

            // Act
            var result = await _accessService.GetAccessors(folderId);

            // Assert
            Assert.Empty(result);
            _unitOfWork.Verify(uow => uow.UserAccessRepository.GetFolderAccessors(folderId), Times.Once);
        }

        [Fact]
        public async Task GetAccessors_FolderHasAccessors_ReturnsMappedAccessModels()
        {
            // Arrange
            uint folderId = 1;
            var accessors = _fixture.Build<UserAccess>()
                                    .Without(ua => ua.Folder)
                                    .With(ua => ua.User, _fixture.Build<User>().Create())
                                    .CreateMany(5);

            _unitOfWork.Setup(uow => uow.UserAccessRepository.GetFolderAccessors(folderId))
                       .ReturnsAsync(accessors);

            var expected = accessors.Select(ua => _mapper.Map<AccessModel>(ua)).ToList();

            // Act
            var result = await _accessService.GetAccessors(folderId);

            // Assert
            Assert.Equal(expected.Count, result.Count());

            // Custom comparison for AccessModel properties
            var expectedList = expected.OrderBy(e => e.User.Id).ToList();
            var actualList = result.OrderBy(a => a.User.Id).ToList();
            Assert.True(expectedList.SequenceEqual(actualList, new AccessModelComparer()));

            _unitOfWork.Verify(uow => uow.UserAccessRepository.GetFolderAccessors(folderId), Times.Once);
        }

        public class AccessModelComparer : IEqualityComparer<AccessModel>
        {
            public bool Equals(AccessModel x, AccessModel y)
            {
                if (x == null || y == null) return false;
                return x.FolderName == y.FolderName && x.Permissions == y.Permissions;
            }

            public int GetHashCode(AccessModel obj)
            {
                return HashCode.Combine(obj.FolderName, obj.Permissions);
            }
        }

        #endregion

        #region GiveAccess

        [Fact]
        public async Task GiveAccess_UserNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var folderId = 1u;
            var model = _fixture.Create<AccessShortModel>();

            _identityServiceMock.Setup(s => s.GetUserByEmail(model.Email)).ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await _accessService.GiveAccess(folderId, model));
        }

        [Fact]
        public async Task GiveAccess_FolderNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var folderId = 1u;
            var model = _fixture.Create<AccessShortModel>();
            var user = _fixture.Create<User>();

            model.Email = user.Email;

            _identityServiceMock.Setup(s => s.GetUserByEmail(model.Email)).ReturnsAsync(user);
            _unitOfWork.Setup(uow => uow.FolderRepository.GetFolderByIdAsync(folderId)).ReturnsAsync((Folder)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await _accessService.GiveAccess(folderId, model));
        }

        [Fact]
        public async Task GiveAccess_ValidData_AddsAccessAndSavesChanges()
        {
            // Arrange
            var folderId = 1u;
            var model = _fixture.Create<AccessShortModel>();
            var user = _fixture.Create<User>();
            var folder = _fixture.Build<Folder>()
                .Without(f => f.OuterFolder)
                .With(f => f.Id, folderId)
                .Create();

            model.Email = user.Email;

            _identityServiceMock.Setup(s => s.GetUserByEmail(model.Email)).ReturnsAsync(user);
            _unitOfWork.Setup(uow => uow.FolderRepository.GetFolderByIdAsync(folderId)).ReturnsAsync(folder);
            _unitOfWork.Setup(uow => uow.UserAccessRepository.AddUserAccess(It.IsAny<UserAccess>())).Verifiable();

            // Act
            var result = await _accessService.GiveAccess(folderId, model);

            // Assert
            _unitOfWork.Verify(uow => uow.UserAccessRepository.AddUserAccess(It.IsAny<UserAccess>()), Times.Once);
            _unitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Once);
            Assert.Equal(model.Email, result.User.Email);
            Assert.Equal(model.Permission.ToString(), result.Permissions);
        }

        #endregion

        #region UpdateAccess Tests

        [Fact]
        public async Task UpdateAccess_OwnerChangeNotAllowed_ThrowsInvalidOperationException()
        {
            // Arrange
            var folderId = 1u;
            var user = _fixture.Create<User>();

            var model = new AccessShortModel() 
            { 
                Email = user.Email, 
                Permission = AccessPermission.Create | AccessPermission.Delete 
            };

            var currentAccess = _fixture.Build<UserAccess>()
                                        .With(ua => ua.FolderId, folderId)
                                        .Without(ua => ua.Folder)
                                        .With(ua => ua.AccessFlags, AccessPermission.Owner)
                                        .Create();

            _unitOfWork.Setup(uow => uow.UserRepository.GetUserByEmail(model.Email)).ReturnsAsync(user);
            _unitOfWork.Setup(uow => uow.UserAccessRepository.GetUserAccessAsync(folderId, user.IdentityGuid)).ReturnsAsync(currentAccess);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await _accessService.UpdateAccess(folderId, model));
        }

        [Fact]
        public async Task UpdateAccess_ValidData_UpdatesAndSavesChanges()
        {
            // Arrange
            var folderId = 1u;
            var model = _fixture.Create<AccessShortModel>();
            var user = _fixture.Create<User>();

            var currentAccess = _fixture.Build<UserAccess>()
                                        .Without(ua => ua.Folder)
                                        .With(ua => ua.FolderId, folderId)
                                        .Create();

            _unitOfWork.Setup(uow => uow.UserRepository.GetUserByEmail(model.Email)).ReturnsAsync(user);
            _unitOfWork.Setup(uow => uow.UserAccessRepository.GetUserAccessAsync(folderId, user.IdentityGuid)).ReturnsAsync(currentAccess);

            // Act
            var result = await _accessService.UpdateAccess(folderId, model);

            // Assert
            _unitOfWork.Verify(uow => uow.UserAccessRepository.UpdateUserAccess(currentAccess), Times.Once);
            _unitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Once);
            //Assert.Equal(model.Email, result.Email);
        }

        #endregion

        // Fix bugs in tests!
        #region DeleteAccess

        [Fact]
        public async Task DeleteAccess_UserNotFound_DoesNothing()
        {
            // Arrange
            uint folderId = 1;
            var email = "nonexistentuser@example.com";

            _unitOfWork.Setup(uow => uow.UserRepository.GetUserByEmail(email)).ReturnsAsync((User)null);

            // Act
            await _accessService.DeleteAccess(folderId, email);

            // Assert
            _unitOfWork.Verify(uow => uow.UserAccessRepository.DeleteUserAccess(It.IsAny<UserAccess>()), Times.Never);
            _unitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Never);
        }

        // Either change the logic of test or sut
        [Fact]
        public async Task DeleteAccess_DeletionOfOwnerAccess_ThrowsInvalidOperationException()
        {
            uint folderId = 1;
            var user = _fixture.Create<User>();

            _unitOfWork.Setup(uow => uow.UserRepository.GetUserByEmail(user.Email)).ReturnsAsync(user);

            var userAccess = _fixture.Build<UserAccess>()
                .With(ua => ua.FolderId, folderId)
                .Without(ua => ua.Folder)
                .With(ua => ua.AccessFlags, AccessPermission.Owner)
                .Create();

            user.UserAccesses.Add(userAccess);

            _unitOfWork.Setup(uow => uow.UserAccessRepository.GetUserAccessAsync(folderId, user.IdentityGuid))
                .ReturnsAsync(userAccess);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _accessService.DeleteAccess(folderId, user.Email));
        }

        [Fact]
        public async Task DeleteAccess_ValidData_DeletesAccessAndSavesChanges()
        {
            // Arrange
            uint folderId = 1;
            var email = "user@example.com";
            var user = _fixture.Create<User>();
            var userAccess = _fixture.Build<UserAccess>()
                .With(ua => ua.FolderId, folderId)
                .Without(ua => ua.Folder)
                .Create();

            user.UserAccesses.Add(userAccess);

            _unitOfWork.Setup(uow => uow.UserRepository.GetUserByEmail(email)).ReturnsAsync(user);
            _unitOfWork.Setup(uow => uow.UserAccessRepository.GetUserAccessAsync(folderId, user.IdentityGuid)).ReturnsAsync(userAccess);

            // Act
            await _accessService.DeleteAccess(folderId, email);

            // Assert
            _unitOfWork.Verify(uow => uow.UserAccessRepository.DeleteUserAccess(userAccess), Times.Once);
            _unitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Once);
        }


        #endregion

    }
}