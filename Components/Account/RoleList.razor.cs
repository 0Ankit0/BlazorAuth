using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;

namespace BlazorAuth.Components.Account
{
    partial class RoleList : ComponentBase
    {
    private List<IdentityRole> roles = new();
        private Dictionary<string, List<string>> roleUserNames = new();
        private string? statusMessage;
        private string? showDeleteConfirmId;
        private IdentityRole? roleToDelete;

        protected override async Task OnInitializedAsync()
        {
            roles = RoleManager.Roles.ToList();
            await LoadRoleUserNames();
        }

        private async Task LoadRoleUserNames()
        {
            roleUserNames = new Dictionary<string, List<string>>();
            var allUsers = UserManager.Users.ToList();
            foreach (var role in roles)
            {
                var userNames = new List<string>();
                foreach (var user in allUsers)
                {
                    var userRoles = await UserManager.GetRolesAsync(user);
                    if (userRoles.Contains(role.Name))
                    {
                        userNames.Add(user.UserName ?? user.Email ?? user.Id);
                    }
                }
                roleUserNames[role.Id] = userNames;
            }
        }

        private void ConfirmDelete(IdentityRole role)
        {
            showDeleteConfirmId = role.Id;
            roleToDelete = role;
        }

        private void CancelDelete()
        {
            showDeleteConfirmId = null;
            roleToDelete = null;
        }

        private async Task DeleteRole(IdentityRole role)
        {
            if (roleUserNames.TryGetValue(role.Id, out var users) && users.Count > 0)
            {
                statusMessage = $"Cannot delete role '{role.Name}' because it is assigned to {users.Count} user(s).";
            }
            else
            {
                var result = await RoleManager.DeleteAsync(role);
                if (result.Succeeded)
                {
                    statusMessage = $"Role '{role.Name}' deleted.";
                    roles = RoleManager.Roles.ToList();
                    await LoadRoleUserNames();
                }
                else
                {
                    statusMessage = string.Join("; ", result.Errors.Select(e => e.Description));
                }
            }
            showDeleteConfirmId = null;
            roleToDelete = null;
        }
    }

}
