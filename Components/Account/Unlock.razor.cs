using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace BlazorAuth.Components.Account;

public partial class Unlock : ComponentBase
{
    [Inject] private UserManager<IdentityUser> UserManager { get; set; } = default!;
    private string? Message;
    private bool IsLoading = false;
    [SupplyParameterFromQuery]
    public string? Email { get; set; }
    [SupplyParameterFromQuery(Name = "code")]
    public string? EncodedCode { get; set; }
    private bool HasParams => !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(EncodedCode);
    protected override async Task OnParametersSetAsync()
    {
        if (HasParams)
        {
            await RemoveLockout();
        }
    }
    private async Task RemoveLockout()
    {
        IsLoading = true;
        Message = string.Empty;
        try
        {
            var user = await UserManager.FindByEmailAsync(Email!);
            if (user == null)
            {
                Message = "Invalid unlock link or user not found.";
                return;
            }
            var codeBytes = WebEncoders.Base64UrlDecode(EncodedCode!);
            var code = Encoding.UTF8.GetString(codeBytes);
            var isValid = await UserManager.VerifyUserTokenAsync(
                user, TokenOptions.DefaultProvider, "RemoveLockout", code);
            if (!isValid)
            {
                Message = "Failed to remove lockout. The link may be invalid or expired.";
                return;
            }
            await UserManager.SetLockoutEndDateAsync(user, null);
            await UserManager.ResetAccessFailedCountAsync(user);
            Message = "Lockout removed. You may now try logging in again.";
        }
        catch
        {
            Message = "An error occurred while attempting to remove lockout.";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
