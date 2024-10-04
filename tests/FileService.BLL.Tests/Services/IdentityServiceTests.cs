using AutoFixture;
using FileService.BLL.Grpc;
using FileService.BLL.Services;
using FileService.DAL.Entities;
using FileService.DAL.Interfaces;
using Grpc.Core;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FileService.BLL.Tests.Services
{
    public class IdentityServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<Identity.IdentityClient> _identityClientMock;
        private readonly IdentityService _sut;
        private readonly Fixture _fixture;
        public IdentityServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _identityClientMock = new Mock<Identity.IdentityClient>();
            _sut = new IdentityService(_identityClientMock.Object, _unitOfWorkMock.Object);
            _fixture = new Fixture();
        }

        #region GetUserById

        [Fact]
        public async Task GetUserById_UserExists_ReturnsUser()
        {
            var userId = Guid.NewGuid();
            var user = _fixture.Build<User>().With(u => u.IdentityGuid, userId).Create();

            _unitOfWorkMock.Setup(uow => uow.UserRepository.GetUserById(userId)).ReturnsAsync(user);

            var actual = await _sut.GetUserById(userId);

            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Never());
            _identityClientMock.Verify(uow => uow.GetUserAsync(It.IsAny<UserRequest>(), It.IsAny<CallOptions>()), Times.Never());

            Assert.NotNull(actual);
            Assert.Equal(user, actual);
        }


        [Fact]
        public async Task GetUserById_UserExistsOnExternalServer_ReturnsRetrievedUser()
        {
            var userId = Guid.NewGuid();
            var userResponse = _fixture.Build<UserInstanceResponse>()
                .With(u => u.Guid, userId.ToString())
                .Create();

            _unitOfWorkMock.Setup(uow => uow.UserRepository.GetUserById(userId))
                .ReturnsAsync((User)null);

            var call = new AsyncUnaryCall<UserInstanceResponse>(
                Task.FromResult(userResponse),
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => [],
                () => Console.WriteLine("Exiting"));
            _identityClientMock.Setup(ic => ic.GetUserAsync(It.IsAny<UserRequest>(), null, null, CancellationToken.None))
                .Returns(call);

            var actual = await _sut.GetUserById(userId);

            _unitOfWorkMock.Verify(uow => uow.UserRepository.AddUser(It.IsAny<User>()), Times.Once());
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once());
            Assert.NotNull(actual);
            Assert.Equal(userId, actual.IdentityGuid);
        }

        [Fact]
        public async Task GetUserById_UserDoesNotExist_ReturnsNull()
        {
            var userId = Guid.NewGuid();

            var call = new AsyncUnaryCall<UserInstanceResponse>(
                Task.FromResult(new UserInstanceResponse() { Guid = default }),
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => [],
                () => Console.WriteLine("Exiting"));

            _unitOfWorkMock.Setup(uow => uow.UserRepository.GetUserById(userId))
                .ReturnsAsync((User)null);

            _identityClientMock.Setup(ic => ic.GetUserAsync(It.IsAny<UserRequest>(), It.IsAny<CallOptions>()))
                .Returns(call);

            var actual = await _sut.GetUserById(userId);

            _unitOfWorkMock.Verify(uow => uow.UserRepository.AddUser(It.IsAny<User>()), Times.Never());
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Never());
            Assert.Null(actual);
        }
        #endregion

        #region GetUserByEmail
        [Fact]
        public async Task GetUserByEmail_UserExists_ReturnsUser()
        {
            var user = _fixture.Create<User>();

            _unitOfWorkMock.Setup(uow => uow.UserRepository.GetUserByEmail(user.Email)).ReturnsAsync(user);

            var actual = await _sut.GetUserByEmail(user.Email);

            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Never());
            _identityClientMock.Verify(uow => uow.GetUserAsync(It.IsAny<UserRequest>(), It.IsAny<CallOptions>()), Times.Never());

            Assert.NotNull(actual);
            Assert.Equal(user, actual);
        }

        [Fact]
        public async Task GetUserByEmail_UserExistsOnExternalServer_ReturnsRetrievedUser()
        {
            var email = "test@example.com";
            var userResponse = _fixture.Build<UserInstanceResponse>()
                .With(u => u.Guid, Guid.NewGuid().ToString())
                .Create();

            _unitOfWorkMock.Setup(uow => uow.UserRepository.GetUserByEmail(email))
                .ReturnsAsync((User)null);

            var call = new AsyncUnaryCall<UserInstanceResponse>(
                Task.FromResult(userResponse),
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => [],
                () => Console.WriteLine("Exiting"));

            _identityClientMock.Setup(ic => ic.GetUserByEmailAsync(It.IsAny<UserEmailRequest>(), It.IsAny<CallOptions>()))
                .Returns(call);

            var actual = await _sut.GetUserByEmail(email);

            _unitOfWorkMock.Verify(uow => uow.UserRepository.AddUser(It.IsAny<User>()), Times.Once());
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once());
            Assert.NotNull(actual);
            Assert.Equal(email, actual.Email);
        }

        [Fact]
        public async Task GetUserByEmail_UserDoesNotExist_ReturnsNull()
        {
            var email = "nonexistent@example.com";

            _unitOfWorkMock.Setup(uow => uow.UserRepository.GetUserByEmail(email))
                .ReturnsAsync((User)null);

            var call = new AsyncUnaryCall<UserInstanceResponse>(
                Task.FromResult(new UserInstanceResponse() { Guid = default }),
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => [],
                () => Console.WriteLine("Exiting"));

            _identityClientMock.Setup(ic => ic.GetUserByEmailAsync(It.IsAny<UserEmailRequest>(), It.IsAny<CallOptions>()))
                .Returns(call);

            var actual = await _sut.GetUserByEmail(email);

            _unitOfWorkMock.Verify(uow => uow.UserRepository.AddUser(It.IsAny<User>()), Times.Never());
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Never());
            Assert.Null(actual);
        }
        #endregion

    }
}
