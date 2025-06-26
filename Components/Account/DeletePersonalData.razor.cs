using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.JSInterop;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlazorAuth.Components.Account
{
    public partial class DeletePersonalData : ComponentBase
    {
        [Parameter] public IdentityUser? User { get; set; }
        private InputModel inputModel = new();
        private string? statusMessage;
        private bool requirePassword;
        private bool showPasswordInput = false;

        [Inject] private UserManager<IdentityUser> UserManager { get; set; } = default!;
        [Inject] private SignInManager<IdentityUser> SignInManager { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;
        [Inject] private IJSRuntime JS { get; set; } = default!;
        [Inject] private HttpClient http { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            if (User == null)
            {
                statusMessage = "Unable to load User.";
                return;
            }
            requirePassword = await UserManager.HasPasswordAsync(User);
        }

        private async Task ShowPasswordInput()
        {
            var confirmed = await JS.InvokeAsync<bool>("confirm", "Are you sure you want to delete your account? This action cannot be undone.");
            if (confirmed)
            {
                showPasswordInput = true;
            }
        }

        private async Task DeletePersonalDataHandler()
        {
            statusMessage = string.Empty;

            if (User == null)
            {
                statusMessage = "Unable to load User.";
                return;
            }

            requirePassword = await UserManager.HasPasswordAsync(User);
            if (requirePassword)
            {
                if (string.IsNullOrWhiteSpace(inputModel.Password) || !await UserManager.CheckPasswordAsync(User, inputModel.Password))
                {
                    statusMessage = "Incorrect password.";
                    return;
                }
            }

            var result = await UserManager.DeleteAsync(User);
            if (!result.Succeeded)
            {
                statusMessage = "Unexpected error occurred deleting User: " + string.Join(" ", result.Errors.Select(e => e.Description));
                return;
            }

            statusMessage = "Your personal data has been deleted.";
            var signoutUserResponse = await http.GetAsync(NavigationManager.BaseUri + "api/account/logout");
            // await SignInManager.SignOutAsync();
            // NavigationManager.NavigateTo("/", true);
        }

        public class InputModel
        {
            [Required(ErrorMessage = "Password is required.")]
            [DataType(DataType.Password)]
            public string? Password { get; set; }
        }
    }
}
