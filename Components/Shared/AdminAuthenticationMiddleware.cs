// Create AdminAuthenticationMiddleware.cs:
using Microsoft.AspNetCore.Components;
using CollegeWebsite.Models;
using CollegeWebsite.Services;
using Microsoft.AspNetCore.Components.Rendering;
using System.Threading.Tasks;

namespace CollegeWebsite.Components.Shared
{
    public class AdminAuthComponent : ComponentBase
    {
        [Inject]
        protected IAuthenticationService AuthService { get; set; } = null!;

        [Inject]
        protected NavigationManager NavigationManager { get; set; } = null!;

        [Parameter]
        public RenderFragment? ChildContent { get; set; }

        protected override void OnInitialized()
        {
            if (!AuthService.IsAuthenticated())
            {
                NavigationManager.NavigateTo("/admin/login", true);
            }
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (AuthService.IsAuthenticated())
            {
                builder.AddContent(0, ChildContent);
            }
        }
    }
}