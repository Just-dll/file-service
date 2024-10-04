using AutoMapper;
using FileService.BLL.Interfaces;
using FileService.BLL.Services;
using FileService.BLL.MapperProfiles;
using FileService.BLL.Models;
using FileService.BLL.Models.Short;
using FileService.DAL.Data;
using FileService.DAL.Entities;
using FileService.DAL.Interfaces;
using FileService.Tests;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using System.IO.Compression;

namespace FileService.BLL.Tests.Services
{
    public class FolderServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly IMapper _mapper;
        private readonly Mock<IStorageProvider> _storageProviderMock;
        private readonly FolderService _sut;

        public FolderServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperProfile>()));
            _storageProviderMock = new Mock<IStorageProvider>();
            _sut = new FolderService(_unitOfWorkMock.Object, _mapper, _storageProviderMock.Object);
        }

        [Fact]
        public async Task GetFolderAsync_NotValidId_ReturnsNull()
        {
            var folderId = 120u;
            _unitOfWorkMock.Setup(uow => uow.FolderRepository.GetFolderByIdAsync(folderId))
                .Returns(Task.FromResult((Folder)null));

            var actual = await _sut.GetFolderAsync(folderId);

            Assert.Null(actual);
        }

        [Fact]
        public async Task GetFolderAsync_FolderExists_ReturnsFolderModel()
        {
            // Arrange
            var folderId = 1U;
            var folderEntity = new Folder { Id = folderId, Name = "Test Folder" };
            var folderModel = new FolderModel { Id = (int)folderId, Name = "Test Folder" };

            var fixture = new Fixture();
            _unitOfWorkMock.Setup(u => u.FolderRepository.GetFolderByIdAsync(folderId))
                           .ReturnsAsync(folderEntity);
            
            // Act
            var result = await _sut.GetFolderAsync(folderId);

            Assert.NotNull(result);
            Assert.Equal(folderModel.Id, result.Id);
            Assert.Equal(folderModel.Name, result.Name);
        }

        [Fact]
        public async Task CreateFolderAsync_ValidFolder_CreatesAndReturnsFolderModel()
        {
            var folderModel = new FolderModel { Name = "folderName" };
            var parentId = 12u;
            var folder = new Folder { Name = folderModel.Name, FolderId = parentId };

            // Updated mock setup to match parameters exactly
            _unitOfWorkMock
                .Setup(uow => uow.FolderRepository.CreateFolder(It.Is<Folder>(f => f.Name == folder.Name && f.FolderId == folder.FolderId)))
                .Verifiable();

            var result = await _sut.CreateFolderAsync(folderModel.Name, parentId);

            Assert.NotNull(result);
            Assert.Equal(folderModel.Name, result.Name);

            _unitOfWorkMock.Verify(uow => uow.FolderRepository.CreateFolder(It.Is<Folder>(f => f.Name == folder.Name && f.FolderId == folder.FolderId)), Times.Once());
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once());
        }

        [Fact]
        public async Task DeleteFolderAsync_FolderExists_DeletesFolderFromDbAndStorage()
        {
            var folderId = 1U;
            var folderEntity = new Folder { Id = folderId, Name = "Test Folder" };
            var folderPath = new List<Folder> { new Folder { Name = "Root" }, folderEntity };

            _unitOfWorkMock.Setup(u => u.FolderRepository.GetFolderByIdAsync(folderId))
                           .ReturnsAsync(folderEntity);
            _unitOfWorkMock.Setup(u => u.FolderRepository.GetOuterFoldersAsync(folderId))
                           .ReturnsAsync(folderPath);

            _storageProviderMock.Setup(sp => sp.DeleteItemAsync(It.IsAny<string>(), null, CancellationToken.None))
                                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                           .ReturnsAsync(1);

            // Act
            await _sut.DeleteFolderAsync(folderId);

            // Assert
            _unitOfWorkMock.Verify(u => u.FolderRepository.DeleteFolder(folderEntity), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
            _storageProviderMock.Verify(sp => sp.DeleteItemAsync(It.IsAny<string>(), null, CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task DeleteFolderAsync_FolderDoesNotExist_DoesNotDeleteAnything()
        {
            // Arrange
            var folderId = 1U;

            _unitOfWorkMock.Setup(u => u.FolderRepository.GetFolderByIdAsync(folderId))
                           .ReturnsAsync((Folder)null);

            // Act
            await _sut.DeleteFolderAsync(folderId);

            // Assert
            _unitOfWorkMock.Verify(u => u.FolderRepository.DeleteFolder(It.IsAny<Folder>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateFolderAsync_FolderExists_UpdatesFolder()
        {
            // Arrange
            var folderId = 1U;
            var existingFolder = new Folder { Id = folderId, Name = "Old Name" };
            var updatedFolderModel = new FolderModel { Name = "New Name" };
            var folderPath = new List<Folder> { new Folder { Name = "Root" }, existingFolder };

            _unitOfWorkMock.Setup(u => u.FolderRepository.GetFolderByIdAsync(folderId))
                           .ReturnsAsync(existingFolder);

            _unitOfWorkMock.Setup(u => u.FolderRepository.GetOuterFoldersAsync(folderId))
                           .ReturnsAsync(folderPath);

            _storageProviderMock.Setup(sp => sp.UpdateFolderAsync(It.IsAny<string>(), It.IsAny<string>(), CancellationToken.None))
                                .Returns(Task.CompletedTask);


            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                           .ReturnsAsync(1);

            // Act
            await _sut.UpdateFolderAsync(folderId, updatedFolderModel);

            // Assert
            _unitOfWorkMock.Verify(u => u.FolderRepository.UpdateFolder(existingFolder), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateFolderAsync_FolderDoesNotExist_ThrowsKeyNotFoundException()
        {
            // Arrange
            var folderId = 1U;
            var updatedFolderModel = new FolderModel { Name = "New Name" };

            _unitOfWorkMock.Setup(u => u.FolderRepository.GetFolderByIdAsync(folderId))
                           .ReturnsAsync((Folder)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.UpdateFolderAsync(folderId, updatedFolderModel));
        }

        [Fact]
        public async Task GetFolderArchiveAsync_FolderExists_ReturnsArchiveWithCorrectFilesAndFolders()
        {
            // Arrange
            var folderId = 1U;
            var folderEntity = new Folder { Id = folderId, Name = "TestFolder" };
            
            var folderPath = new List<Folder>
            {
                new Folder { Name = "Root" },
                folderEntity
            };

            // Create a zip archive in-memory to simulate folder contents
            byte[] archiveData;
            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    // Add folder structure and files to the archive
                    var folderEntry = archive.CreateEntry($"{folderEntity.Name}/");
                    var fileEntry = archive.CreateEntry($"{folderEntity.Name}/file1.txt");

                    // Add content to file1.txt
                    using var fileStream = fileEntry.Open();
                    using var writer = new StreamWriter(fileStream);
                    await writer.WriteAsync("This is file1 content");
                }
                archiveData = memoryStream.ToArray(); // Get byte array from memory stream
            }

            _unitOfWorkMock.Setup(u => u.FolderRepository.GetFolderByIdAsync(folderId))
                           .ReturnsAsync(folderEntity);

            _unitOfWorkMock.Setup(u => u.FolderRepository.GetOuterFoldersAsync(folderId))
                           .ReturnsAsync(folderPath);

            _storageProviderMock.Setup(sp => sp.ReadFolderAsync(It.IsAny<string>(), CancellationToken.None))
                                .ReturnsAsync(archiveData);

            // Act
            var result = await _sut.GetFolderArchiveAsync(folderId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(folderEntity.Name, result!.FolderName);

            // Validate that the archive contains correct folder and file names
            using (var archiveStream = new MemoryStream(result.ArchiveData))
            using (var archive = new ZipArchive(archiveStream, ZipArchiveMode.Read))
            {
                var entries = archive.Entries;

                // Check if the archive contains the folder and the file
                Assert.Contains(entries, e => e.FullName == $"{folderEntity.Name}/");
                Assert.Contains(entries, e => e.FullName == $"{folderEntity.Name}/file1.txt");

                // Optionally, read the file content to verify its correctness
                var fileEntry = entries.Single(e => e.FullName == $"{folderEntity.Name}/file1.txt");
                using (var fileStream = fileEntry.Open())
                using (var reader = new StreamReader(fileStream))
                {
                    var fileContent = await reader.ReadToEndAsync();
                    Assert.Equal("This is file1 content", fileContent);
                }
            }
        }

        [Fact]
        public async Task GetFolderArchiveAsync_FolderDoesNotExist_ReturnsNull()
        {
            // Arrange
            var folderId = 1U;

            _unitOfWorkMock.Setup(u => u.FolderRepository.GetFolderByIdAsync(folderId))
                           .ReturnsAsync((Folder)null);

            // Act
            var result = await _sut.GetFolderArchiveAsync(folderId);

            // Assert
            Assert.Null(result);
        }
    }
}
