using BlazorAuth.Components;
using BlazorAuth.Config;
using BlazorAuth.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddIdentityAndDb(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>();
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapRazorPages();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/api/account/login", async ([FromForm] LoginRequest loginRequest, SignInManager<IdentityUser> signInManager, HttpContext context) =>
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

app.MapPost("/account/loginwith2fa", async (
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
app.MapGet("/account/externalLoginLink", async (
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

app.MapGet("/account/externalLoginLink/callback", async (
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

app.Run();
public class TwoFactorLoginRequest
{
    public string TwoFactorCode { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
    public bool RememberMachine { get; set; }
    public string? ReturnUrl { get; set; }
}
