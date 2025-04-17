// This file helps with Blazor initialization and provides debugging support
window.blazorHelpers = {
  // Logging wrapper to help with debugging
  log: function (message, type = "info") {
    const types = {
      info: console.log,
      warn: console.warn,
      error: console.error,
      debug: console.debug,
    };

    const logFunc = types[type] || console.log;
    logFunc(`[Blazor] ${message}`);
  },

  // Check Blazor status
  checkBlazorStatus: function () {
    const status = {
      blazorLoaded: typeof window.Blazor !== "undefined",
      signalRLoaded: typeof window.signalR !== "undefined",
      serverLoaded: typeof window._blazor !== "undefined",
      localStorageAvailable: false,
      sessionStorageAvailable: false,
    };

    try {
      localStorage.setItem("test", "test");
      localStorage.removeItem("test");
      status.localStorageAvailable = true;
    } catch (e) {
      status.localStorageError = e.message;
    }

    try {
      sessionStorage.setItem("test", "test");
      sessionStorage.removeItem("test");
      status.sessionStorageAvailable = true;
    } catch (e) {
      status.sessionStorageError = e.message;
    }

    return status;
  },

  // Initialize environment
  init: function () {
    this.log("Initializing Blazor helpers");

    // Monitor for Blazor loading
    if (window.Blazor) {
      this.log("Blazor already loaded");
    } else {
      let checkCount = 0;
      const checkInterval = setInterval(() => {
        checkCount++;
        if (window.Blazor) {
          this.log("Blazor loaded successfully");
          clearInterval(checkInterval);
        } else if (checkCount > 50) {
          this.log("Blazor failed to load after 5 seconds", "error");
          clearInterval(checkInterval);
        }
      }, 100);
    }

    // Set up global error handler
    window.addEventListener("error", (e) => {
      this.log(
        `Global error: ${e.message} at ${e.filename}:${e.lineno}`,
        "error"
      );
    });

    // Set up unhandled promise rejection handler
    window.addEventListener("unhandledrejection", (e) => {
      this.log(`Unhandled promise rejection: ${e.reason}`, "error");
    });

    return true;
  },

  // Helper for authentication state
  setAuthState: function (isAuthenticated, userData) {
    try {
      if (isAuthenticated && userData) {
        localStorage.setItem(
          "authState",
          JSON.stringify({
            isAuthenticated: true,
            userData: userData,
            timestamp: new Date().toISOString(),
          })
        );
        this.log("Auth state saved to localStorage");
        return true;
      } else {
        localStorage.removeItem("authState");
        this.log("Auth state cleared from localStorage");
        return true;
      }
    } catch (e) {
      this.log(`Error setting auth state: ${e.message}`, "error");
      return false;
    }
  },

  // Get authentication state
  getAuthState: function () {
    try {
      const authData = localStorage.getItem("authState");
      if (authData) {
        return JSON.parse(authData);
      }
      return null;
    } catch (e) {
      this.log(`Error getting auth state: ${e.message}`, "error");
      return null;
    }
  },
};

// Initialize on page load
document.addEventListener("DOMContentLoaded", function () {
  window.blazorHelpers.init();
});
