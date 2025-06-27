using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using BlazorAuth.Models;

namespace BlazorAuth.Components.Account;

public partial class UserManager : ComponentBase
{
    [Inject] private UserManager<IdentityUser> userManager { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    private List<IdentityUser> users = new();
    private Dictionary<string, List<string>> userRoles = new();
    private string? statusMessage;
    private string? showDeleteConfirmId;
    private IdentityUser? userToDelete;
    private string? showResetPasswordId;
    private IdentityUser? userToReset;
    private string newPassword = string.Empty;
    private bool isLoading = true;
    private string? showManageClaimsId;
    private IdentityUser? userToManageClaims;
    private List<Claim> userClaims = new();
    private ClaimModel newUserClaim = new();
    protected override async Task OnInitializedAsync()
    {
        users = userManager.Users.ToList();
        userRoles = new Dictionary<string, List<string>>();
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            userRoles[user.Id] = roles.ToList();
        }
        isLoading = false;
    }
    private void ConfirmDelete(IdentityUser user)
    {
        showDeleteConfirmId = user.Id;
        userToDelete = user;
    }
    private void CancelDelete()
    {
        showDeleteConfirmId = null;
        userToDelete = null;
    }
    private async Task DeleteUser(IdentityUser user)
    {
        var result = await userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            statusMessage = $"User '{user.Email}' deleted.";
            users = userManager.Users.ToList();
            userRoles = new Dictionary<string, List<string>>();
            foreach (var u in users)
            {
                var roles = await userManager.GetRolesAsync(u);
                userRoles[u.Id] = roles.ToList();
            }
        }
        else
        {
            statusMessage = string.Join("; ", result.Errors.Select(e => e.Description));
        }
        showDeleteConfirmId = null;
        userToDelete = null;
    }
    private void ShowResetPassword(IdentityUser user)
    {
        showResetPasswordId = user.Id;
        userToReset = user;
        newPassword = string.Empty;
    }
    private void CancelResetPassword()
    {
        showResetPasswordId = null;
        userToReset = null;
        newPassword = string.Empty;
    }
    private async Task ResetPassword(IdentityUser user)
    {
        if (string.IsNullOrWhiteSpace(newPassword))
        {
            statusMessage = "Password cannot be empty.";
            return;
        }
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, token, newPassword);
        if (result.Succeeded)
        {
            statusMessage = $"Password for '{user.Email}' has been reset.";
        }
        else
        {
            statusMessage = string.Join("; ", result.Errors.Select(e => e.Description));
        }
        showResetPasswordId = null;
        userToReset = null;
        newPassword = string.Empty;
    }
    private async void ShowManageClaims(IdentityUser user)
    {
        showManageClaimsId = user.Id;
        userToManageClaims = user;
        newUserClaim = new();
        userClaims = (await userManager.GetClaimsAsync(user)).ToList();
        StateHasChanged();
    }
    private void CancelManageClaims()
    {
        showManageClaimsId = null;
        userToManageClaims = null;
        userClaims = new();
        newUserClaim = new();
    }
    private async Task AddUserClaimAsync()
    {
        if (userToManageClaims == null) return;
        if (string.IsNullOrWhiteSpace(newUserClaim.Type) || string.IsNullOrWhiteSpace(newUserClaim.Value))
        {
            statusMessage = "Claim type and value are required.";
            return;
        }
        if (userClaims.Any(c => c.Type == newUserClaim.Type && c.Value == newUserClaim.Value))
        {
            statusMessage = "This claim already exists for the user.";
            return;
        }
        var result = await userManager.AddClaimAsync(userToManageClaims, new Claim(newUserClaim.Type, newUserClaim.Value));
        if (result.Succeeded)
        {
            userClaims = (await userManager.GetClaimsAsync(userToManageClaims)).ToList();
            newUserClaim = new();
            statusMessage = "Claim added.";
        }
        else
        {
            statusMessage = string.Join("; ", result.Errors.Select(e => e.Description));
        }
    }
    private async Task RemoveUserClaimAsync(Claim claim)
    {
        if (userToManageClaims == null) return;
        var result = await userManager.RemoveClaimAsync(userToManageClaims, claim);
        if (result.Succeeded)
        {
            userClaims = (await userManager.GetClaimsAsync(userToManageClaims)).ToList();
            statusMessage = "Claim removed.";
        }
        else
        {
            statusMessage = string.Join("; ", result.Errors.Select(e => e.Description));
        }
    }
}
