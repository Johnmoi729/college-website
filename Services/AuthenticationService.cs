using Microsoft.AspNetCore.Http;
using Microsoft.JSInterop;
using System.Text.Json;

namespace CollegeWebsite.Services
{
    public interface IAuthenticationService
    {
        Task<bool> LoginAsync(string username, string password);
        Task<bool> EmergencyLoginAsync();
        void Logout();
        bool IsAuthenticated();
        string? GetCurrentUser();
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly AdminService _adminService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthStateService _authStateService;
        private readonly IJSRuntime _jsRuntime;
        private const string SessionKey = "CurrentAdmin";

        public AuthenticationService(
            AdminService adminService,
            IHttpContextAccessor httpContextAccessor,
            IAuthStateService authStateService,
            IJSRuntime jsRuntime)
        {
            _adminService = adminService;
            _httpContextAccessor = httpContextAccessor;
            _authStateService = authStateService;
            _jsRuntime = jsRuntime;
            Console.WriteLine("AuthenticationService initialized");
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                Console.WriteLine($"AuthService.LoginAsync called with username: {username}");

                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    Console.WriteLine("Login failed: Username or password is empty");
                    return false;
                }

                // Clear existing auth state first
                await _authStateService.ClearAuthStateAsync();

                Console.WriteLine("Checking admin credentials...");

                // Hard-coded admin credentials for development
                if (username == "admin" && password == "admin123")
                {
                    Console.WriteLine("✓ Development admin credentials accepted");

                    // Create admin session data
                    var sessionAdmin = new
                    {
                        Id = "admin123",
                        Username = "admin",
                        FullName = "Administrator",
                        Role = "Admin"
                    };

                    // Store auth state via our service
                    var sessionJson = System.Text.Json.JsonSerializer.Serialize(sessionAdmin);
                    await _authStateService.SetAuthStateAsync(true, sessionJson);

                    // Also store directly in HttpContext session as a backup
                    var httpContext = _httpContextAccessor.HttpContext;
                    if (httpContext != null)
                    {
                        httpContext.Session.SetString(SessionKey, sessionJson);
                        await httpContext.Session.CommitAsync();
                        Console.WriteLine("Directly set session value as backup");
                    }

                    return true;
                }

                // Regular database authentication
                var isAuthenticated = await _adminService.AuthenticateAsync(username, password);
                Console.WriteLine($"Database authentication result: {isAuthenticated}");

                if (isAuthenticated)
                {
                    var admin = await _adminService.GetByUsernameAsync(username);

                    if (admin != null)
                    {
                        // Create auth state data
                        var sessionAdmin = new
                        {
                            admin.Id,
                            admin.Username,
                            FullName = admin.FullName ?? "Admin User",
                            admin.Role
                        };

                        var sessionJson = JsonSerializer.Serialize(sessionAdmin);
                        await _authStateService.SetAuthStateAsync(true, sessionJson);

                        // Also store directly in HttpContext session as a backup
                        var httpContext = _httpContextAccessor.HttpContext;
                        if (httpContext != null)
                        {
                            httpContext.Session.SetString(SessionKey, sessionJson);
                            await httpContext.Session.CommitAsync();
                        }

                        return true;
                    }
                    else
                    {
                        Console.WriteLine("Authentication succeeded but failed to retrieve admin details");
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AuthenticationService.LoginAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        public bool IsAuthenticated()
        {
            try
            {
                // First check direct session access
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null)
                {
                    var sessionData = httpContext.Session.GetString(SessionKey);
                    if (!string.IsNullOrEmpty(sessionData))
                    {
                        Console.WriteLine("Found auth data directly in session");
                        return true;
                    }
                }

                // Then fall back to the auth state service
                var authResult = _authStateService.IsAuthenticatedAsync().GetAwaiter().GetResult();
                Console.WriteLine($"AuthService.IsAuthenticated checking via AuthStateService: {authResult}");
                return authResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in IsAuthenticated: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> EmergencyLoginAsync()
        {
            try
            {
                // Create a mock admin session
                var sessionAdmin = new
                {
                    Id = "emergency123",
                    Username = "admin",
                    FullName = "Emergency Admin",
                    Role = "Admin"
                };

                var sessionJson = JsonSerializer.Serialize(sessionAdmin);

                // Store auth state via our service
                await _authStateService.SetAuthStateAsync(true, sessionJson);

                // Also store directly in HttpContext session
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null)
                {
                    httpContext.Session.SetString(SessionKey, sessionJson);
                    await httpContext.Session.CommitAsync();
                }

                Console.WriteLine("Emergency admin session created successfully");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Emergency login error: {ex.Message}");
                return false;
            }
        }

        public void Logout()
        {
            try
            {
                // Clear auth state service
                _authStateService.ClearAuthStateAsync().GetAwaiter().GetResult();

                // Clear session directly
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null)
                {
                    httpContext.Session.Remove(SessionKey);
                    httpContext.Session.CommitAsync().GetAwaiter().GetResult();
                }

                Console.WriteLine("User logged out - auth state cleared");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logout error: {ex.Message}");
            }
        }

        public string? GetCurrentUser()
        {
            try
            {
                // Try to get from session first
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null)
                {
                    var sessionData = httpContext.Session.GetString(SessionKey);
                    if (!string.IsNullOrEmpty(sessionData))
                    {
                        try
                        {
                            using JsonDocument doc = JsonDocument.Parse(sessionData);
                            JsonElement root = doc.RootElement;
                            if (root.TryGetProperty("FullName", out JsonElement fullNameElement))
                            {
                                return fullNameElement.GetString();
                            }
                        }
                        catch { }
                    }
                }

                // Fall back to auth state service
                var userData = _authStateService.GetUserDataAsync().GetAwaiter().GetResult();
                if (!string.IsNullOrEmpty(userData))
                {
                    try
                    {
                        using JsonDocument doc = JsonDocument.Parse(userData);
                        JsonElement root = doc.RootElement;
                        if (root.TryGetProperty("FullName", out JsonElement fullNameElement))
                        {
                            return fullNameElement.GetString();
                        }
                    }
                    catch { }
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting current user: {ex.Message}");
                return null;
            }
        }
    }
}