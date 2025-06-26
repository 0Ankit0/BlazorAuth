using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;

namespace BlazorAuth.Components.Account;

public partial class ResetPasswordConfirmation : ComponentBase
{
    [Inject] private SignInManager<IdentityUser> SignInManager { get; set; } = default!;
    protected override async Task OnInitializedAsync()
    {
        await SignInManager.SignOutAsync();
    }
}
