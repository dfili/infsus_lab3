using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using tamb.Controllers;
using tamb.Data;
using tamb.Models;
using Xunit;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Microsoft.Extensions.Logging;

namespace UnitTests.IntegrationTests
{
    public class ReservationIntegrationTest
    {
        [Fact]
        public async Task CreateAndFetchReservation_IntegrationTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "ReservationTestDB")
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                var instrument = new Instrument { Name = "Prim", Type = "Tambura" };
                var person = new Person{ ImePrezime = "Marko Markić" };

                context.Instruments.Add(instrument);
                context.Persons.Add(person);
                await context.SaveChangesAsync();

                var reservation = new Reservation
                {
                    InstrumentId = instrument.Id,
                    ReservedById = person.Id,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(3),
                    Status = "Pending"
                };

                context.Reservations.Add(reservation);
                await context.SaveChangesAsync();

                var mockLogger = new Mock<ILogger<ReservationController>>();
                var controller = new ReservationController(context, mockLogger.Object);

                var result = await controller.Index(null) as ViewResult;

                var model = Assert.IsAssignableFrom<List<Reservation>>(result?.ViewData.Model);
                Assert.Single(model);
                Assert.Equal("Marko Markić", model.First().ReservedBy?.ImePrezime);
                Assert.Equal("Prim", model.First().Instrument?.Name);
            }
        }
    }
}
