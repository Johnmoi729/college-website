using CollegeWebsite.Models;
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

        public async Task<bool> AuthenticateAsync(string username, string password)
        {
            var admin = await GetByUsernameAsync(username);
            if (admin != null && admin.Password == password) // In a real app, use password hashing
            {
                admin.LastLogin = DateTime.Now;
                await UpdateAsync(admin.Id!, admin);
                return true;
            }
            return false;
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