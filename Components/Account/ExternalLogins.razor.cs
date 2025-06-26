using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorAuth.Components.Account
{
    public partial class ExternalLogins : ComponentBase
    {
        [CascadingParameter] private Task<AuthenticationState>? authenticationStateTask { get; set; }
        private IList<UserLoginInfo> currentLogins = new List<UserLoginInfo>();
        private IList<AuthenticationScheme> otherLogins = new List<AuthenticationScheme>();
        private bool isLoading = true;
        private string? statusMessage;
        private bool showRemoveButton = false;

        [Inject] private UserManager<IdentityUser> UserManager { get; set; } = default!;
        [Inject] private SignInManager<IdentityUser> SignInManager { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            await LoadLoginsAsync();
            var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
            if (Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query).TryGetValue("statusMessage", out var msg))
            {
                statusMessage = msg;
            }
        }

        private async Task LoadLoginsAsync()
        {
            isLoading = true;
            statusMessage = null;
            if (authenticationStateTask == null)
            {
                statusMessage = "Unable to load authentication state.";
                isLoading = false;
                return;
            }
            var authState = await authenticationStateTask;
            var user = await UserManager.GetUserAsync(authState.User);
            if (user == null)
            {
                statusMessage = "User not found.";
                isLoading = false;
                return;
            }
            currentLogins = await UserManager.GetLoginsAsync(user);
            var allSchemes = await SignInManager.GetExternalAuthenticationSchemesAsync();
            otherLogins = allSchemes.Where(s => currentLogins.All(cl => cl.LoginProvider != s.Name)).ToList();

            var hasPassword = await UserManager.HasPasswordAsync(user);
            showRemoveButton = hasPassword || currentLogins.Count > 1;

            isLoading = false;
            StateHasChanged();
        }

        private async Task RemoveLogin(string loginProvider, string providerKey)
        {
            if (authenticationStateTask == null)
            {
                statusMessage = "Unable to load authentication state.";
                return;
            }
            var authState = await authenticationStateTask;
            var user = await UserManager.GetUserAsync(authState.User);
            if (user == null)
            {
                statusMessage = "User not found.";
                return;
            }
            var result = await UserManager.RemoveLoginAsync(user, loginProvider, providerKey);
            if (result.Succeeded)
            {
                await SignInManager.RefreshSignInAsync(user);
                statusMessage = "The external login was removed.";
                await LoadLoginsAsync();
            }
            else
            {
                statusMessage = "The external login was not removed.";
            }
        }

        private void LinkLogin(string provider)
        {
            var returnUrl = NavigationManager.BaseUri.TrimEnd('/') + "/account/externallogins";
            NavigationManager.NavigateTo($"/account/externalLoginLink?provider={Uri.EscapeDataString(provider)}&returnUrl={Uri.EscapeDataString(returnUrl)}", forceLoad: true);
        }
    }
}
