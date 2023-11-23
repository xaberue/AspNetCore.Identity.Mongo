﻿using AspNetCore.Identity.Mongo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Policy;
using SampleSite.Identity;
using SampleSite.Mailing;

namespace TestSite;

public class Startup
{
    private string ConnectionString => Configuration.GetConnectionString("DefaultConnection");

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddIdentityMongoDbProvider<TestSiteUser>(identity =>
            {
                identity.Password.RequireDigit = false;
                identity.Password.RequireLowercase = false;
                identity.Password.RequireNonAlphanumeric = false;
                identity.Password.RequireUppercase = false;
                identity.Password.RequiredLength = 1;
                identity.Password.RequiredUniqueChars = 0;
            } ,
            mongo =>
            {
                mongo.ConnectionString = ConnectionString;
            }
        );

        services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();
        services.AddSingleton<IAuthorizationHandler, HasClaimHandler>();

        services.AddSingleton<IEmailSender, EmailSender>();
        services.AddRazorPages();
    }
    
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseDeveloperExceptionPage();

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=User}/{action=Index}/{id?}");
            endpoints.MapRazorPages();
        });
    }
}
