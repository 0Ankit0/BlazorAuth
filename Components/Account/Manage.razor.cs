using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace BlazorAuth.Components.Account
{
    public partial class Manage : ComponentBase
    {
        [Parameter]
        public string? Section { get; set; }
        private bool _navigatedToSection = false;
        private string activeSection = "profile";
        private IdentityUser? user;
        [CascadingParameter] private Task<AuthenticationState>? authenticationStateTask { get; set; }
        private bool isLoaded = false;

        [Inject] private UserManager<IdentityUser> UserManager { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            if (authenticationStateTask != null)
            {
                var authState = await authenticationStateTask;
                user = await UserManager.GetUserAsync(authState.User);
            }
            isLoaded = true;
        }

        protected override void OnParametersSet()
        {
            if (string.IsNullOrWhiteSpace(Section))
            {
                if (!_navigatedToSection)
                {
                    _navigatedToSection = true;
                    NavigationManager.NavigateTo("/account/manage/profile", forceLoad: false);
                }
                activeSection = "profile";
            }
            else
            {
                activeSection = Section;
            }
        }

        private void SetSection(string section)
        {
            if (activeSection != section)
            {
                NavigationManager.NavigateTo($"/account/manage/{section}", forceLoad: false);
            }
        }
    }
}
