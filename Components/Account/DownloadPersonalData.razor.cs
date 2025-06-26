using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlazorAuth.Components.Account
{
    public partial class DownloadPersonalData : ComponentBase
    {
        [Parameter] public IdentityUser? User { get; set; }
        private string? statusMessage;

        [Inject] private UserManager<IdentityUser> UserManager { get; set; } = default!;
        [Inject] private ILogger<DownloadPersonalData> Logger { get; set; } = default!;
        [Inject] private IJSRuntime JS { get; set; } = default!;

        private async Task DownloadData()
        {
            if (User == null)
            {
                statusMessage = "User not found.";
                return;
            }

            Logger.LogInformation("User with ID '{UserId}' asked for their personal data.", await UserManager.GetUserIdAsync(User));

            var personalData = new Dictionary<string, string?>();
            var personalDataProps = typeof(IdentityUser).GetProperties()
                .Where(prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)));
            foreach (var p in personalDataProps)
            {
                personalData.Add(p.Name, p.GetValue(User)?.ToString() ?? "null");
            }

            var logins = await UserManager.GetLoginsAsync(User);
            foreach (var l in logins)
            {
                personalData.Add($"{l.LoginProvider} external login provider key", l.ProviderKey);
            }

            var authenticatorKey = await UserManager.GetAuthenticatorKeyAsync(User);
            personalData.Add("Authenticator Key", authenticatorKey ?? "null");

            var json = JsonSerializer.Serialize(personalData, new JsonSerializerOptions { WriteIndented = true });
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            await JS.InvokeVoidAsync("downloadFileFromBytes", "PersonalData.json", "application/json", bytes);

            statusMessage = "Personal data downloaded.";
        }
    }
}
