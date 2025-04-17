using Microsoft.JSInterop;
using System.Text.Json;

namespace CollegeWebsite.Services
{
    public interface IAuthStateService
    {
        Task<bool> IsAuthenticatedAsync();
        Task SetAuthStateAsync(bool isAuthenticated, string userData);
        Task ClearAuthStateAsync();
        Task<string?> GetUserDataAsync();
        Task NotifyRenderCompleteAsync();
    }

    public class AuthStateService : IAuthStateService
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string SessionKey = "CurrentAdmin";
        private const string LocalStorageKey = "AdminAuthState";

        private bool? _cachedAuthState;
        private string? _cachedUserData;
        private bool _isClientSide = false;

        public AuthStateService(
            IJSRuntime jsRuntime,
            IHttpContextAccessor httpContextAccessor)
        {
            _jsRuntime = jsRuntime;
            _httpContextAccessor = httpContextAccessor;
            Console.WriteLine("AuthStateService initialized");
        }

        public async Task NotifyRenderCompleteAsync()
        {
            _isClientSide = true;
            Console.WriteLine("AuthStateService: Rendering complete, JS interop now available");
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            // First check memory cache for fastest response
            if (_cachedAuthState.HasValue)
            {
                Console.WriteLine($"Using cached auth state: {_cachedAuthState.Value}");
                return _cachedAuthState.Value;
            }

            // Then check session
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                try
                {
                    var sessionData = httpContext.Session.GetString(SessionKey);
                    if (!string.IsNullOrEmpty(sessionData))
                    {
                        Console.WriteLine("Found auth state in session");
                        _cachedAuthState = true;
                        _cachedUserData = sessionData;
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("No auth data found in session");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error accessing session: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("HttpContext is null when checking authentication");
            }

            // Only try localStorage if we're client-side (not during prerendering)
            if (_isClientSide)
            {
                try
                {
                    var storageData = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", LocalStorageKey);
                    if (!string.IsNullOrEmpty(storageData))
                    {
                        Console.WriteLine("Found auth state in localStorage");

                        // Restore the session from localStorage if possible
                        if (httpContext != null)
                        {
                            httpContext.Session.SetString(SessionKey, storageData);
                            await httpContext.Session.CommitAsync();
                            Console.WriteLine("Restored session data from localStorage");
                        }

                        _cachedAuthState = true;
                        _cachedUserData = storageData;
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("No auth data found in localStorage");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error accessing localStorage: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Skipping localStorage check (client-side rendering not ready)");
            }

            _cachedAuthState = false;
            return false;
        }

        public async Task SetAuthStateAsync(bool isAuthenticated, string userData)
        {
            // Update memory cache
            _cachedAuthState = isAuthenticated;
            _cachedUserData = isAuthenticated ? userData : null;

            Console.WriteLine($"Setting auth state: {isAuthenticated}, Data length: {userData?.Length ?? 0}");

            // Update session
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                try
                {
                    if (isAuthenticated && !string.IsNullOrEmpty(userData))
                    {
                        httpContext.Session.SetString(SessionKey, userData);
                        await httpContext.Session.CommitAsync();
                        Console.WriteLine("Stored auth data in session");
                    }
                    else
                    {
                        httpContext.Session.Remove(SessionKey);
                        await httpContext.Session.CommitAsync();
                        Console.WriteLine("Removed auth data from session");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error modifying session: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("WARNING: HttpContext is null when setting auth state");
            }

            // Only update localStorage if we're client-side
            if (_isClientSide)
            {
                try
                {
                    if (isAuthenticated && !string.IsNullOrEmpty(userData))
                    {
                        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", LocalStorageKey, userData);
                        Console.WriteLine("Stored auth data in localStorage");
                    }
                    else
                    {
                        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", LocalStorageKey);
                        Console.WriteLine("Removed auth data from localStorage");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating localStorage: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Skipping localStorage update (client-side rendering not ready)");
            }
        }

        public async Task ClearAuthStateAsync()
        {
            Console.WriteLine("Clearing all auth state");
            await SetAuthStateAsync(false, string.Empty);
        }

        public async Task<string?> GetUserDataAsync()
        {
            // First check memory cache
            if (!string.IsNullOrEmpty(_cachedUserData))
            {
                return _cachedUserData;
            }

            // Then check session
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                try
                {
                    var sessionData = httpContext.Session.GetString(SessionKey);
                    if (!string.IsNullOrEmpty(sessionData))
                    {
                        _cachedUserData = sessionData;
                        return sessionData;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error getting user data from session: {ex.Message}");
                }
            }

            // Only try localStorage if we're client-side
            if (_isClientSide)
            {
                try
                {
                    var storageData = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", LocalStorageKey);
                    if (!string.IsNullOrEmpty(storageData))
                    {
                        _cachedUserData = storageData;
                        return storageData;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error getting user data from localStorage: {ex.Message}");
                }
            }

            return null;
        }
    }
}