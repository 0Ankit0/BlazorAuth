using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using BlazorAuth.Models;

namespace BlazorAuth.Components.Account;

public partial class TwoFactorAuthentication : ComponentBase
{
    [Inject] private UserManager<IdentityUser> UserManager { get; set; } = default!;
    [Inject] private SignInManager<IdentityUser> SignInManager { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private HttpClient Http { get; set; } = default!;
    [Inject] private ILogger<TwoFactorAuthentication> Logger { get; set; } = default!;

    [Parameter] public IdentityUser? User { get; set; }
    private bool isLoading = true;
    private bool isProcessing = false;
    private string? loadError;
    private string? statusMessage;
    private bool isMachineRemembered;
    private bool hasAuthenticator;
    private int recoveryCodesLeft;
    private bool is2faEnabled;
    private bool showResetAuthenticatorModal = false;
    protected override async Task OnInitializedAsync()
    {
        await LoadAsync();
        var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
        if (Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query).TryGetValue("statusMessage", out var msg))
        {
            statusMessage = msg;
        }
    }
    private async Task LoadAsync()
    {
        isLoading = true;
        loadError = null;
        statusMessage = null;
        if (User == null)
        {
            loadError = "User not found.";
            isLoading = false;
            return;
        }
        hasAuthenticator = !string.IsNullOrEmpty(await UserManager.GetAuthenticatorKeyAsync(User));
        is2faEnabled = await UserManager.GetTwoFactorEnabledAsync(User);
        var machineRememberedResponse = await Http.PostAsJsonAsync(NavigationManager.BaseUri + "api/account/is2famachineremembered", User);
        var responseData = await machineRememberedResponse.Content.ReadFromJsonAsync<MachineRememberedResponse>();
        isMachineRemembered = responseData?.isRemembered ?? false;
        recoveryCodesLeft = await UserManager.CountRecoveryCodesAsync(User);
        isLoading = false;
    }
    private async Task ForgetBrowser()
    {
        isProcessing = true;
        statusMessage = null;
        if (User == null)
        {
            statusMessage = "User not found.";
            isProcessing = false;
            return;
        }
        await SignInManager.ForgetTwoFactorClientAsync();
        statusMessage = "The current browser has been forgotten. When you login again from this browser you will be prompted for your 2fa code.";
        isProcessing = false;
        await LoadAsync();
    }
    private void ConfirmResetAuthenticator()
    {
        showResetAuthenticatorModal = false;
        NavigationManager.NavigateTo("/account/resetauthenticator");
    }
}
