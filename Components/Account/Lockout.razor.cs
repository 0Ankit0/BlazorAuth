using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Threading.Tasks;

namespace BlazorAuth.Components.Account
{
    public partial class Lockout : ComponentBase
    {
        private string? Message;
        private bool IsLoading = false;
        private string Email = string.Empty;

        [Inject] private UserManager<IdentityUser> UserManager { get; set; } = default!;
        [Inject] private IEmailSender EmailSender { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;

        private async Task RequestCode()
        {
            IsLoading = true;
            Message = string.Empty;
            try
            {
                var user = await UserManager.FindByEmailAsync(Email);
                Message = "If an account exists for this username, an unlock link has been sent.";

                if (user == null)
                {
                    return;
                }

                var code = await UserManager.GenerateUserTokenAsync(user, TokenOptions.DefaultProvider, "RemoveLockout");
                var encodedCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var unlockUrl = NavigationManager.BaseUri.TrimEnd('/') +
                    $"/account/unlock?email={Uri.EscapeDataString(Email)}&code={Uri.EscapeDataString(encodedCode)}";

                string emailBody = $@"
                    <p>Your account is locked out. Click the link below to unlock your account:</p>
                    <p><a href=""{unlockUrl}"">Unlock Account</a></p>
                    <p>If you did not request this, you can ignore this email.</p>
                ";

                await EmailSender.SendEmailAsync(Email, "Unlock Your Account", emailBody);
            }
            catch
            {
                Message = "An error occurred while sending the unlock link.";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
