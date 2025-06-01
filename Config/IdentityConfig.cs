using BlazorAuth.Data;
using BlazorAuth.Services;
using Microsoft.AspNetCore.Identity;
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

        services.AddIdentity<IdentityUser, IdentityRole>()
           .AddEntityFrameworkStores<AuthDbContext>()
           .AddDefaultTokenProviders();

        services.AddScoped<IEmailSender<IdentityUser>, EmailSender>();
    }
}
