using System;
using DatingApp.API.Data;
using Microsoft.AspNetCore.Hosting;
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

                   // Using Migrate() to ensure the database will bi created if it isn`t allready
                   // And apllying all migrations
                   context.Database.Migrate();

                   // Seed the test data
                   Seed.SeedUsers(context);
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
