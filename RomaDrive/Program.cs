using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Minio;
using RomaDrive.Data;
using RomaDrive.Models;
using Minio.DataModel.Args;
using System.Security.Policy;
using RomaDrive.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace RomaDrive
{
    public class Program
    {
        
        public static void Main(string[] args)
        {
            //var endpoint = "play.min.io";
            //var accessKey = "Q3AM3UQ867SPQQA43P2F";
            //var secretKey = "zuf+tfteSlswRu7BJ86wtrueekitnifILbZam1KYY3TG";

            //var endpoint = new Uri("http://localhost:9000");
            //var accessKey = "cuitPU5SrOI1vujnYQqc";
            //var secretKey = "jtWbOBLo5ZvESmPFsdbMWNXhNLSMVNK4hpE2loXt";

            var builder = WebApplication.CreateBuilder(args);

            

            // Add Minio using the custom endpoint and configure additional settings for default MinioClient initialization
            //builder.Services.AddMinio(configureClient => configureClient.WithEndpoint(endpoint).WithCredentials(accessKey, secretKey).WithSSL().Build());

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddTransient<IEmailSender, EmailSender>();
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

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

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            app.Run();
        }
    }
}
