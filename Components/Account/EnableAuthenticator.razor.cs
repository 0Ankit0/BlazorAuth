using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlazorAuth.Models;

namespace BlazorAuth.Components.Account
{
    public partial class EnableAuthenticator : ComponentBase
    {
        [CascadingParameter] private Task<AuthenticationState>? authenticationStateTask { get; set; }
        private string? sharedKey;
        private string? authenticatorUri;
        private string? statusMessage;
        private bool isLoaded = false;
        private EnableAuthenticatorInputModel enableAuthenticatorInputModel = new();
        private string[]? recoveryCodes;
        private bool showRecoveryCodes = false;

        [Inject] private UserManager<IdentityUser> UserManager { get; set; } = default!;
        [Inject] private ILogger<EnableAuthenticator> Logger { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;
        [Inject] private IJSRuntime JS { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            await LoadSharedKeyAndQrCodeUriAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!string.IsNullOrEmpty(authenticatorUri))
            {
                await JS.InvokeVoidAsync("renderQrCode", "qrCode", authenticatorUri);
            }
        }

        private async Task LoadSharedKeyAndQrCodeUriAsync()
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
                statusMessage = "User not found.";
                return;
            }

            var unformattedKey = await UserManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(unformattedKey))
            {
                await UserManager.ResetAuthenticatorKeyAsync(user);
                unformattedKey = await UserManager.GetAuthenticatorKeyAsync(user);
            }

            sharedKey = FormatKey(unformattedKey!);

            var email = await UserManager.GetEmailAsync(user);
            authenticatorUri = GenerateQrCodeUri(email, unformattedKey);

            isLoaded = true;
            StateHasChanged();
        }

        private string FormatKey(string unformattedKey)
        {
            var result = new StringBuilder();
            int currentPosition = 0;
            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.AsSpan(currentPosition, 4)).Append(' ');
                currentPosition += 4;
            }
            if (currentPosition < unformattedKey.Length)
            {
                result.Append(unformattedKey.AsSpan(currentPosition));
            }
            return result.ToString().ToLowerInvariant();
        }

        private string GenerateQrCodeUri(string email, string unformattedKey)
        {
            var issuer = "Microsoft.AspNetCore.Identity.UI";
            return string.Format(
                CultureInfo.InvariantCulture,
                "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6",
                Uri.EscapeDataString(issuer),
                Uri.EscapeDataString(email),
                unformattedKey);
        }

        private async Task OnSubmitAsync()
        {
            statusMessage = string.Empty;
            showRecoveryCodes = false;
            recoveryCodes = null;

            if (authenticationStateTask == null)
            {
                statusMessage = "Invalid state.";
                return;
            }
            var authState = await authenticationStateTask;
            var user = await UserManager.GetUserAsync(authState.User);
            if (user == null)
            {
                statusMessage = "User not found.";
                return;
            }

            if (string.IsNullOrWhiteSpace(enableAuthenticatorInputModel.Code))
            {
                statusMessage = "Verification code is required.";
                return;
            }

            var verificationCode = enableAuthenticatorInputModel.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

            var is2faTokenValid = await UserManager.VerifyTwoFactorTokenAsync(
                user, UserManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

            if (!is2faTokenValid)
            {
                statusMessage = "Verification code is invalid.";
                return;
            }

            await UserManager.SetTwoFactorEnabledAsync(user, true);
            var userId = await UserManager.GetUserIdAsync(user);
            Logger.LogInformation("User with ID '{UserId}' has enabled 2FA with an authenticator app.", userId);

            statusMessage = "Your authenticator app has been verified.";

            if (await UserManager.CountRecoveryCodesAsync(user) == 0)
            {
                var codes = await UserManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
                recoveryCodes = codes.ToArray();
                showRecoveryCodes = true;
            }
            else
            {
                NavigationManager.NavigateTo("/account/manage/2FA", forceLoad: true);
            }

            StateHasChanged();
        }
        private void NavigateBack()
        {
            JS.InvokeVoidAsync("history.back");
        }
    }
}
