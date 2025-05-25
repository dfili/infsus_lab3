using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tamb.Controllers;
using tamb.Data;
using tamb.Models;
using Xunit;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;

namespace UnitTests
{
    public class SheetMusicInventoryControllerTest
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public SheetMusicInventoryControllerTest()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task Index_ReturnsAllSheetMusic_WhenNoSearchString()
        {
            using (var context = new ApplicationDbContext(_options))
            {
                context.SheetMusic.AddRange(
                    new SheetMusic { id = 1, Title = "Oda radosti", Composer = "Ludwig van Beethoven", Genre = "Klasična", Instrumentation = "Tambura", FileName = "oda_radosti.pdf" },
                    new SheetMusic { id = 2, Title = "Lijepa naša domovino", Composer = "Josip Runjanin", Genre = "Himna", Instrumentation = "Tambura", FileName = "lijepa_nasa.pdf" }
                );
                await context.SaveChangesAsync();

                var controller = new SheetMusicInventoryController(context);

                var result = await controller.Index(null) as ViewResult;
                Assert.NotNull(result);

                var model = Assert.IsAssignableFrom<List<SheetMusic>>(result.ViewData.Model);
                Assert.NotNull(model);

                Assert.Equal(2, model.Count);

            }
        }

        [Fact]
        public async Task Index_ReturnsFilteredSheetMusic_WhenSearchStringProvided()
        {
            using (var context = new ApplicationDbContext(_options))
            {
                context.SheetMusic.AddRange(
                    new SheetMusic { id = 1, Title = "Oda radosti", Composer = "Ludwig van Beethoven", Genre = "Klasična", Instrumentation = "Tambura", FileName = "oda_radosti.pdf" },
                    new SheetMusic { id = 2, Title = "Lijepa naša domovino", Composer = "Josip Runjanin", Genre = "Himna", Instrumentation = "Tambura", FileName = "lijepa_nasa.pdf" }
                );
                await context.SaveChangesAsync();

                var controller = new SheetMusicInventoryController(context);

                var result = await controller.Index("Oda") as ViewResult;
                Assert.NotNull(result);

                var model = Assert.IsAssignableFrom<List<SheetMusic>>(result.ViewData.Model);
                Assert.NotNull(model);

                Assert.Single(model);
                Assert.Equal("Oda radosti", model.First().Title);
            }
        }

        [Fact]
        public async Task Download_ReturnsNotFound_WhenIdIsNull()
        {
            using (var context = new ApplicationDbContext(_options))
            {
                var controller = new SheetMusicInventoryController(context);
                var result = await controller.Download(null);

                Assert.IsType<NotFoundResult>(result);
            }
        }

        [Fact]
        public async Task Download_ReturnsNotFound_WhenSheetMusicDoesNotExist()
        {
            using (var context = new ApplicationDbContext(_options))
            {
                var controller = new SheetMusicInventoryController(context);
                var result = await controller.Download(999); 

                Assert.IsType<NotFoundResult>(result);
            }
        }

        [Fact]
        public async Task Download_ReturnsFileContent_WhenSheetMusicExists()
        {
            using (var context = new ApplicationDbContext(_options))
            {
                var music = new SheetMusic
                {
                    id = 1,
                    Title = "Lijepa naša domovino",
                    Composer = "Josip Runjanin",
                    Genre = "Himna",
                    Instrumentation = "Tamburaški orkestar",
                    FileName = "lijepa_nasa.pdf"
                };
                context.SheetMusic.Add(music);
                await context.SaveChangesAsync();

                var controller = new SheetMusicInventoryController(context);
                var result = await controller.Download(1);

                var fileResult = Assert.IsType<FileContentResult>(result);
                Assert.Equal("application/pdf", fileResult.ContentType);
                Assert.Equal("lijepa_nasa.pdf", fileResult.FileDownloadName);
                Assert.Contains("Lijepa naša domovino", System.Text.Encoding.UTF8.GetString(fileResult.FileContents));

            }
        }
    }
}
