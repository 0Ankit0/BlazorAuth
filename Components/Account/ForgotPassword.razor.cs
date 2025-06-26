using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;

namespace BlazorAuth.Components.Account
{
    public partial class ForgotPassword : ComponentBase
    {
        private ForgotModel forgotModel = new ForgotModel();
        private string? statusMessage;

        [Inject] private UserManager<IdentityUser> UserManager { get; set; } = default!;
        [Inject] private IEmailSender EmailSender { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;

        private async Task HandleForgotPassword()
        {
            statusMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(forgotModel.Email))
            {
                statusMessage = "Email is required.";
                return;
            }

            var user = await UserManager.FindByEmailAsync(forgotModel.Email);
            if (user == null || !(await UserManager.IsEmailConfirmedAsync(user)))
            {
                NavigationManager.NavigateTo("/account/forgotpasswordconfirmation");
                return;
            }

            var code = await UserManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = NavigationManager.BaseUri.TrimEnd('/') +
                $"/account/resetpassword?code={Uri.EscapeDataString(code)}";

            await EmailSender.SendEmailAsync(
                forgotModel.Email,
                "Reset Password",
                $"Please reset your password by <a href='{callbackUrl}'>clicking here</a>.");

            NavigationManager.NavigateTo("/account/forgotpasswordconfirmation");
        }

        public class ForgotModel
        {
            [Required]
            [EmailAddress]
            public string? Email { get; set; }
        }
    }
}
