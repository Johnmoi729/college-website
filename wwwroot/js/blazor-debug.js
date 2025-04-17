// Monitor Blazor initialization
window.addEventListener("DOMContentLoaded", function () {
  console.log("DOM content loaded, waiting for Blazor init...");

  // Check if Blazor is already available
  if (window.Blazor) {
    console.log("Blazor already initialized at page load");
  }

  // Monitor for Blazor object
  let checkInterval = setInterval(function () {
    if (window.Blazor) {
      console.log("Blazor initialized!");
      clearInterval(checkInterval);
    }
  }, 500);

  // Monitor for errors
  window.addEventListener("error", function (e) {
    console.error("Global error caught:", e.message, e.filename, e.lineno);
  });
});
