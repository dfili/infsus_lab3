using Microsoft.EntityFrameworkCore;
using tamb.Data;
using tamb.Models;
using Xunit;

namespace UnitTests.DataTests
{
    public class ApplicationDbContextTest
    {
        [Fact]
        public void CanCreateDbContext_WithInMemoryDb()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb1")
                .Options;

            using var context = new ApplicationDbContext(options);

            Assert.NotNull(context);
            Assert.NotNull(context.Instruments);
            Assert.NotNull(context.Reservations);
            Assert.NotNull(context.Persons);
            Assert.NotNull(context.SheetMusic);
        }

        [Fact]
        public void CanAddAndRetrieveInstrument()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("TestDb2")
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                context.Instruments.Add(new Instrument { Name = "Prim", Type = "Tambura", Manufacturer = "Katulić" });
                context.SaveChanges();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var instrument = context.Instruments.FirstOrDefault();
                Assert.NotNull(instrument);
                Assert.Equal("Prim", instrument!.Name);
            }
        }
    }
}
