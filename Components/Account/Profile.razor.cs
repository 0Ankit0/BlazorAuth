using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace BlazorAuth.Components.Account
{
    public partial class Profile : ComponentBase
    {
        [Parameter] public IdentityUser? User { get; set; }
        private string currentUsername = string.Empty;
        private string? statusMessage;
        private bool isLoaded = false;
        private bool showChangeUsername = false;
        private InputModel inputModel = new();

        [Inject] private UserManager<IdentityUser> UserManager { get; set; } = default!;
        [Inject] private SignInManager<IdentityUser> SignInManager { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;
        [Inject] private System.Net.Http.HttpClient http { get; set; } = default!;

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
            currentUsername = await UserManager.GetUserNameAsync(User) ?? "";
            inputModel.NewUsername = currentUsername;
            isLoaded = true;
        }

        private async Task HandleChangeUsername()
        {
            statusMessage = string.Empty;
            if (User == null)
            {
                statusMessage = "User not found.";
                return;
            }

            var username = await UserManager.GetUserNameAsync(User);
            if (inputModel.NewUsername != username)
            {
                var setUsernameResult = await UserManager.SetUserNameAsync(User, inputModel.NewUsername);
                if (!setUsernameResult.Succeeded)
                {
                    statusMessage = "Unexpected error when trying to set username.";
                    return;
                }
                statusMessage = "Username changed successfully.";
                await http.GetAsync(NavigationManager.BaseUri + "api/account/refreshsignin");
            }
            else
            {
                statusMessage = "Your username is unchanged.";
            }
            showChangeUsername = false;
            await LoadAsync();
        }

        public class InputModel
        {
            [Required]
            [Display(Name = "New username")]
            public string NewUsername { get; set; } = string.Empty;
        }
    }
}
