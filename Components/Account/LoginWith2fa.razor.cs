using Microsoft.AspNetCore.Components;

namespace BlazorAuth.Components.Account
{
    public partial class LoginWith2fa : ComponentBase
    {
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;

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
