using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace BlazorAuth.Components.Account
{
    public partial class ConfirmEmailChange : ComponentBase
    {
        [Parameter, SupplyParameterFromQuery]
        public string? UserId { get; set; }
        [Parameter, SupplyParameterFromQuery]
        public string? Email { get; set; }
        [Parameter, SupplyParameterFromQuery]
        public string? Code { get; set; }

        private string? statusMessage;
        private bool isLoading = true;

        [Inject] private UserManager<IdentityUser> UserManager { get; set; } = default!;
        [Inject] private SignInManager<IdentityUser> SignInManager { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            if (string.IsNullOrEmpty(UserId) ||
                string.IsNullOrEmpty(Email) ||
                string.IsNullOrEmpty(Code))
            {
                NavigationManager.NavigateTo("/", forceLoad: true);
                return;
            }

            var user = await UserManager.FindByIdAsync(UserId);
            if (user == null)
            {
                statusMessage = $"Unable to load user with ID '{UserId}'.";
                isLoading = false;
                return;
            }

            try
            {
                var decodedCode = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(Code));
                var result = await UserManager.ChangeEmailAsync(user, Email, decodedCode);
                if (!result.Succeeded)
                {
                    statusMessage = string.Join(" ", result.Errors.Select(e => e.Description));
                    isLoading = false;
                    return;
                }

                await SignInManager.RefreshSignInAsync(user);
                statusMessage = "Thank you for confirming your email change.";
            }
            catch
            {
                statusMessage = "Invalid confirmation link.";
            }
            isLoading = false;
        }
    }
}
