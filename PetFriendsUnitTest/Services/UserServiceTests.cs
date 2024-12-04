using DataAccess.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using UserAuthenticationAPI.Controllers;
using UserAuthenticationAPI.DTOs.UserDTOs;
using UserAuthenticationAPI.Repository.OtpRepository;
using UserAuthenticationAPI.Repository.UserRepository;
using UserAuthenticationAPI.Services;
using UserAuthenticationAPI.Utilities;

namespace PetFriendsUnitTest.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IOtpRepository> _mockOtpRepository;
        private readonly UserService _userService;
        private readonly VerifyService _verifyService;
        private readonly Mock<UserAuthenticationAPI.Utilities.Encoder> _mockEncoder;
        public UserServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockOtpRepository = new Mock<IOtpRepository>();
            _userService = new UserService(_mockUserRepository.Object, _mockOtpRepository.Object);
        }

        [Fact]
        public async Task Login_UserNotRegistered_ReturnsNotFound()
        {
            // Arrange
            var loginModel = new UserLoginReqModel
            {
                Email = "test@example.com",
                Password = "password123"
            };

            _mockUserRepository.Setup(repo => repo.GetUserByEmail(loginModel.Email)).ReturnsAsync((User?)null);

            // Act
            var result = await _userService.Login(loginModel);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.Code);
            Assert.Equal("Email is not registered!", result.Message);
        }

        [Fact]
        public async Task Login_UserNotVerified_ReturnsPleaseVerify()
        {
            // Arrange
            var loginModel = new UserLoginReqModel
            {
                Email = "test@example.com",
                Password = "password123"
            };

            var user = new User
            {
                Email = loginModel.Email,
                Status = "INACTIVE"
            };

            _mockUserRepository.Setup(repo => repo.GetUserByEmail(loginModel.Email)).ReturnsAsync(user);

            // Act
            var result = await _userService.Login(loginModel);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.Code);
            Assert.Equal("Please verify your account", result.Message);
        }


        [Fact]
        public async Task Login_ValidCredentials_ReturnsSuccess()
        {
            // Arrange
            var loginModel = new UserLoginReqModel
            {
                Email = "test@example.com",
                Password = "password123" // Mật khẩu người dùng nhập
            };

            // Tạo salt và hashed password thật sự
            var hashedPasswordDto = UserAuthenticationAPI.Utilities.Encoder.CreateHashPassword("password123");
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = loginModel.Email,
                Password = hashedPasswordDto.HashedPassword, // Mật khẩu đã hash
                Salt = hashedPasswordDto.Salt, // Salt được tạo
                Status = "ACTIVE"
            };

            // Mock UserRepository để trả về user hợp lệ
            _mockUserRepository.Setup(repo => repo.GetUserByEmail(loginModel.Email))
                               .ReturnsAsync(user);

            // Mock Update User
            _mockUserRepository.Setup(repo => repo.Update(It.IsAny<User>()))
                               .ReturnsAsync(true);

            // Act
            var result = await _userService.Login(loginModel);

            // Assert
            Assert.True(result.IsSuccess); // Login thành công
            Assert.Equal(200, result.Code); // Mã HTTP 200
            Assert.NotNull(result.Data); // Dữ liệu không null
            Assert.IsType<UserLoginResModel>(result.Data); // Kết quả là kiểu UserLoginResModel

            _mockUserRepository.Verify(repo => repo.Update(It.Is<User>(u => u.LastLoggedIn.HasValue)), Times.Once);
        }


        [Fact]
        public async Task Login_InvalidPassword_ReturnsBadRequest()
        {
            // Arrange
            var loginModel = new UserLoginReqModel
            {
                Email = "test@example.com",
                Password = "invalidpassword"
            };

            var hashedPasswordDto = UserAuthenticationAPI.Utilities.Encoder.CreateHashPassword("password123");
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = loginModel.Email,
                Password = hashedPasswordDto.HashedPassword, // Mật khẩu đã hash
                Salt = hashedPasswordDto.Salt, // Salt được tạo
                Status = "ACTIVE"
            };

            _mockUserRepository.Setup(repo => repo.GetUserByEmail(loginModel.Email))
                              .ReturnsAsync(user);

            // Mock Update User
            _mockUserRepository.Setup(repo => repo.Update(It.IsAny<User>()))
                               .ReturnsAsync(true);

            //Act
            var result = await _userService.Login(loginModel);

            //Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.Code);
            Assert.Equal("Password is invalid", result.Message);
        }

        [Fact]
        public async Task Register_EmailAlreadyExists_ReturnsBadRequest()
        {
            // Arrange
            var registerModel = new UserReqModel
            {
                Email = "test@example.com",
                Password = "password123",
                PhoneNumber = "1234567890"
            };

            var existingUser = new User { Email = "test@example.com" };

            _mockUserRepository.Setup(repo => repo.GetUserByEmail(registerModel.Email))
                               .ReturnsAsync(existingUser);

            // Act
            var result = await _userService.CreateAccount(registerModel);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.Code);
            Assert.Equal("Email is already registered!", result.Message);
        }

        [Fact]
        public async Task Register_PhoneNumberAlreadyExists_ReturnsBadRequest()
        {
            // Arrange
            var registerModel = new UserReqModel
            {
                Email = "test@example.com",
                Password = "password123",
                PhoneNumber = "1234567890"
            };

            var existingUser = new User { PhoneNumber = "1234567890" };

            _mockUserRepository.Setup(repo => repo.GetUserByPhoneNumber(registerModel.PhoneNumber))
                               .ReturnsAsync(existingUser);

            // Act
            var result = await _userService.CreateAccount(registerModel);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.Code);
            Assert.Equal("Phone number is already registered!", result.Message);
        }


        [Fact]
        public async Task Register_ValidUser_ReturnsSuccess()
        {
            // Arrange
            var registerModel = new UserReqModel
            {
                Email = "test@example.com",
                Password = "password123",
                PhoneNumber = "1234567890"
            };

            _mockUserRepository.Setup(repo => repo.GetUserByEmail(registerModel.Email))
                               .ReturnsAsync((User?)null);

            _mockUserRepository.Setup(repo => repo.GetUserByPhoneNumber(registerModel.PhoneNumber))
                               .ReturnsAsync((User?)null);

            _mockUserRepository.Setup(repo => repo.Insert(It.IsAny<User>()))
                    .ReturnsAsync(Guid.NewGuid());

            // Act
            var result = await _userService.CreateAccount(registerModel);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.Code);
            Assert.Equal("Verification email sent successfully!", result.Message);
        }

        [Fact]
        public async Task GetUserProfile_MissingUserId_ReturnsBadRequest()
        {
            // Arrange
            var controller = new UserController(_userService, _verifyService);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            // Act
            var result = await controller.GetUserProfile();

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            var badRequest = result as BadRequestObjectResult;
            Assert.Equal("Unable to retrieve user ID", badRequest?.Value);
        }

        [Fact]
        public async Task GetUserProfile_InvalidUserIdFormat_ReturnsBadRequest()
        {
            // Arrange
            var controller = new UserController(_userService, _verifyService);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
          new Claim("userid", "invalid-guid-format")
         }));

            // Act
            var result = await controller.GetUserProfile();

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            var badRequest = result as BadRequestObjectResult;
            Assert.Equal("Invalid user ID format", badRequest?.Value);
        }

        [Fact]
        public async Task GetUserProfile_ValidUserId_ReturnsSuccess()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Email = "test@example.com",
                FullName = "Test User",
                PhoneNumber = "1234567890"
            };

            _mockUserRepository.Setup(repo => repo.Get(userId)).ReturnsAsync(user);

            // Act
            var result = await _userService.GetUserProfile(userId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.Code);
            Assert.NotNull(result.Data);
        }

        [Fact]
        public async Task GetUserProfile_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _mockUserRepository.Setup(repo => repo.Get(userId)).ReturnsAsync((User?)null);

            var result = await _userService.GetUserProfile(userId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.Code);
            Assert.Equal("Not found", result.Message);
        }
    }
}
