using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace BlazorAuth.Components.Account
{
    public partial class ConfirmPhone : ComponentBase
    {
        [CascadingParameter] private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;
        [Parameter]
        [SupplyParameterFromQuery]
        public string PhoneNo { get; set; } = string.Empty;
        private string code = string.Empty;
        private string? statusMessage;

        [Inject] private NavigationManager NavigationManager { get; set; } = default!;
        [Inject] private UserManager<IdentityUser> UserManager { get; set; } = default!;

        private async Task ConfirmCode()
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                statusMessage = "Please enter a code.";
                return;
            }
            var user = (await AuthenticationStateTask).User;
            if (!user.Identity.IsAuthenticated)
            {
                statusMessage = "You must be logged in to confirm your phone.";
                return;
            }
            var identityUser = await UserManager.GetUserAsync(user);
            if (identityUser == null)
            {
                statusMessage = "Unable to load user.";
                return;
            }

            var result = await UserManager.ChangePhoneNumberAsync(identityUser, PhoneNo, code);

            if (result.Succeeded)
            {
                statusMessage = "Phone number confirmed successfully.";
                NavigationManager.NavigateTo("/account/manage/profile");
            }
            else
            {
                statusMessage = "Invalid confirmation code. Please try again.";
            }
        }
    }
}
