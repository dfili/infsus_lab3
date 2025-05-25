using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using tamb.Data;
using tamb.Models;
using Xunit;

namespace UnitTests.DataTests
{
    public class DbInitializerTest
    {
        private IServiceProvider BuildServiceProvider(string dbName)
        {
            var services = new ServiceCollection();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(dbName));

            return services.BuildServiceProvider();
        }

        [Fact]
        public void DbInitializer_SeedsDataCorrectly()
        {
            var serviceProvider = BuildServiceProvider("SeedTestDb");

            DbInitializer.Initialize(serviceProvider);

            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            Assert.True(context.Instruments.Any());
            Assert.True(context.Persons.Any());
            Assert.True(context.Reservations.Any());

            Assert.Equal(8, context.Instruments.Count());
            Assert.Equal(3, context.Persons.Count());
            Assert.Equal(3, context.Reservations.Count());
        }

        [Fact]
        public void DbInitializer_DoesNotSeedAgain_IfAlreadySeeded()
        {
            var serviceProvider = BuildServiceProvider("SeedOnceTestDb");

            DbInitializer.Initialize(serviceProvider);

            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                context.Instruments.Add(new Instrument { Name = "Test", Type = "Test", Manufacturer = "Test" });
                context.SaveChanges();
            }

            DbInitializer.Initialize(serviceProvider);

            using var secondScope = serviceProvider.CreateScope();
            var context2 = secondScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Assert.Equal(9, context2.Instruments.Count()); // 8 + 1
        }
    }
}
