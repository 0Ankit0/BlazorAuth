using Microsoft.AspNetCore.Components;

namespace BlazorAuth.Components.Account
{
    public partial class LoginWithRecoveryCode : ComponentBase
    {
        private string? statusMessage;

        [Inject] private NavigationManager NavigationManager { get; set; } = default!;

        protected override void OnInitialized()
        {
            var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
            var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
            if (query.TryGetValue("message", out var msg))
            {
                statusMessage = msg;
            }
        }

        private string GetQuery(string key, string defaultValue = "")
        {
            var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
            var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
            if (query.TryGetValue(key, out var value))
            {
                return value;
            }
            return defaultValue;
        }
    }
}
