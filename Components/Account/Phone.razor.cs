using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BlazorAuth.Models;

namespace BlazorAuth.Components.Account
{
    public partial class Phone : ComponentBase
    {
        [Parameter] public IdentityUser? User { get; set; }
        private string currentPhone = string.Empty;
        private bool isPhoneConfirmed;
        private string? statusMessage;
        private bool isLoaded = false;
        private bool showChangePhone = false;
        private PhoneInputModel phoneInputModel = new();

        [Inject] private UserManager<IdentityUser> UserManager { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;
        [Inject] private ISmsSender smsSender { get; set; } = default!;

        protected override async Task OnParametersSetAsync()
        {
            await LoadAsync();
        }

        private async Task LoadAsync()
        {
            if (User == null)
            {
                statusMessage = "User not found.";
                isLoaded = false;
                return;
            }
            currentPhone = await UserManager.GetPhoneNumberAsync(User) ?? "";
            phoneInputModel.NewPhone = currentPhone;
            isPhoneConfirmed = await UserManager.IsPhoneNumberConfirmedAsync(User);
            isLoaded = true;
        }

        private async Task HandleChangePhone()
        {
            statusMessage = string.Empty;
            if (User == null)
            {
                statusMessage = "User not found.";
                return;
            }

            var phone = await UserManager.GetPhoneNumberAsync(User);
            if (phoneInputModel.NewPhone != phone)
            {
                var userId = await UserManager.GetUserIdAsync(User);
                var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User, phoneInputModel.NewPhone);
                await smsSender.SendSmsAsync(phoneInputModel.NewPhone, $"Your confirmation code is: {code}");

                statusMessage = "Verification code sent to new phone number. Please check your SMS.";
                await Task.Delay(500);
                NavigationManager.NavigateTo($"/account/confirmphone?phoneNo={Uri.EscapeDataString(phoneInputModel.NewPhone)}");
            }
            else
            {
                statusMessage = "Your phone number is unchanged.";
            }
            showChangePhone = false;
            await LoadAsync();
        }

        private async Task SendVerificationSms()
        {
            statusMessage = string.Empty;
            if (User == null)
            {
                statusMessage = "User not found.";
                return;
            }

            var phone = await UserManager.GetPhoneNumberAsync(User);
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User, phone);
            await smsSender.SendSmsAsync(phone, $"Your confirmation code is: {code}");

            statusMessage = "Verification code sent. Please check your SMS.";
            await Task.Delay(500);
            NavigationManager.NavigateTo($"/account/confirmphone?phoneNo={Uri.EscapeDataString(phone)}");
        }
    }
}
