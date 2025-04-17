using CollegeWebsite.Models;
using CollegeWebsite.Utilities;
using MongoDB.Driver;
using System.Diagnostics;

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
            try
            {
                Console.WriteLine($"Searching for admin with username: '{username}'");
                var results = await FindAsync(a => a.Username == username);
                var admin = results.FirstOrDefault();
                Console.WriteLine($"Admin found: {admin != null}");
                return admin;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetByUsernameAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> AuthenticateAsync(string username, string password)
        {
            Console.WriteLine("========== AUTHENTICATION ATTEMPT ==========");
            Console.WriteLine($"Username: '{username}'");
            Console.WriteLine($"Password Length: {password?.Length ?? 0}");

            try
            {
                // Handle test/development hardcoded credentials first
                if (username == "admin" && password == "admin123")
                {
                    Console.WriteLine("✓ Hardcoded admin credentials accepted");
                    return true;
                }

                // Actual database authentication
                var admin = await GetByUsernameAsync(username);

                if (admin == null)
                {
                    Console.WriteLine("✗ Admin not found in database");
                    return false;
                }

                Console.WriteLine("Admin found, checking password");

                // Plain text comparison (for development/testing only)
                if (admin.Password == password)
                {
                    Console.WriteLine("✓ Password matched directly (UNSAFE - FOR TESTING ONLY)");

                    // Update last login
                    try
                    {
                        admin.LastLogin = DateTime.Now;
                        if (!string.IsNullOrEmpty(admin.Id))
                        {
                            await UpdateAsync(admin.Id, admin);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Warning: Could not update last login time: {ex.Message}");
                    }

                    return true;
                }

                // Try hashed password verification if direct comparison failed
                try
                {
                    var isVerified = PasswordHasher.VerifyPassword(password, admin.Password);
                    Console.WriteLine($"Hashed password verification: {(isVerified ? "✓ Succeeded" : "✗ Failed")}");

                    if (isVerified)
                    {
                        // Update last login
                        admin.LastLogin = DateTime.Now;
                        if (!string.IsNullOrEmpty(admin.Id))
                        {
                            await UpdateAsync(admin.Id, admin);
                        }
                    }

                    return isVerified;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"✗ Password verification error: {ex.Message}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Authentication error: {ex.Message}");
                Debug.WriteLine($"Authentication Stack Trace: {ex.StackTrace}");
                return false;
            }
            finally
            {
                Console.WriteLine("========== END AUTHENTICATION ATTEMPT ==========");
            }
        }

        public async Task<bool> ChangePasswordAsync(string adminId, string currentPassword, string newPassword)
        {
            try
            {
                var admin = await GetByIdAsync(adminId);
                if (admin == null) return false;

                // Check current password (direct match for development)
                if (admin.Password == currentPassword || PasswordHasher.VerifyPassword(currentPassword, admin.Password))
                {
                    // For production, you would hash the new password:
                    // admin.Password = PasswordHasher.HashPassword(newPassword);

                    // For development:
                    admin.Password = newPassword;

                    await UpdateAsync(adminId, admin);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error changing password: {ex.Message}");
                return false;
            }
        }

        // Create a method to ensure at least one admin exists
        public async Task EnsureAdminExistsAsync()
        {
            try
            {
                var admins = await GetAllAsync();
                if (admins == null || !admins.Any())
                {
                    Console.WriteLine("No admins found in database, creating default admin");

                    var defaultAdmin = new Admin
                    {
                        Username = "admin",
                        Password = "admin123", // In production, use: PasswordHasher.HashPassword("admin123")
                        Email = "admin@college.edu",
                        FullName = "Default Administrator",
                        Role = "Admin",
                        IsActive = true,
                        LastLogin = DateTime.Now
                    };

                    await CreateAsync(defaultAdmin);
                    Console.WriteLine("✓ Default admin created successfully");
                }
                else
                {
                    Console.WriteLine($"Found {admins.Count} existing admin accounts");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error ensuring admin exists: {ex.Message}");
            }
        }
    }
}