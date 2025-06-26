using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Threading.Tasks;

namespace BlazorAuth.Components.Account
{
    public partial class ConfirmEmail : ComponentBase
    {
        [Parameter, SupplyParameterFromQuery]
        public string? UserId { get; set; }
        [Parameter, SupplyParameterFromQuery]
        public string? Code { get; set; }

        private string? statusMessage;
        private bool isLoading = true;
        private bool shouldRedirect = false;

        [Inject] private UserManager<IdentityUser> UserManager { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            var user = await UserManager.FindByIdAsync(UserId!);
            if (user == null)
            {
                statusMessage = $"Unable to load user with ID '{UserId}'.";
                isLoading = false;
                return;
            }

            try
            {
                var decodedCode = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(Code!));
                var result = await UserManager.ConfirmEmailAsync(user, decodedCode);
                statusMessage = result.Succeeded ? "Thank you for confirming your email." : "Error confirming your email.";
            }
            catch
            {
                statusMessage = "Invalid confirmation link.";
            }
            isLoading = false;
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if (shouldRedirect)
            {
                shouldRedirect = false;
                NavigationManager.NavigateTo("/", forceLoad: true);
            }
        }
    }
}
