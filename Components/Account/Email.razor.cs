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
    public partial class Email : ComponentBase
    {
        [Parameter] public IdentityUser? User { get; set; }
        private string currentEmail = string.Empty;
        private bool isEmailConfirmed;
        private string? statusMessage;
        private bool isLoaded = false;
        private bool showChangeEmail = false;
        private InputModel inputModel = new();

        [Inject] private UserManager<IdentityUser> UserManager { get; set; } = default!;
        [Inject] private IEmailSender EmailSender { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;

        protected override async Task OnParametersSetAsync()
        {
            await LoadAsync();
        }

        private async Task LoadAsync()
        {
            if (User == null)
            {
                statusMessage = "User not found.";
                isLoaded = false;
                return;
            }
            currentEmail = await UserManager.GetEmailAsync(User);
            inputModel.NewEmail = currentEmail;
            isEmailConfirmed = await UserManager.IsEmailConfirmedAsync(User);
            isLoaded = true;
        }

        private async Task HandleChangeEmail()
        {
            statusMessage = string.Empty;
            if (User == null)
            {
                statusMessage = "User not found.";
                return;
            }

            var email = await UserManager.GetEmailAsync(User);
            if (inputModel.NewEmail != email)
            {
                var userId = await UserManager.GetUserIdAsync(User);
                var code = await UserManager.GenerateChangeEmailTokenAsync(User, inputModel.NewEmail);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = NavigationManager.BaseUri.TrimEnd('/') +
                    $"/account/confirmemailchange?userId={Uri.EscapeDataString(userId)}&email={Uri.EscapeDataString(inputModel.NewEmail)}&code={Uri.EscapeDataString(code)}";
                await EmailSender.SendEmailAsync(
                    inputModel.NewEmail,
                    "Confirm your email",
                    $"Please confirm your account by <a href='{callbackUrl}'>clicking here</a>.");

                statusMessage = "Confirmation link to change email sent. Please check your email.";
            }
            else
            {
                statusMessage = "Your email is unchanged.";
            }
            showChangeEmail = false;
            await LoadAsync();
        }

        private async Task SendVerificationEmail()
        {
            statusMessage = string.Empty;
            if (User == null)
            {
                statusMessage = "User not found.";
                return;
            }

            var userId = await UserManager.GetUserIdAsync(User);
            var email = await UserManager.GetEmailAsync(User);
            var code = await UserManager.GenerateEmailConfirmationTokenAsync(User);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = NavigationManager.BaseUri.TrimEnd('/') +
                $"/account/confirmemail?userId={Uri.EscapeDataString(userId)}&code={Uri.EscapeDataString(code)}";
            await EmailSender.SendEmailAsync(
                email!,
                "Confirm your email",
                $"Please confirm your account by <a href='{callbackUrl}'>clicking here</a>.");

            statusMessage = "Verification email sent. Please check your email.";
        }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "New email")]
            public string NewEmail { get; set; } = string.Empty;
        }
    }
}
