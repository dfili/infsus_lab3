using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using tamb.Models;
using Xunit;

namespace UnitTests.ModelTests
{
    public class InstrumentTest
    {
        [Fact]
        public void Instrument_CanBeCreated_WithDefaults()
        {
            var instrument = new Instrument
            {
                Name = "Prim",
                Type = "Tambura",
                Manufacturer = "Katulić",
                Year = 2000,
                Condition = "Good"
            };

            Assert.Equal("Prim", instrument.Name);
            Assert.Equal("Tambura", instrument.Type);
            Assert.Empty(instrument.Reservations);
        }

        [Fact]
        public void Instrument_Name_IsRequired()
        {
            var instrument = new Instrument
            {
                Name = "",
                Type = "Tambura"
            };

            var context = new ValidationContext(instrument, null, null);
            var results = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(instrument, context, results, true);

            Assert.Contains(results, r => r.MemberNames.Contains("Name"));
        }
    }
}
