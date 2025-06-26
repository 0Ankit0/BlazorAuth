using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorAuth.Components.Account
{
    public partial class GenerateRecoveryCodes : ComponentBase
    {
        [CascadingParameter] private Task<AuthenticationState>? authenticationStateTask { get; set; }
        private bool isLoading = true;
        private bool isGenerating = false;
        private bool isTwoFactorEnabled = false;
        private string? statusMessage;
        private string[]? recoveryCodes;

        [Inject] private UserManager<IdentityUser> UserManager { get; set; } = default!;
        [Inject] private ILogger<GenerateRecoveryCodes> Logger { get; set; } = default!;
        [Inject] private IJSRuntime JS { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            await CheckTwoFactorAsync();
        }

        private async Task CheckTwoFactorAsync()
        {
            isLoading = true;
            statusMessage = null;
            recoveryCodes = null;

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
            isTwoFactorEnabled = await UserManager.GetTwoFactorEnabledAsync(user);
            isLoading = false;
        }

        private async Task GenerateCodes()
        {
            isGenerating = true;
            statusMessage = null;
            recoveryCodes = null;

            if (authenticationStateTask == null)
            {
                statusMessage = "Unable to load authentication state.";
                isGenerating = false;
                return;
            }
            var authState = await authenticationStateTask;
            var user = await UserManager.GetUserAsync(authState.User);
            if (user == null)
            {
                statusMessage = "User not found.";
                isGenerating = false;
                return;
            }
            isTwoFactorEnabled = await UserManager.GetTwoFactorEnabledAsync(user);
            if (!isTwoFactorEnabled)
            {
                statusMessage = "Cannot generate recovery codes because 2FA is not enabled.";
                isGenerating = false;
                return;
            }

            var codes = await UserManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
            recoveryCodes = codes?.ToArray();

            Logger.LogInformation("User with ID '{UserId}' has generated new 2FA recovery codes.", await UserManager.GetUserIdAsync(user));
            statusMessage = "You have generated new recovery codes.";
            isGenerating = false;
        }
        private void NavigateBack()
        {
            JS.InvokeVoidAsync("history.back");
        }
    }
}
