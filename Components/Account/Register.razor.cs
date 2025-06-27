using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using BlazorAuth.Models;

namespace BlazorAuth.Components.Account
{
    public partial class Register : ComponentBase
    {
        private RegisterModel registerModel = new();
        private string? statusMessage;
        private string? returnUrl;
        private IList<AuthenticationScheme> externalLogins = new List<AuthenticationScheme>();

        [Inject] private UserManager<IdentityUser> UserManager { get; set; } = default!;
        [Inject] private SignInManager<IdentityUser> SignInManager { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;
        [Inject] private ILogger<Register> Logger { get; set; } = default!;
        [Inject] private IEmailSender EmailSender { get; set; } = default!;
        [Inject] private IServiceProvider ServiceProvider { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
            var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
            if (query.TryGetValue("returnUrl", out var returnUrlValue))
            {
                returnUrl = returnUrlValue;
            }
            else
            {
                returnUrl = "/";
            }
            externalLogins = (await SignInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        private async Task HandleRegister()
        {
            statusMessage = string.Empty;
            var context = new ValidationContext(registerModel);
            var results = new List<ValidationResult>();
            if (!Validator.TryValidateObject(registerModel, context, results, true))
            {
                statusMessage = string.Join(" ", results.Select(r => r.ErrorMessage));
                return;
            }
            var user = new IdentityUser
            {
                UserName = registerModel.Username,
                Email = registerModel.Email,
                PhoneNumber = registerModel.PhoneNo
            };
            var result = await UserManager.CreateAsync(user, registerModel.Password);
            if (result.Succeeded)
            {
                Logger.LogInformation("User created a new account with password.");
                var userId = await UserManager.GetUserIdAsync(user);
                var code = await UserManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = NavigationManager.BaseUri.TrimEnd('/') +
                    $"/Identity/Account/ConfirmEmail?userId={Uri.EscapeDataString(userId)}&code={Uri.EscapeDataString(code)}&returnUrl={Uri.EscapeDataString(returnUrl ?? "/")}";
                await EmailSender.SendEmailAsync(registerModel.Email, "Confirm your email",
                    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
                if (UserManager.Options.SignIn.RequireConfirmedAccount)
                {
                    NavigationManager.NavigateTo($"/account/registerconfirmation?email={Uri.EscapeDataString(registerModel.Email)}&returnUrl={Uri.EscapeDataString(returnUrl ?? "/")}");
                    return;
                }
                else
                {
                    statusMessage = "User Logged in Successfully.Please login to proceed.";
                    await Task.Delay(500);
                    NavigationManager.NavigateTo("/account/login");
                }
            }
            statusMessage = string.Join(" ", result.Errors.Select(e => e.Description));
        }

        private async Task ExternalLogin(string provider)
        {
            var url = $"/account/externalLoginLink?provider={Uri.EscapeDataString(provider)}&returnUrl={Uri.EscapeDataString(returnUrl ?? "/")}";
            NavigationManager.NavigateTo(url, forceLoad: true);
        }
    }
}
