using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Threading.Tasks;

namespace BlazorAuth.Components.Account
{
    public partial class RegisterConfirmation : ComponentBase
    {
        private string? email;
        private string? emailConfirmationUrl;
        private bool displayConfirmAccountLink;

        [Inject] private UserManager<IdentityUser> UserManager { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
            var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
            if (query.TryGetValue("email", out var emailValue))
            {
                email = emailValue;
            }
            else
            {
                email = null;
                return;
            }
            displayConfirmAccountLink = true;
            if (displayConfirmAccountLink)
            {
                var user = await UserManager.FindByEmailAsync(email);
                if (user != null)
                {
                    var userId = await UserManager.GetUserIdAsync(user);
                    var code = await UserManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var returnUrl = query.TryGetValue("returnUrl", out var r) ? r.ToString() : "/";
                    emailConfirmationUrl = NavigationManager.BaseUri.TrimEnd('/') +
                        $"/account/confirmEmail?userId={Uri.EscapeDataString(userId)}&code={Uri.EscapeDataString(code)}&returnUrl={Uri.EscapeDataString(returnUrl)}";
                }
            }
        }
    }
}
