using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Hackathon.Data;
using Hackathon.Areas.Identity.Data;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Hackathon.Settings;
using SendGrid.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity.UI.Services;
using Hackathon.Services;
// � ������ ConfigureServices

namespace Hackathon
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var connectionString = builder.Configuration.GetConnectionString("HackathonContextConnection") ?? throw new InvalidOperationException("Connection string 'HackathonContextConnection' not found.");

            builder.Services.AddDbContext<HackathonContext>(options => options.UseSqlite(connectionString));

            builder.Services.AddDefaultIdentity<HackathonUser>(options => options.SignIn.RequireConfirmedAccount = true).AddRoles<IdentityRole>().AddEntityFrameworkStores<HackathonContext>();

            builder.Services.AddRazorPages();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API Name", Version = "v1" });
            });

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddAuthentication()
                .AddGoogle(GoogleOptions =>
                {
                    GoogleOptions.ClientId = builder.Configuration.GetSection("GoogleAuthSettings").GetValue<string>("ClientId");
                    GoogleOptions.ClientSecret = builder.Configuration.GetSection("GoogleAuthSettings").GetValue<string>("ClientSecret");
                });
            builder.Services.Configure<SendGridSettings>(builder.Configuration.GetSection("SendGridSettings"));
            builder.Services.AddSendGrid(options => {
                options.ApiKey = builder.Configuration.GetSection("SendGridSettings").GetValue<string>("ApiKey");
            });
            builder.Services.AddScoped<IEmailSender, EmailSenderService>();
            var app = builder.Build();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Api");
            });

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseAuthentication();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapRazorPages();

            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var roles = new[] { "Admin", "User" };

                foreach (var role in roles)
                {

                    if (!await roleManager.RoleExistsAsync(role))
                        await roleManager.CreateAsync(new IdentityRole(role));

                }
            }


            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}