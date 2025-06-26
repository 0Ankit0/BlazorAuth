using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace BlazorAuth.Components.Account;

public partial class ResetAuthenticator : ComponentBase
{
    [Inject] private UserManager<IdentityUser> UserManager { get; set; } = default!;
    [Inject] private SignInManager<IdentityUser> SignInManager { get; set; } = default!;
    [Inject] private ILogger<ResetAuthenticator> Logger { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private HttpClient http { get; set; } = default!;

    [CascadingParameter] private Task<AuthenticationState>? authenticationStateTask { get; set; }
    private string? statusMessage;

    protected override async Task OnInitializedAsync()
    {
        statusMessage = null;
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
        await UserManager.SetTwoFactorEnabledAsync(user, false);
        await UserManager.ResetAuthenticatorKeyAsync(user);
        var userId = await UserManager.GetUserIdAsync(user);
        Logger.LogInformation("User with ID '{UserId}' has reset their authentication app key.", userId);
        statusMessage = "Your authenticator app key has been reset, you will need to configure your authenticator app using the new key.";
        var refreshSigninResponse = await http.GetAsync(NavigationManager.BaseUri + "api/account/refreshsignin");
    }
    private void NavigateBack()
    {
        JS.InvokeVoidAsync("history.back");
    }
}
