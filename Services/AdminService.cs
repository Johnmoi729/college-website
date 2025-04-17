using CollegeWebsite.Models;
using CollegeWebsite.Utilities;
using MongoDB.Driver;

namespace CollegeWebsite.Services
{
    public class AdminService : MongoDBService<Admin>, IMongoDBService<Admin>
    {
        public AdminService(DatabaseSettings settings)
            : base(settings, settings.AdminsCollectionName)
        {
        }

        public async Task<Admin?> GetByUsernameAsync(string username)
        {
            return await FindAsync(a => a.Username == username).ContinueWith(t => t.Result.FirstOrDefault());
        }

        // Update AdminService.cs to use passwordhasher ultilities:
        public async Task<bool> AuthenticateAsync(string username, string password)
        {
            try
            {
                Console.WriteLine($"Looking up admin with username: {username}");
                var admin = await GetByUsernameAsync(username);

                if (admin == null)
                {
                    Console.WriteLine("Admin not found");
                    return false;
                }

                Console.WriteLine("Admin found, verifying password");
                // For testing purposes, add direct comparison as a fallback
                bool isVerified = false;

                try
                {
                    isVerified = PasswordHasher.VerifyPassword(password, admin.Password);
                    Console.WriteLine($"Password verification result: {isVerified}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Password verification error: {ex.Message}");
                    // Hard-coded fallback for testing - remove in production!
                    if (username == "admin" && password == "admin123")
                    {
                        Console.WriteLine("Using fallback verification");
                        isVerified = true;
                    }
                }

                if (isVerified)
                {
                    admin.LastLogin = DateTime.Now;
                    await UpdateAsync(admin.Id!, admin);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Authentication error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ChangePasswordAsync(string adminId, string currentPassword, string newPassword)
        {
            var admin = await GetByIdAsync(adminId);
            if (admin != null && admin.Password == currentPassword) // In a real app, use password hashing
            {
                admin.Password = newPassword; // In a real app, hash the new password
                await UpdateAsync(adminId, admin);
                return true;
            }
            return false;
        }
    }
}