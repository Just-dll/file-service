using FileService.DAL.Repositories;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.DAL.Tests.Repositories
{
    public class StorageProviderTests
    {
        private readonly StorageProvider sut;
        private readonly string workingDirectoryPath;
        public StorageProviderTests()
        {
            workingDirectoryPath = Path.GetTempPath();
            sut = new StorageProvider(workingDirectoryPath);
        }

        [Fact]
        public async Task UploadFile_ValidFile_LoadedFile()
        {
            // Arrange
            var formFileMock = new Mock<IFormFile>();
            var fileName = "nameFile.txt";
            var fileContent = "Hi I'm Josh";
            byte[] fileBytes = Encoding.UTF8.GetBytes(fileContent);

            var ms = new MemoryStream(fileBytes);
            // Set up memory stream to simulate file content
            formFileMock.Setup(f => f.FileName).Returns(fileName);
            formFileMock.Setup(f => f.Length).Returns(fileBytes.Length);

            formFileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns((Stream targetStream, CancellationToken cancellationToken) => ms.CopyToAsync(targetStream, cancellationToken));

            var relativePath = "current/";
            var folderPath = Path.Combine(workingDirectoryPath, relativePath);
            var filePath = Path.Combine(folderPath, fileName);

            // Act
            string result = await sut.UploadFileAsync(relativePath, formFileMock.Object);

            // Assert
            Assert.True(File.Exists(filePath), "The file should be created.");

            var loadedFileContent = await File.ReadAllTextAsync(filePath);
            Assert.Equal(fileContent, loadedFileContent);

            // Clean up
            File.Delete(filePath);
            Directory.Delete(folderPath);
        }

        [Fact]
        public async Task UploadFile_InvalidFileName_ThrowsException()
        {
            var formFileMock = new Mock<IFormFile>();
            var fileName = "</?>";
            formFileMock.Setup(f => f.Name).Returns(fileName);
            formFileMock.Setup(f => f.Name).Returns("ALalalalslal");
            formFileMock.Setup(f => f.FileName).Returns(fileName);

            var relativePath = "current/";
            var folderPath = Path.Combine(workingDirectoryPath, relativePath);
            var filePath = Path.Combine(folderPath, fileName);

            // Act && Assert
            await Assert.ThrowsAsync<IOException>(async () => await sut.UploadFileAsync(relativePath, formFileMock.Object, CancellationToken.None));
        }

        [Fact]
        public async Task ReadFileAsync_FileExists_ValidFile()
        {
            // Arrange
            var fileName = "nameFile.txt";
            var fileContent = "Hi I'm Josh";
            byte[] fileBytes = Encoding.UTF8.GetBytes(fileContent);

            var relativePath = "curr/";
            var folderPath = Path.Combine(workingDirectoryPath, relativePath);
            var filePath = Path.Combine(folderPath, fileName);

            Directory.CreateDirectory(folderPath);

            using (File.Create(filePath)) { }

            // Open the file for writing in a new stream
            using (var fs = new FileStream(filePath, FileMode.Create))
            {
                fs.Write(fileBytes, 0, fileBytes.Length);
            }

            // Act
            byte[]? result = await sut.ReadFileAsync(relativePath, fileName, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(fileBytes.Length, result.Length);
            Assert.Equal(fileBytes, result);

            // Clean up
            File.Delete(filePath);
            Directory.Delete(folderPath);
        }

        [Fact]
        public async Task ReadFileAsync_FileDoesntExists_ThrowsException()
        {
            // Arrange
            var fileName = "nameFile.txt";

            var relativePath = "curr/";
            var folderPath = Path.Combine(workingDirectoryPath, relativePath);
            var filePath = Path.Combine(folderPath, fileName);

            // Act & Assert
            await Assert.ThrowsAsync<DirectoryNotFoundException>(async () => await sut.ReadFileAsync(relativePath, fileName, CancellationToken.None));
        }

        [Fact]
        public async Task DeleteItemAsync_DeletingExistingFile_DeletesFile()
        {
            var fileName = "nameFile.txt";

            var relativePath = "curr/";
            var folderPath = Path.Combine(workingDirectoryPath, relativePath);
            var filePath = Path.Combine(folderPath, fileName);

            Directory.CreateDirectory(folderPath);
            using (File.Create(filePath)) { }

            // Act
            await sut.DeleteItemAsync(relativePath, fileName);

            // Assert
            Assert.True(!File.Exists(filePath));

            Directory.Delete(folderPath);
        }

        [Fact]  
        public async Task DeleteItem_DeletingExistingFolder_DeletesFolder()
        {
            // Arrange
            var relativePath = "curr/";
            var folderPath = Path.Combine(workingDirectoryPath, relativePath);

            Directory.CreateDirectory(folderPath);

            // Act
            await sut.DeleteItemAsync(relativePath);

            // Assert
            Assert.True(!Directory.Exists(folderPath));
        }

        [Fact]
        public async Task UpdateFolderAsync_DirectoryExists_MovesFolderToDestination()
        {
            var relativePath = "curr/";

            var relativeDestination = "dest/";

            var folderPath = Path.Combine(workingDirectoryPath, relativePath);

            var relativeFolderPath = Path.Combine(workingDirectoryPath, relativeDestination);
            Directory.CreateDirectory(folderPath);

            await sut.UpdateFolderAsync(relativePath, relativeDestination);

            Assert.True(!Directory.Exists(folderPath));

            Assert.True(Directory.Exists(relativeFolderPath));

            Directory.Delete(relativeFolderPath);
        }
    }
}
