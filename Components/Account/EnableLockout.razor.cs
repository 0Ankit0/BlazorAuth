using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.JSInterop;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlazorAuth.Components.Account
{
    public partial class EnableLockout : ComponentBase
    {
        [Parameter] public IdentityUser? User { get; set; }
        private InputModel inputModel = new();
        private string? statusMessage;
        private bool requirePassword;
        private bool showPasswordInput = false;

        [Inject] private UserManager<IdentityUser> UserManager { get; set; } = default!;
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
            statusMessage = string.Empty;
            if (User == null)
            {
                statusMessage = "Unable to load User.";
                return;
            }
            showPasswordInput = true;
        }

        private async Task EnableLockoutHandler()
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

            var result = await UserManager.SetLockoutEndDateAsync(User, DateTimeOffset.UtcNow.AddYears(100));
            if (result.Succeeded)
            {
                statusMessage = "Your account has been locked out. You will need to unlock it via the unlock process.";
                var signoutUserResponse = await http.GetAsync(NavigationManager.BaseUri + "api/account/logout");
            }
            else
            {
                statusMessage = "Unexpected error occurred enabling lockout: " + string.Join(" ", result.Errors.Select(e => e.Description));
            }
        }

        public class InputModel
        {
            [Required(ErrorMessage = "Password is required.")]
            [DataType(DataType.Password)]
            public string? Password { get; set; }
        }
    }
}
