using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlazorAuth.Components.Account
{
    public partial class ChangePassword : ComponentBase
    {
        [CascadingParameter] private Task<AuthenticationState> authenticationStateTask { get; set; } = default!;
        [Inject] private UserManager<IdentityUser> UserManager { get; set; } = default!;
        [Inject] private SignInManager<IdentityUser> SignInManager { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;
        [Inject] private HttpClient Http { get; set; } = default!;

        private ChangePasswordModel changePasswordModel = new();
        private string? statusMessage;

        protected override async Task OnInitializedAsync()
        {
            var authState = await authenticationStateTask;
            var user = await UserManager.GetUserAsync(authState.User);
            if (user == null)
            {
                statusMessage = $"Unable to load user.";
                return;
            }

            var hasPassword = await UserManager.HasPasswordAsync(user);
            if (!hasPassword)
            {
                // Redirect to set password page if user doesn't have a password
                NavigationManager.NavigateTo("/account/setpassword", true);
            }
        }

        private async Task HandleChangePassword()
        {
            statusMessage = string.Empty;

            if (!ValidateModel())
                return;

            var authState = await authenticationStateTask;
            var user = await UserManager.GetUserAsync(authState.User);
            if (user == null)
            {
                statusMessage = $"Unable to load user.";
                return;
            }

            var result = await UserManager.ChangePasswordAsync(user, changePasswordModel.OldPassword!, changePasswordModel.NewPassword!);
            if (!result.Succeeded)
            {
                statusMessage = string.Join(" ", result.Errors.Select(e => e.Description));
                return;
            }

            // Call the refresh sign-in endpoint
            var response = await Http.PostAsync(NavigationManager.BaseUri + "api/account/refreshsignin", null);
            if (response.IsSuccessStatusCode)
            {
                statusMessage = "Your password has been changed and your sign-in has been refreshed.";
            }
            else
            {
                statusMessage = "Password changed, but failed to refresh sign-in.";
            }

            // Optionally, reload the page to clear the form and show the status message
            NavigationManager.NavigateTo(NavigationManager.Uri, forceLoad: true);
        }

        private bool ValidateModel()
        {
            // Manual validation for max length (since [StringLength] is not enforced by default in Blazor)
            if (string.IsNullOrWhiteSpace(changePasswordModel.NewPassword) || changePasswordModel.NewPassword.Length < 6)
            {
                statusMessage = "The new password must be at least 6 characters long.";
                return false;
            }
            if (changePasswordModel.NewPassword.Length > 100)
            {
                statusMessage = "The new password must be at most 100 characters long.";
                return false;
            }
            if (changePasswordModel.NewPassword != changePasswordModel.ConfirmPassword)
            {
                statusMessage = "The new password and confirmation password do not match.";
                return false;
            }
            return true;
        }

        public class ChangePasswordModel
        {
            [Required]
            [DataType(DataType.Password)]
            public string? OldPassword { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The new password must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string? NewPassword { get; set; }

            [DataType(DataType.Password)]
            [Compare(nameof(NewPassword), ErrorMessage = "The new password and confirmation password do not match.")]
            public string? ConfirmPassword { get; set; }
        }
    }
}
