using tamb.Controllers;
using tamb.Models;
using tamb.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace UnitTests
{
    public class InstrumentInventoryControllerTest
    {
        private InstrumentInventoryController GetControllerWithData()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var context = new ApplicationDbContext(options);

            context.Instruments.Add(new Instrument { Id = 1, Name = "Prim", Type = "Tambura", Manufacturer = "Katulić" });
            context.Instruments.Add(new Instrument { Id = 2, Name = "Kontra", Type = "Tambura", Manufacturer = "Nepoznato" });
            context.SaveChanges();

            return new InstrumentInventoryController(context);
        }

        [Fact]
        public async Task Index_Returns_View_With_Instruments()
        {
            var controller = GetControllerWithData();

            var result = await controller.Index(null);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Instrument>>(viewResult.Model);
            Assert.Equal(2, model.Count());
        }

        [Fact]
        public async Task Create_ValidInstrument_RedirectsToIndex()
        {
            var controller = GetControllerWithData();
            var newInstrument = new Instrument
            {
                Name = "Harmonika",
                Type = "Harmonika",
                Manufacturer = "Pigini",
                Model = "PRELUDIO P 30",
                Year = 2023,
                Condition = "Excellent",
                Notes = ""
            };

            var result = await controller.Create(newInstrument);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async Task Edit_Returns_ViewResult_WithInstrument()
        {
            var controller = GetControllerWithData();
            var result = await controller.Edit(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<Instrument>(viewResult.Model);
        }

        [Fact]
        public async Task Details_NonExistentId_Returns_NotFound()
        {
            var controller = GetControllerWithData();
            var result = await controller.Details(999);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteConfirmed_RemovesInstrument_AndRedirects()
        {
            var controller = GetControllerWithData();

            var result = await controller.DeleteConfirmed(1);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public void Reserve_RedirectsToCreateReservation()
        {
            var controller = GetControllerWithData();
            var result = controller.Reserve(1);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Create", redirect.ActionName);
            Assert.Equal("Reservation", redirect.ControllerName);
        }
    }
}
