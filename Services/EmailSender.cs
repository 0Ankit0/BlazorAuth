using Microsoft.AspNetCore.Identity;

namespace BlazorAuth.Services
{
    public class EmailSender :  IEmailSender<IdentityUser>
    {
        public Task SendConfirmationLinkAsync(IdentityUser user, string email, string confirmationLink)
        {
            Console.WriteLine($"Sending confirmation link to {email}: {confirmationLink}");
            return Task.CompletedTask;
        }
        public Task SendPasswordResetLinkAsync(IdentityUser user, string email, string resetLink)
        {
            Console.WriteLine($"Sending password reset link to {email}: {resetLink}");
            return Task.CompletedTask;
        }
        public Task SendPasswordResetCodeAsync(IdentityUser user, string email, string resetCode)
        {
           Console.WriteLine($"Sending password reset code to {email}: {resetCode}");
            return Task.CompletedTask;
        }
    }
}
