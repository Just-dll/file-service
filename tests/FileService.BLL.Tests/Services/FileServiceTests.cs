using AutoFixture;
using AutoMapper;
using FileService.BLL.Interfaces;
using FileService.BLL.MapperProfiles;
using FileService.BLL.Models;
using FileService.BLL.Models.Short;
using FileService.DAL.Data;
using FileService.DAL.Entities;
using FileService.DAL.Exceptions;
using FileService.DAL.Interfaces;
using FileService.Tests;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Linq.Expressions;
using System.Text;

namespace FileService.BLL.Tests.Services
{
    public class FileServiceTests
    {
        private readonly Mock<IFormFile> _fileMock;
        private readonly IMapper _mapper;
        private readonly Mock<IStorageProvider> _storageMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly BLL.Services.FileService _sut;
        private readonly Mock<IDateTimeService> _timeServiceMock;
        private readonly Fixture fixture;

        public FileServiceTests()
        {
            _fileMock = new Mock<IFormFile>();
            _mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperProfile>()));
            _storageMock = new Mock<IStorageProvider>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _timeServiceMock = new Mock<IDateTimeService>();

            fixture = new Fixture();
            _sut = new BLL.Services.FileService(_unitOfWorkMock.Object, _storageMock.Object, _mapper, _timeServiceMock.Object);
        }

        private void SetupFileMock(string fileName, long fileSize = 1024)
        {
            _fileMock.Setup(f => f.Name).Returns(fileName);
            _fileMock.Setup(f => f.FileName).Returns(fileName);
            _fileMock.Setup(f => f.Length).Returns(fileSize);
        }

        #region GetFileAsync Tests

        [Fact]
        public async Task GetFileAsync_FileDoesntExists_ReturnsNull()
        {
            // Arrange
            var folderId = 112u;

            var fileId = 11u;

            _unitOfWorkMock.Setup(uow => uow.FileRepository.GetFileByIdAsync(folderId, fileId))
                .ReturnsAsync((DAL.Entities.File) null);

            // Act
            var result = await _sut.GetFileAsync(folderId, fileId);

            // Assert

            _unitOfWorkMock.Verify(uow => uow.FileRepository.GetFileByIdAsync(folderId, fileId), Times.Once());
            Assert.Null(result);
        }

        [Fact]
        public async Task GetFileAsync_ValidId_ValidFileModel()
        {
            var currentTime = DateTime.UtcNow;

            _timeServiceMock.Setup(dts => dts.DateTimeNow).Returns(currentTime);
            
            var folder = new Folder { Id = 11u, Name = "fileTest" };

            var existingFile = new DAL.Entities.File
            {
                Id = 1121u,
                CreationDate = currentTime,
                FolderId = folder.Id,
                InternalFilePath = "somepath/path/path",
                Name = "someName",
                Size = 1024,
            };

            _unitOfWorkMock.Setup(uow => uow.FileRepository.GetFileByIdAsync(folder.Id, existingFile.Id))
                .ReturnsAsync(existingFile);

            var expectedFile = new FileModel()
            {
                Name = existingFile.Name,
                FilePath = null/*"path/path/fileTest/"*/,
                Size = "1 KB",
                CreationDate = currentTime
            };

            var result = await _sut.GetFileAsync(folder.Id, existingFile.Id);

            Assert.NotNull(result);

            Assert.Equal(expectedFile.Name, result.Name);
            Assert.Equal(expectedFile.Size, result.Size);
            Assert.Equal(expectedFile.CreationDate, result.CreationDate);
            Assert.Equal(expectedFile.FilePath, result.FilePath);

        }

        #endregion

        #region UploadFileAsync Tests
        [Fact]
        public async Task UploadFileAsync_AlreadyPresentFileInFolder_ThrowsAlreadyExistsException()
        {
            // Arrange
            var folder = new Folder { Id = 11, Name = "fileTest" };

            var existingFile = new DAL.Entities.File
            {
                CreationDate = DateTime.Now,
                FolderId = folder.Id,
                InternalFilePath = "somepath/path/path",
                Name = "someName"
            };

            folder.Files.Add(existingFile);
            _unitOfWorkMock.Setup(uow => uow.FolderRepository.GetFolderByIdAsync(folder.Id)).ReturnsAsync(folder);
            _unitOfWorkMock.Setup(uow => uow.FileRepository.AddFile(existingFile)).Verifiable();

            SetupFileMock(existingFile.Name);

            // Act & Assert
            await Assert.ThrowsAsync<AlreadyExistsException>(() => _sut.UploadFileAsync(_fileMock.Object, folder.Id));
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Never);
            _unitOfWorkMock.Verify(uow => uow.FileRepository.AddFile(existingFile), Times.Never);
        }

        [Fact]
        public async Task UploadFileAsync_NonexistentFolderIdProvided_ThrowsDirectoryNotFoundException()
        {
            // Arrange
            var invalidFolderId = 1u;
            _unitOfWorkMock.Setup(uow => uow.FolderRepository.GetFolderByIdAsync(invalidFolderId))
                .ReturnsAsync((Folder)null);

            SetupFileMock("someFile.txt");

            // Act & Assert
            await Assert.ThrowsAsync<DirectoryNotFoundException>(() => _sut.UploadFileAsync(_fileMock.Object, invalidFolderId));
        }

        [Fact]
        public async Task UploadFileAsync_ValidFile_ReturnsCorrectFileEntity()
        {
            // Arrange
            string fileName = "someFile.txt";
            SetupFileMock(fileName);

            string uploadRelativeFilePath = "/someFolder/nestedFolder/";
            string internalFilePath = Path.Combine(Path.GetTempPath(), uploadRelativeFilePath);
            List<Folder> path = [];
            int counter = 1;
            foreach (var item in uploadRelativeFilePath.Split('/'))
            {
                path.Add(new() { Name = item });
            }

            var folderId = 11u;

            _unitOfWorkMock.Setup(uow => uow.FolderRepository.GetFolderByIdAsync(folderId)).ReturnsAsync(path[^1]);

            _unitOfWorkMock.Setup(uow => uow.FolderRepository.GetOuterFoldersAsync(folderId))
                .ReturnsAsync(path);

            _unitOfWorkMock.Setup(uow => uow.FileRepository.AddFile(It.IsAny<DAL.Entities.File>())).Verifiable();

            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync()).Verifiable();

            _storageMock.Setup(s => s.UploadFileAsync(uploadRelativeFilePath, _fileMock.Object, CancellationToken.None))
                        .ReturnsAsync(internalFilePath);

            var currDateTime = DateTime.Now;
            _timeServiceMock.Setup(dts => dts.DateTimeNow).Returns(currDateTime);

            var expected = new FileShortModel
            {
                Name = fileName,
                Size = "1 KB",
                CreationDate = DateOnly.FromDateTime(currDateTime)
            };

            // Act
            var result = await _sut.UploadFileAsync(_fileMock.Object, folderId);

            // Assert
            Assert.Equal(expected.Name, result.Name);
            Assert.Equal(expected.Size, result.Size);
            Assert.Equal(expected.CreationDate, result.CreationDate);
        }

        #endregion

        #region DeleteFileAsync Tests

        [Fact]
        public async Task DeleteFileAsync_FileExists_DeletesFile()
        {
            // Arrange
            var folderId = 12u;
            var file = new DAL.Entities.File()
            {
                Id = 11u,
                CreationDate = DateTime.Now,
                FolderId = folderId,
                InternalFilePath = "some/pa/th",
                Name = "NameName"
            };

            _unitOfWorkMock.Setup(uow => uow.FileRepository.GetFileByIdAsync(folderId, file.Id))
                .ReturnsAsync(file);
            _unitOfWorkMock.Setup(uow => uow.FileRepository.DeleteFile(file)).Verifiable();
            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync()).Verifiable();


            // Act
            await _sut.DeleteFileAsync(folderId, file.Id);

            _unitOfWorkMock.Verify(uow => uow.FileRepository.GetFileByIdAsync(folderId, file.Id), Times.Once());
            _unitOfWorkMock.Verify(uow => uow.FileRepository.DeleteFile(file), Times.Once());
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once());
        }

        [Fact]
        public async Task DeleteFileAsync_FileDoesntExists_DoesNothing()
        {
            var folderId = 1121u;
            var fileId = 23122u;

            _unitOfWorkMock.Setup(uow => uow.FileRepository.GetFileByIdAsync(folderId, fileId))
                .ReturnsAsync((DAL.Entities.File)null);

            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync()).Verifiable();
            
            _unitOfWorkMock.Setup(uow => uow.FileRepository.DeleteFile(It.IsAny<DAL.Entities.File>())).Verifiable();

            await _sut.DeleteFileAsync(folderId, fileId);

            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Never());
            _unitOfWorkMock.Verify(uow => uow.FileRepository.DeleteFile(It.IsAny<DAL.Entities.File>()), Times.Never());
        }
        #endregion

        #region DownloadFileAsync Tests

        [Fact]
        public async Task DownloadFileAsync_FileExists_ValidFileDownloadModel()
        {
            List<Folder> outerFolders = [
                new() { Name = "Allalall", Id = 11u }, 
                new() { Name = "apsa", Id = 12u, FolderId = 11u}, 
                new() { Name = "Alsda", Id = 13u, FolderId = 12u }
            ];

            var filePath = string.Join('/', outerFolders.Select(f => f.Name));
            var content = "Content of file";
            byte[] bytes = Encoding.UTF8.GetBytes(content);

            var folderId = outerFolders[^1].Id;

            var file = new DAL.Entities.File()
            {
                CreationDate = DateTime.Now,
                Name = "filename",
                FolderId = folderId,
                InternalFilePath = filePath
            };

            _storageMock.Setup(isp => isp.ReadFileAsync(filePath, file.Name, CancellationToken.None))
                .ReturnsAsync(bytes);

            _unitOfWorkMock.Setup(uow => uow.FileRepository.GetFileByIdAsync(folderId, file.Id))
                .ReturnsAsync(file);

            _unitOfWorkMock.Setup(uow => uow.FolderRepository.GetOuterFoldersAsync(folderId))
                .ReturnsAsync(outerFolders.AsEnumerable());

            var expected = new FileDownloadModel() { Data = bytes, Name = file.Name };

            // Act
            var result = await _sut.DownloadFile(folderId, file.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected.Name, result.Name);
            Assert.Equal(expected.Data, result.Data);
        }

        [Fact]
        public async Task DownalodFileAsync_FileDoesntExists_ReturnsNull()
        {
            var folderId = 11u;
            var fileId = 12334u;

            _unitOfWorkMock.Setup(uow => uow.FileRepository.GetFileByIdAsync(folderId, fileId))
                .ReturnsAsync((DAL.Entities.File)null);

            var result = await _sut.DownloadFile(folderId, fileId);

            Assert.Null(result);
        }

        #endregion

    }
}