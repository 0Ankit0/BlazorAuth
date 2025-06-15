using BlazorAuth.Data;
using BlazorAuth.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace BlazorAuth.Config;
public static class IdentityConfig
{
    public static void AddIdentityAndDb(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AuthDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection") ?? "Host=localhost;Database=BlazorAuth;Username=postgres;Password=admin"));
       
        services.AddAuthentication()
         .AddBearerToken(IdentityConstants.BearerScheme);

        services.AddIdentity<IdentityUser, IdentityRole>(options =>
        {
            options.User.RequireUniqueEmail = true;
        })
           .AddEntityFrameworkStores<AuthDbContext>()
           .AddDefaultTokenProviders();

        services.AddScoped<IEmailSender<IdentityUser>, IdentityEmailSender>(); //for identity-specific email sending
        services.AddScoped<IEmailSender, GeneralEmailSender>(); //for general email sending
        services.AddScoped<ISmsSender, SmsSender>();

    }
}
