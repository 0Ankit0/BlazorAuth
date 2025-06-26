using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorAuth.Components.Account
{
    public partial class PersonalData : ComponentBase
    {
        [CascadingParameter] private Task<AuthenticationState>? authenticationStateTask { get; set; }
        private string? userEmail;
        private string? statusMessage;

        [Inject] private UserManager<IdentityUser> UserManager { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            var authState = await authenticationStateTask;
            var user = await UserManager.GetUserAsync(authState.User);
            userEmail = user?.Email;
        }

        private async Task DeletePersonalData()
        {
            var authState = await authenticationStateTask;
            var user = await UserManager.GetUserAsync(authState.User);
            if (user == null)
            {
                statusMessage = "User not found.";
                return;
            }
            var result = await UserManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                statusMessage = "Personal data deleted.";
                NavigationManager.NavigateTo("/");
            }
            else
            {
                statusMessage = string.Join(" ", result.Errors.Select(e => e.Description));
            }
        }
    }
}
