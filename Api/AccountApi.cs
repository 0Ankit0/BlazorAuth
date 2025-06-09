using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BlazorAuth.Api;

public static class AccountApi
{
    public static IEndpointRouteBuilder MapAccountApi(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/account/login", async ([FromForm] LoginRequest loginRequest, SignInManager<IdentityUser> signInManager, HttpContext context) =>
        {
            var result = await signInManager.PasswordSignInAsync(loginRequest.Email, loginRequest.Password, false, false);
            if (result.Succeeded)
            {
                // Redirect to homepage
                context.Response.Redirect("/");
                return;
            }
            // Redirect back to login with error
            context.Response.Redirect("/account/login?error=Invalid%20login%20attempt");
        }).DisableAntiforgery();

        endpoints.MapPost("/account/loginwith2fa", async (
            [FromForm] TwoFactorLoginRequest model,
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            HttpContext httpContext) =>
        {
            // Ensure the user has gone through the username & password screen first
            var user = await signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return Results.BadRequest("Unable to load two-factor authentication user.");
            }

            var authenticatorCode = model.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

            var result = await signInManager.TwoFactorAuthenticatorSignInAsync(
                authenticatorCode, model.RememberMe, model.RememberMachine);

            if (result.Succeeded)
            {
                return Results.Ok(new { redirectUrl = model.ReturnUrl ?? "/" });
            }
            else if (result.IsLockedOut)
            {
                return Results.StatusCode(StatusCodes.Status423Locked); // 423 Locked
            }
            else
            {
                return Results.BadRequest("Invalid authenticator code.");
            }
        });
        endpoints.MapGet("/account/externalLoginLink", async (
            HttpContext httpContext,
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            [FromQuery] string provider,
            [FromQuery] string returnUrl) =>
        {
            // Clear the existing external cookie to ensure a clean login process
            await httpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            // Set up the callback URL for after the external provider returns
            var callbackUrl = $"/account/externalLoginLink/callback?returnUrl={Uri.EscapeDataString(returnUrl)}";
            var userId = userManager.GetUserId(httpContext.User);
            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, callbackUrl, userId);

            // Initiate the external login challenge
            return Results.Challenge(properties, new[] { provider });
        });

        endpoints.MapGet("/account/externalLoginLink/callback", async (
            HttpContext httpContext,
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            [FromQuery] string returnUrl) =>
        {
            var user = await userManager.GetUserAsync(httpContext.User);
            if (user == null)
            {
                return Results.Redirect($"{returnUrl}?statusMessage=User%20not%20found.");
            }

            var userId = await userManager.GetUserIdAsync(user);
            var info = await signInManager.GetExternalLoginInfoAsync(userId);
            if (info == null)
            {
                return Results.Redirect($"{returnUrl}?statusMessage=Unexpected%20error%20loading%20external%20login%20info.");
            }

            var result = await userManager.AddLoginAsync(user, info);
            if (!result.Succeeded)
            {
                return Results.Redirect($"{returnUrl}?statusMessage=The%20external%20login%20was%20not%20added.%20External%20logins%20can%20only%20be%20associated%20with%20one%20account.");
            }

            // Clear the existing external cookie to ensure a clean login process
            await httpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            return Results.Redirect($"{returnUrl}?statusMessage=The%20external%20login%20was%20added.");
        });

        endpoints.MapPost("/api/account/refreshsignin", async (
            HttpContext context,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager) =>
        {
            var user = await userManager.GetUserAsync(context.User);
            if (user == null)
            {
                return Results.Unauthorized();
            }

            await signInManager.RefreshSignInAsync(user);
            return Results.Ok(new { message = "Sign-in refreshed." });
        }).RequireAuthorization();

        endpoints.MapPost("/api/account/is2famachineremembered", async (
      [FromBody] IdentityUser user,
      UserManager<IdentityUser> userManager,
      SignInManager<IdentityUser> signInManager) =>
        {
            if (user == null)
                return Results.NotFound();

            var isRemembered = await signInManager.IsTwoFactorClientRememberedAsync(user);
            return Results.Ok(new MachineRememberedResponse { isRemembered = isRemembered });
        });

        return endpoints;
    }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
public class MachineRememberedResponse
{
    public bool isRemembered { get; set; }
}