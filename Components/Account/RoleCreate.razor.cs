using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;

namespace BlazorAuth.Components.Account;

public partial class RoleCreate : ComponentBase
{
    [Inject] private RoleManager<IdentityRole> RoleManager { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Parameter] public string? RoleId { get; set; }
    private RoleModel roleModel = new();
    private string? statusMessage;
    private bool IsEditMode => !string.IsNullOrEmpty(RoleId);
    private IdentityRole? loadedRole;
    private List<Claim> roleClaims = new();
    private ClaimModel newClaim = new();
    private bool showAdvancedOptions = false;
    private void ToggleAdvancedOptions() => showAdvancedOptions = !showAdvancedOptions;
    protected override async Task OnInitializedAsync()
    {
        if (IsEditMode)
        {
            loadedRole = await RoleManager.FindByIdAsync(RoleId);
            if (loadedRole == null)
            {
                statusMessage = "Role not found.";
                return;
            }
            roleModel.Name = loadedRole.Name ?? string.Empty;
            await LoadClaimsAsync();
        }
    }
    private async Task LoadClaimsAsync()
    {
        if (loadedRole != null)
        {
            roleClaims = (await RoleManager.GetClaimsAsync(loadedRole)).ToList();
        }
        StateHasChanged();
    }
    private Task AddClaimAsync()
    {
        if (string.IsNullOrWhiteSpace(newClaim.Type) || string.IsNullOrWhiteSpace(newClaim.Value))
        {
            statusMessage = "Claim type and value are required.";
            return Task.CompletedTask;
        }
        if (roleClaims.Any(c => c.Type == newClaim.Type && c.Value == newClaim.Value))
        {
            statusMessage = "This claim already exists.";
            return Task.CompletedTask;
        }
        roleClaims.Add(new Claim(newClaim.Type, newClaim.Value));
        newClaim = new();
        statusMessage = "Claim added.";
        StateHasChanged();
        return Task.CompletedTask;
    }
    private Task RemoveClaimAsync(Claim claim)
    {
        roleClaims.Remove(claim);
        statusMessage = "Claim removed.";
        StateHasChanged();
        return Task.CompletedTask;
    }
    private async Task HandleSubmit()
    {
        if (string.IsNullOrWhiteSpace(roleModel.Name))
        {
            statusMessage = "Role name cannot be empty.";
            return;
        }
        if (IsEditMode)
        {
            if (loadedRole == null)
            {
                statusMessage = "Role not found.";
                return;
            }
            loadedRole.Name = roleModel.Name;
            var result = await RoleManager.UpdateAsync(loadedRole);
            if (result.Succeeded)
            {
                var existingClaims = await RoleManager.GetClaimsAsync(loadedRole);
                foreach (var claim in existingClaims)
                {
                    await RoleManager.RemoveClaimAsync(loadedRole, claim);
                }
                foreach (var claim in roleClaims)
                {
                    await RoleManager.AddClaimAsync(loadedRole, claim);
                }
                NavigationManager.NavigateTo("/account/roles");
            }
            else
            {
                statusMessage = string.Join("; ", result.Errors.Select(e => e.Description));
            }
        }
        else
        {
            var newRole = new IdentityRole(roleModel.Name);
            var result = await RoleManager.CreateAsync(newRole);
            if (result.Succeeded)
            {
                foreach (var claim in roleClaims)
                {
                    await RoleManager.AddClaimAsync(newRole, claim);
                }
                NavigationManager.NavigateTo("/account/roles");
            }
            else
            {
                statusMessage = string.Join("; ", result.Errors.Select(e => e.Description));
            }
        }
    }
    public class RoleModel
    {
        [Required]
        public string Name { get; set; } = string.Empty;
    }
    public class ClaimModel
    {
        [Required]
        public string Type { get; set; } = string.Empty;
        [Required]
        public string Value { get; set; } = string.Empty;
    }
}
