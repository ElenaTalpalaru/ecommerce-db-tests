using AwesomeAssertions;
using DbTests.Database;
using Microsoft.EntityFrameworkCore;
using System;

namespace DbTests
{
    public class Tests
    {
        private UserService _userService;


        [SetUp]
        public void Setup()
        {
            _userService = new();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            // Dispose the database connection after all tests
            _userService?.Dispose();
        }

        [Test]
        public async Task Should_Already_Have_Customers()
        {
            var userList = await _userService.GetAllUsers();
            userList.Count.Should().BeGreaterThan(0);
        }

        [Test]
        public async Task Should_Update_User_Email_Successfully()
        {
            // Arrange
            var users = await _userService.GetAllUsers();
            var userToUpdate = users.FirstOrDefault();
            userToUpdate.Should().NotBeNull("Need at least one user to test updates");

            var originalEmail = userToUpdate.Email;
            var newEmail = $"updated_{DateTime.Now.Ticks}@test.com";

            // Act
            userToUpdate.Email = newEmail;
            await _userService.UpdateUser(userToUpdate);

            // Assert
            var updatedUser = await _userService.GetUserById(userToUpdate.Id);
            updatedUser.Email.Should().Be(newEmail);
            updatedUser.Email.Should().NotBe(originalEmail);
        }

        [Test]
        public async Task Should_Update_User_Profile_Information()
        {
            // Arrange
            var users = await _userService.GetAllUsers();
            var userToUpdate = users.FirstOrDefault();
            userToUpdate.Should().NotBeNull();

            var newFirstName = "UpdatedFirstName";
            var newLastName = "UpdatedLastName";
            var newPhone = "555-0123";

            // Act
            userToUpdate.FirstName = newFirstName;
            userToUpdate.LastName = newLastName;
            userToUpdate.Phone = newPhone;
            await _userService.UpdateUser(userToUpdate);

            // Assert
            var updatedUser = await _userService.GetUserById(userToUpdate.Id);
            updatedUser.FirstName.Should().Be(newFirstName);
            updatedUser.LastName.Should().Be(newLastName);
            updatedUser.Phone.Should().Be(newPhone);
        }

        [Test]
        public async Task Should_Update_User_Role_Successfully()
        {
            // Arrange
            var users = await _userService.GetAllUsers();
            var userToUpdate = users.FirstOrDefault(u => u.Role != UserRole.Admin);
            userToUpdate.Should().NotBeNull("Need a non-admin user to test role updates");

            var originalRole = userToUpdate.Role;

            // Act
            userToUpdate.Role = UserRole.Admin;
            await _userService.UpdateUser(userToUpdate);

            // Assert
            var updatedUser = await _userService.GetUserById(userToUpdate.Id);
            updatedUser.Role.Should().Be(UserRole.Admin);
            updatedUser.Role.Should().NotBe(originalRole);
        }

        [Test]
        public async Task Should_Update_User_Active_Status()
        {
            // Arrange
            var users = await _userService.GetAllUsers();
            var userToUpdate = users.FirstOrDefault();
            userToUpdate.Should().NotBeNull();

            var originalStatus = userToUpdate.IsActive;

            // Act
            userToUpdate.IsActive = !originalStatus;
            await _userService.UpdateUser(userToUpdate);

            // Assert
            var updatedUser = await _userService.GetUserById(userToUpdate.Id);
            updatedUser.IsActive.Should().Be(!originalStatus);
        }

        [Test]
        public async Task Should_Update_User_Email_Verification_Status()
        {
            // Arrange
            var users = await _userService.GetAllUsers();
            var userToUpdate = users.FirstOrDefault(u => !u.EmailVerified);
            userToUpdate.Should().NotBeNull("Need an unverified user to test email verification");

            // Act
            userToUpdate.EmailVerified = true;
            await _userService.UpdateUser(userToUpdate);

            // Assert
            var updatedUser = await _userService.GetUserById(userToUpdate.Id);
            updatedUser.EmailVerified.Should().BeTrue();
        }

        [Test]
        public async Task Should_Update_Multiple_Users_In_Batch()
        {
            // Arrange
            var users = await _userService.GetAllUsers();
            var usersToUpdate = users.Take(3).ToList();
            usersToUpdate.Should().HaveCountGreaterThan(0);

            // Act - Update FirstName which should affect DisplayName if it's computed
            foreach (var user in usersToUpdate)
            {
                user.FirstName = $"BatchUpdated_{user.Id.ToString()[..8]}";
            }

            await _userService.UpdateMultipleUsers(usersToUpdate);

            // Assert
            foreach (var originalUser in usersToUpdate)
            {
                var updatedUser = await _userService.GetUserById(originalUser.Id);
                updatedUser.FirstName.Should().StartWith("BatchUpdated_");
                // If DisplayName is computed from FirstName + LastName, it should reflect the change
                updatedUser.DisplayName.Should().Contain("BatchUpdated_");
            }
        }

        [Test]
        public async Task Should_Handle_Concurrent_Updates_Gracefully()
        {
            // Arrange
            var users = await _userService.GetAllUsers();
            var userToUpdate = users.FirstOrDefault();
            userToUpdate.Should().NotBeNull();

            var originalUpdatedAt = userToUpdate.UpdatedAt;

            // Act - Simulate concurrent updates
            var firstUpdate = Task.Run(async () =>
            {
                var user = await _userService.GetUserById(userToUpdate.Id);
                user.FirstName = "ConcurrentUpdate1";
                await _userService.UpdateUser(user);
            });

            var secondUpdate = Task.Run(async () =>
            {
                await Task.Delay(50); // Small delay to ensure different timing
                var user = await _userService.GetUserById(userToUpdate.Id);
                user.LastName = "ConcurrentUpdate2";
                await _userService.UpdateUser(user);
            });

            await Task.WhenAll(firstUpdate, secondUpdate);

            // Assert
            var finalUser = await _userService.GetUserById(userToUpdate.Id);
            finalUser.UpdatedAt.Should().BeAfter(originalUpdatedAt);
            // One of the updates should have succeeded
            (finalUser.FirstName == "ConcurrentUpdate1" || finalUser.LastName == "ConcurrentUpdate2")
                .Should().BeTrue();
        }

        [Test]
        public async Task Should_Fail_When_Updating_NonExistent_User()
        {
            // Arrange
            var nonExistentUser = new User
            {
                Id = Guid.NewGuid(),
                Email = "nonexistent@test.com",
                FirstName = "NonExistent",
                LastName = "User"
            };

            // Act & Assert
            var action = async () => await _userService.UpdateUser(nonExistentUser);
            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*not found*");
        }

        [Test]
        public async Task Should_Validate_Email_Format_On_Update()
        {
            // Arrange
            var users = await _userService.GetAllUsers();
            var userToUpdate = users.FirstOrDefault();
            userToUpdate.Should().NotBeNull();

            // Act & Assert
            userToUpdate.Email = "invalid-email-format";
            var action = async () => await _userService.UpdateUser(userToUpdate);
            await action.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*email format*");
        }
    }
}
 