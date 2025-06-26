using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;

namespace BlazorAuth.Components.Account;

public partial class SetPassword : ComponentBase
{
    [Inject] private UserManager<IdentityUser> UserManager { get; set; } = default!;
    [Inject] private SignInManager<IdentityUser> SignInManager { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [CascadingParameter] private Task<AuthenticationState>? authenticationStateTask { get; set; }
    private SetPasswordModel setPasswordModel = new SetPasswordModel();
    private string? statusMessage;
    private bool hasPassword = false;
    protected override async Task OnInitializedAsync()
    {
        if (authenticationStateTask == null)
        {
            statusMessage = "Invalid authentication state.";
            return;
        }
        var authState = await authenticationStateTask;
        var user = await UserManager.GetUserAsync(authState.User);
        if (user == null)
        {
            statusMessage = "User not found.";
            return;
        }
        hasPassword = await UserManager.HasPasswordAsync(user);
        if (hasPassword)
        {
            statusMessage = "You already have a password set.";
        }
    }
    private async Task HandleSetPassword()
    {
        statusMessage = string.Empty;
        if (setPasswordModel.NewPassword != setPasswordModel.ConfirmPassword)
        {
            statusMessage = "Passwords do not match.";
            return;
        }
        if (authenticationStateTask == null || string.IsNullOrEmpty(setPasswordModel.NewPassword))
        {
            statusMessage = "Invalid state or password.";
            return;
        }
        var authState = await authenticationStateTask;
        var user = await UserManager.GetUserAsync(authState.User);
        if (user == null)
        {
            statusMessage = "User not found.";
            return;
        }
        var result = await UserManager.AddPasswordAsync(user, setPasswordModel.NewPassword);
        if (result.Succeeded)
        {
            await SignInManager.RefreshSignInAsync(user);
            statusMessage = "Password set successfully.";
            hasPassword = true;
        }
        else
        {
            statusMessage = string.Join(" ", result.Errors.Select(e => e.Description));
        }
    }
    public class SetPasswordModel
    {
        [Required]
        [MinLength(6)]
        public string? NewPassword { get; set; }
        [Required]
        [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
        public string? ConfirmPassword { get; set; }
    }
}
