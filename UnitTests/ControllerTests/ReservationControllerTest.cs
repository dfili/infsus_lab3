using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using tamb.Controllers;
using tamb.Data;
using tamb.Models;
using Xunit;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Moq;
using System;

namespace UnitTests
{
    public class ReservationControllerTest
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;

        public ReservationControllerTest()
        {
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WithReservations()
        {
            using (var context = new ApplicationDbContext(_dbOptions))
            {
                context.Instruments.Add(new Instrument { Id = 1, Name = "Harmonika" });
                context.Persons.Add(new Person { Id = 1, ImePrezime = "Pero Perić" });
                context.Reservations.Add(new Reservation
                {
                    InstrumentId = 1,
                    ReservedById = 1,
                    StartDate = DateTime.UtcNow,
                    Status = "Confirmed"
                });
                await context.SaveChangesAsync();

                var loggerMock = new Mock<ILogger<ReservationController>>();
                var controller = new ReservationController(context, loggerMock.Object);

                var result = await controller.Index(null);

                Assert.IsType<ViewResult>(result);
            }
        }

        [Fact]
        public async Task Create_Post_InvalidDateOrder_ReturnsModelError()
        {
            using (var context = new ApplicationDbContext(_dbOptions))
            {
                context.Instruments.Add(new Instrument { Id = 1, Name = "Harmonika" });
                context.Persons.Add(new Person { Id = 1, ImePrezime = "Pero Perić" });
                await context.SaveChangesAsync();

                var loggerMock = new Mock<ILogger<ReservationController>>();
                var controller = new ReservationController(context, loggerMock.Object);
                var reservation = new Reservation
                {
                    InstrumentId = 1,
                    ReservedById = 1,
                    StartDate = DateTime.UtcNow.AddDays(1),
                    EndDate = DateTime.UtcNow,
                    Status = "Confirmed"
                };

                var result = await controller.Create(reservation);

                Assert.False(controller.ModelState.IsValid);
            }
        }
    }
}
