using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlazorAuth.Components.Account
{
    public partial class ManageNav : ComponentBase
    {
        private bool isAuthenticated;

        [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;
        [Inject] private HttpClient Http { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            isAuthenticated = authState.User.Identity?.IsAuthenticated ?? false;
        }

        private async Task LogoutAsync()
        {
            NavigationManager.NavigateTo("/api/account/logout",forceLoad:true);
        }
    }
}
