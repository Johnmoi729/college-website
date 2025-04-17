using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace CollegeWebsite.Components.Shared
{
    public class AdminAuthComponent : ComponentBase
    {
        [Inject]
        protected IHttpContextAccessor HttpContextAccessor { get; set; } = null!;

        [Inject]
        protected NavigationManager NavigationManager { get; set; } = null!;

        [Inject]
        protected IJSRuntime JSRuntime { get; set; } = null!;

        [Parameter]
        public RenderFragment? ChildContent { get; set; }

        private bool isAuthenticated = false;

        protected override void OnInitialized()
        {
            // Direct, simplified authentication check
            var httpContext = HttpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                var adminData = httpContext.Session.GetString("CurrentAdmin");
                isAuthenticated = !string.IsNullOrEmpty(adminData);

                if (!isAuthenticated)
                {
                    Console.WriteLine("AdminAuthComponent: No admin session found, redirecting to login");
                    NavigationManager.NavigateTo("/direct-session-test", true); // For testing, go to our test page
                }
                else
                {
                    Console.WriteLine("AdminAuthComponent: Admin session found, proceeding");
                }
            }
            else
            {
                Console.WriteLine("AdminAuthComponent: HttpContext is null, redirecting to login");
                NavigationManager.NavigateTo("/direct-session-test", true);
            }
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (isAuthenticated)
            {
                builder.AddContent(0, ChildContent);
            }
            else
            {
                // Show a loading indicator
                builder.OpenElement(1, "div");
                builder.AddAttribute(2, "class", "d-flex justify-content-center mt-5");
                builder.OpenElement(3, "div");
                builder.AddAttribute(4, "class", "spinner-border text-primary");
                builder.AddAttribute(5, "role", "status");
                builder.OpenElement(6, "span");
                builder.AddAttribute(7, "class", "visually-hidden");
                builder.AddContent(8, "Loading...");
                builder.CloseElement(); // span
                builder.CloseElement(); // div spinner
                builder.CloseElement(); // div container
            }
        }
    }
}