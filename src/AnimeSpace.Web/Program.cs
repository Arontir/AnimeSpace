using AnimeSpace.Application;
using AnimeSpace.Domain.Entities;
using AnimeSpace.Infrastructure;
using AnimeSpace.Infrastructure.Persistence.Context;
using AnimeSpace.Infrastructure.Persistence.Seed;
using AnimeSpace.Web.Extensions;
using AnimeSpace.Web.Filters;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace AnimeSpace.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateBootstrapLogger();

            try
            {
                var builder = WebApplication.CreateBuilder(args);

                builder.Host.UseSerilog((context, services, configuration) => configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .WriteTo.File(
                        path: Path.Combine(context.HostingEnvironment.ContentRootPath, "logs", "animespace-.log"),
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 14,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"));

                // Add services to the container.
                builder.Services.AddInfrastructure(builder.Configuration, builder.Environment.WebRootPath);
                builder.Services.AddApplication();
                builder.Services.AddDatabaseDeveloperPageExceptionFilter();

                builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
                {
                    options.SignIn.RequireConfirmedAccount = false;
                    options.Password.RequireDigit = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequiredLength = 6;
                })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

                builder.Services.ConfigureApplicationCookie(options =>
                {
                    options.Events.OnRedirectToLogin = context =>
                    {
                        if (context.Request.IsAjax())
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        }
                        else
                        {
                            context.Response.Redirect(context.RedirectUri);
                        }
                        return Task.CompletedTask;
                    };

                    options.Events.OnRedirectToAccessDenied = context =>
                    {
                        if (context.Request.IsAjax())
                        {
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        }
                        else
                        {
                            context.Response.Redirect(context.RedirectUri);
                        }
                        return Task.CompletedTask;
                    };
                });

                builder.Services.AddAntiforgery(o => o.HeaderName = "RequestVerificationToken");

                builder.Services.AddControllersWithViews(options =>
                {
                    options.Filters.Add<GlobalExceptionFilter>();

                    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
                });

                var app = builder.Build();

                using (var scope = app.Services.CreateScope())
                {
                    await DbInitializer.InitializeAsync(scope.ServiceProvider);
                }

                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.UseMigrationsEndPoint();
                }
                else
                {
                    app.UseExceptionHandler("/Home/Error");
                    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                    app.UseHsts();
                }

                app.UseHttpsRedirection();

                app.UseStaticFiles();
                app.UseRouting();

                app.UseAuthentication();
                app.UseAuthorization();

                app.MapStaticAssets();

                app.MapControllerRoute(
                    name: "areas",
                    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

                app.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}")
                    .WithStaticAssets();

                app.MapRazorPages()
                   .WithStaticAssets();

                app.Run();
            }
            catch (Exception ex) when (ex is not HostAbortedException)
            {
                Log.Fatal(ex, "Застосунок несподівано зупинився під час запуску.");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
