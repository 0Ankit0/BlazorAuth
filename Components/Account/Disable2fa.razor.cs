using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace BlazorAuth.Components.Account
{
    public partial class Disable2fa : ComponentBase
    {
        [CascadingParameter] private Task<AuthenticationState>? authenticationStateTask { get; set; }
        private string? statusMessage;

        [Inject] private UserManager<IdentityUser> UserManager { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;
        [Inject] private ILogger<Disable2fa> Logger { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            if (authenticationStateTask == null)
            {
                statusMessage = "Invalid state.";
                return;
            }
            var authState = await authenticationStateTask;
            var user = await UserManager.GetUserAsync(authState.User);
            if (user == null)
            {
                statusMessage = "Unable to load user.";
                return;
            }

            var is2faEnabled = await UserManager.GetTwoFactorEnabledAsync(user);
            if (!is2faEnabled)
            {
                statusMessage = "2FA is not currently enabled for your account.";
                NavigationManager.NavigateTo("/account/manage/2FA", true);
                return;
            }

            var disable2faResult = await UserManager.SetTwoFactorEnabledAsync(user, false);
            if (!disable2faResult.Succeeded)
            {
                statusMessage = "Unexpected error occurred disabling 2FA.";
                return;
            }

            Logger.LogInformation("User with ID '{UserId}' has disabled 2fa.", await UserManager.GetUserIdAsync(user));
            statusMessage = "2FA has been disabled. You can reenable 2FA when you setup an authenticator app.";
            NavigationManager.NavigateTo("/account/manage/2FA", true);
        }
    }
}
