using System;
using tamb.Models;
using Xunit;

namespace UnitTests.ModelTests
{
    public class ReservationTest
    {
        [Fact]
        public void Reservation_Properties_AssignCorrectly()
        {
            var reservation = new Reservation
            {
                InstrumentId = 1,
                ReservedById = 2,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(3),
                Status = "Confirmed"
            };

            Assert.Equal(1, reservation.InstrumentId);
            Assert.Equal(2, reservation.ReservedById);
            Assert.Equal("Confirmed", reservation.Status);
        }
    }
}
