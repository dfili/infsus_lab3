using Microsoft.EntityFrameworkCore;
using tamb.Data;
using tamb.Models;
using tamb.Controllers;
using Xunit;
using Microsoft.AspNetCore.Mvc;


namespace UnitTests.IntegrationTests
{
    public class SheetMusicIntegrationTest
    {
        [Fact]
        public async Task Integration_Index_ReturnsViewWithData()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("TestDB")
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                context.SheetMusic.Add(new SheetMusic { id = 1, Title = "Test", FileName = "test.pdf" });
                await context.SaveChangesAsync();

                var controller = new SheetMusicInventoryController(context);

                var result = await controller.Index(null) as ViewResult;

                var model = Assert.IsAssignableFrom<List<SheetMusic>>(result.ViewData.Model);
                Assert.Single(model);
            }
        }
    }
}