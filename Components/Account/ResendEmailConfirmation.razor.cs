using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;

namespace BlazorAuth.Components.Account
{
    public partial class ResendEmailConfirmation : ComponentBase
    {
        private InputModel inputModel = new();
        private string? statusMessage;

        [Inject] private UserManager<IdentityUser> UserManager { get; set; } = default!;
        [Inject] private IEmailSender EmailSender { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;
        }

        private async Task HandleResend()
        {
            statusMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(inputModel.Email))
            {
                statusMessage = "Please enter a valid email address.";
                return;
            }
            var user = await UserManager.FindByEmailAsync(inputModel.Email);
            statusMessage = "Verification email sent. Please check your email.";
            if (user == null)
            {
                return;
            }
            var userId = await UserManager.GetUserIdAsync(user);
            var code = await UserManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = NavigationManager.BaseUri.TrimEnd('/') +
                $"/account/confirmemail?userId={Uri.EscapeDataString(userId)}&code={Uri.EscapeDataString(code)}";
            await EmailSender.SendEmailAsync(
                inputModel.Email,
                "Confirm your email",
                $"Please confirm your account by <a href='{callbackUrl}'>clicking here</a>.");
        }
    }
}
