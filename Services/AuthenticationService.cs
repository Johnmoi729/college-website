using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace CollegeWebsite.Services
{
    public interface IAuthenticationService
    {
        Task<bool> LoginAsync(string username, string password);
        void Logout();
        bool IsAuthenticated();
        string? GetCurrentUser();
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly AdminService _adminService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string SessionKey = "CurrentAdmin";

        public AuthenticationService(AdminService adminService, IHttpContextAccessor httpContextAccessor)
        {
            _adminService = adminService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            _httpContextAccessor.HttpContext?.Session.Clear();
            var isAuthenticated = await _adminService.AuthenticateAsync(username, password);
            if (isAuthenticated)
            {
                var admin = await _adminService.GetByUsernameAsync(username);
                if (admin != null)
                {
                    // Store user in session
                    var sessionAdmin = new { admin.Id, admin.Username, admin.FullName, admin.Role };
                    _httpContextAccessor.HttpContext?.Session.SetString(SessionKey, JsonSerializer.Serialize(sessionAdmin));
                    _httpContextAccessor.HttpContext?.Response.Cookies.Append(
                    "X-Session-Id",
                    Guid.NewGuid().ToString(),
                    new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        MaxAge = TimeSpan.FromMinutes(30)
                    });
                    return true;
                }
            }
            return false;
        }

        public void Logout()
        {
            _httpContextAccessor.HttpContext?.Session.Remove(SessionKey);
        }

        public bool IsAuthenticated()
        {
            return !string.IsNullOrEmpty(_httpContextAccessor.HttpContext?.Session.GetString(SessionKey));
        }

        public string? GetCurrentUser()
        {
            var adminJson = _httpContextAccessor.HttpContext?.Session.GetString(SessionKey);
            if (!string.IsNullOrEmpty(adminJson))
            {
                try
                {
                    using JsonDocument doc = JsonDocument.Parse(adminJson);
                    JsonElement root = doc.RootElement;
                    if (root.TryGetProperty("FullName", out JsonElement fullNameElement))
                    {
                        return fullNameElement.GetString();
                    }
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }
    }
}