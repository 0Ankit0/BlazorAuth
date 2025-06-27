using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using BlazorAuth.Models;

namespace BlazorAuth.Components.Account;

public partial class RoleAssign : ComponentBase
{
    [Parameter] public string RoleId { get; set; } = string.Empty;
    [Inject] private RoleManager<IdentityRole> RoleManager { get; set; } = default!;
    [Inject] private UserManager<IdentityUser> UserManager { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    private IdentityRole? role;
    private string? statusMessage;
    private List<IdentityUser> usersInRole = new();
    private AssignUserModel assignUserModel = new();
    protected override async Task OnInitializedAsync()
    {
        role = await RoleManager.FindByIdAsync(RoleId);
        if (role != null)
        {
            usersInRole = (await UserManager.GetUsersInRoleAsync(role.Name)).ToList();
        }
    }
    private async Task AssignUser()
    {
        if (role == null) return;
        var users = UserManager.Users.Where(u => u.Email == assignUserModel.UserEmail).ToList();
        if (users.Count == 0)
        {
            statusMessage = "User not found.";
            return;
        }
        if (users.Count > 1)
        {
            statusMessage = "Multiple users found with this email. Please resolve duplicates in the user database.";
            return;
        }
        var user = users[0];
        var result = await UserManager.AddToRoleAsync(user, role.Name);
        if (result.Succeeded)
        {
            statusMessage = $"User '{assignUserModel.UserEmail}' assigned to role '{role.Name}'.";
            usersInRole = (await UserManager.GetUsersInRoleAsync(role.Name)).ToList();
            assignUserModel.UserEmail = string.Empty;
        }
        else
        {
            statusMessage = string.Join("; ", result.Errors.Select(e => e.Description));
        }
    }
    private async Task RemoveUser(string userId)
    {
        if (role == null) return;
        var user = await UserManager.FindByIdAsync(userId);
        if (user == null) return;
        var result = await UserManager.RemoveFromRoleAsync(user, role.Name);
        if (result.Succeeded)
        {
            statusMessage = $"User removed from role.";
            usersInRole = (await UserManager.GetUsersInRoleAsync(role.Name)).ToList();
        }
        else
        {
            statusMessage = string.Join("; ", result.Errors.Select(e => e.Description));
        }
    }
}
