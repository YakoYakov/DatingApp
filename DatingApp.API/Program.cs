using System;
using DatingApp.API.Data;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DatingApp.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Only Build the host wait it`s execution until the seeding of the database is completed
           var host = CreateHostBuilder(args).Build();

           using (var scope = host.Services.CreateScope())
           {
               // Get the services from the scope
               IServiceProvider services = scope.ServiceProvider;

               try
               {
                   // Get the datacontext from the services
                   DataContext context = services.GetService<DataContext>();

                   // Get the UserManager from the services
                   UserManager<User> userManager = services.GetService<UserManager<User>>();

                   // Using Migrate() to ensure the database will bi created if it isn`t allready
                   // And apllying all migrations
                   context.Database.Migrate();

                   // Seed the test data
                   Seed.SeedUsers(userManager);
               }
               catch (Exception ex)
               {
                   ILogger<Program> logger = services.GetRequiredService<ILogger<Program>>();
                   logger.LogError(ex, "An error occured during migration");
               }

               // Finally Run the host
               host.Run();
           }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
