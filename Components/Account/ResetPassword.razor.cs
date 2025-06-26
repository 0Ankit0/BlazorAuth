using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace BlazorAuth.Components.Account;

public partial class ResetPassword : ComponentBase
{
    [Inject] private UserManager<IdentityUser> UserManager { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    private ResetModel resetModel = new();
    private string? statusMessage;
    [CascadingParameter] private Task<AuthenticationState>? authenticationStateTask { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
        var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
        if (query.TryGetValue("code", out var codeValue))
        {
            try
            {
                resetModel.Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(codeValue));
            }
            catch
            {
                statusMessage = "Invalid or missing reset code.";
            }
        }
        else
        {
            statusMessage = "A code must be supplied for password reset.";
        }
        if (authenticationStateTask != null)
        {
            var authState = await authenticationStateTask;
            var user = await UserManager.GetUserAsync(authState.User);
            if (user != null)
            {
                resetModel.Email = await UserManager.GetEmailAsync(user) ?? string.Empty;
            }
        }
    }

    private async Task HandleReset()
    {
        statusMessage = string.Empty;
        if (string.IsNullOrWhiteSpace(resetModel.Code))
        {
            statusMessage = "A code must be supplied for password reset.";
            return;
        }
        var context = new ValidationContext(resetModel);
        var results = new List<ValidationResult>();
        if (!Validator.TryValidateObject(resetModel, context, results, true))
        {
            statusMessage = string.Join(" ", results.Select(r => r.ErrorMessage));
            return;
        }
        var user = await UserManager.FindByEmailAsync(resetModel.Email);
        if (user == null)
        {
            NavigationManager.NavigateTo("/account/resetpasswordconfirmation");
            return;
        }
        var result = await UserManager.ResetPasswordAsync(user, resetModel.Code, resetModel.Password);
        if (result.Succeeded)
        {
            NavigationManager.NavigateTo("/account/resetpasswordconfirmation");
            return;
        }
        statusMessage = string.Join(" ", result.Errors.Select(e => e.Description));
    }

    public class ResetModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public string Code { get; set; } = string.Empty;
    }
}
