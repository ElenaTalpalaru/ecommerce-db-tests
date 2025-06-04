using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace DbTests.Database
{
    public class UserService : IDisposable
    {
        private ECommerceContext _context;
        private bool _disposed = false;

        public UserService()
        {
            // Try to get connection string from environment variable first, fallback to default
            var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                ?? "Host=localhost;Port=32768;Database=ecommerce_db;Username=ecommerce_user;Password=ecommerce_password";

            var options = new DbContextOptionsBuilder<ECommerceContext>()
                .UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention()
                .Options;
            _context = new(options);
        }

        public async Task<List<User>> GetAllUsers()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User> GetUserById(Guid id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                throw new InvalidOperationException($"User with ID {id} not found");
            return user;
        }

        public async Task<User> UpdateUser(User user)
        {
            // Validate email format
            if (!IsValidEmail(user.Email))
                throw new ArgumentException("Invalid email format", nameof(user.Email));

            // Check if user exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
            if (existingUser == null)
                throw new InvalidOperationException($"User with ID {user.Id} not found");

            // Update properties
            existingUser.Email = user.Email;
            existingUser.FirstName = user.FirstName;
            existingUser.LastName = user.LastName;
            existingUser.Phone = user.Phone;
            existingUser.Role = user.Role;
            existingUser.IsActive = user.IsActive;
            existingUser.EmailVerified = user.EmailVerified;
            // DisplayName is likely computed from FirstName + LastName, so don't set it directly
            existingUser.UpdatedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();
            return existingUser;
        }

        public async Task UpdateMultipleUsers(List<User> users)
        {
            foreach (var user in users)
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
                if (existingUser != null)
                {
                    // Update properties
                    existingUser.Email = user.Email;
                    existingUser.FirstName = user.FirstName;
                    existingUser.LastName = user.LastName;
                    existingUser.Phone = user.Phone;
                    existingUser.Role = user.Role;
                    existingUser.IsActive = user.IsActive;
                    existingUser.EmailVerified = user.EmailVerified;
                    // DisplayName is likely computed, so don't set it directly
                    existingUser.UpdatedAt = DateTimeOffset.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<User> CreateUser(User user)
        {
            if (!IsValidEmail(user.Email))
                throw new ArgumentException("Invalid email format", nameof(user.Email));

            user.Id = Guid.NewGuid();
            user.CreatedAt = DateTimeOffset.UtcNow;
            user.UpdatedAt = DateTimeOffset.UtcNow;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task DeleteUser(Guid id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                throw new InvalidOperationException($"User with ID {id} not found");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        // Optional: Method to help with test data cleanup
        public async Task ClearTestData()
        {
            // Only use this in test environment
            var testUsers = await _context.Users
                .Where(u => u.Email.Contains("@test.com") || u.FirstName.StartsWith("Test"))
                .ToListAsync();

            _context.Users.RemoveRange(testUsers);
            await _context.SaveChangesAsync();
        }

        private static bool IsValidEmail(string email)
        {
            return new EmailAddressAttribute().IsValid(email);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context?.Dispose();
                }
                _disposed = true;
            }
        }

        ~UserService()
        {
            Dispose(false);
        }
    }
}