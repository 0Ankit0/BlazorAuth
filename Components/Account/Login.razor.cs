using Microsoft.AspNetCore.Components;
using System;

namespace BlazorAuth.Components.Account
{
    public partial class Login : ComponentBase
    {
        private string? statusMessage;
        private bool _showPassword = false;

        [Inject] private NavigationManager NavigationManager { get; set; } = default!;

        private void TogglePassword()
        {
            _showPassword = !_showPassword;
        }

        protected override void OnInitialized()
        {
            var uri = new Uri(NavigationManager.Uri);
            var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
            if (query.TryGetValue("error", out var msg))
            {
                statusMessage = msg;
            }
        }
    }
}
