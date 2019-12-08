using LearnDotNetCore.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LearnDotNetCore
{
    public static class MyExtensions
    {
        public static IHost MigrateDatabase(this IHost webHost)
        {
            // Manually run any outstanding migrations if configured to do so
            var envAutoMigrate = Environment.GetEnvironmentVariable("AUTO_MIGRATE");
            if (envAutoMigrate != null && envAutoMigrate == "true")
            {
                Console.WriteLine("*** AUTO MIGRATE ***");

                var serviceScopeFactory = (IServiceScopeFactory)webHost.Services.GetService(typeof(IServiceScopeFactory));

                using (var scope = serviceScopeFactory.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    var dbContext = services.GetRequiredService<AppDbContext>();

                    dbContext.Database.Migrate();
                }
            }

            return webHost;
        }
    }
}
