using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using tamb.Models; // Make sure to include your models namespace
using tamb.Data; // Required for file handling (for download simulation)

namespace tamb.Controllers
{
    // Controller to handle sheet music-related requests
    public class SheetMusicInventoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        public SheetMusicInventoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Action method for the Sheet Music Inventory Index page
        // Handles displaying the list of sheet music
        public async Task<IActionResult> Index(string searchString) // Made async to use async EF Core methods
        {
            // Start query from the DbContext's DbSet for SheetMusic
            var sheetMusic = _context.SheetMusic.AsQueryable();

            // Apply search filter if a search string is provided
            if (!string.IsNullOrEmpty(searchString))
            {
                // Filter by Title, Composer, Instrumentation, or Genre
                sheetMusic = sheetMusic.Where(s => s.Title.Contains(searchString)
                                                   || s.Composer.Contains(searchString)
                                                   || s.Instrumentation.Contains(searchString)
                                                   || s.Genre.Contains(searchString));
            }

            // Pass the filtered list of sheet music to the view after executing the query asynchronously
            return View(await sheetMusic.ToListAsync()); // Use ToListAsync for async operation
        }

        // Action method to simulate downloading a sheet music file
        // This remains similar to the previous version, but would ideally use the DbContext
        // to retrieve the file path or identifier from the database.
        public async Task<IActionResult> Download(int? id) // Made async
        {
            // If no ID is provided, return a Not Found result
            if (id == null)
            {
                return NotFound();
            }


            // Find the sheet music by ID using the DbContext asynchronously
            var music = await _context.SheetMusic.FirstOrDefaultAsync(m => m.id == id);

            // If the sheet music is not found, return a Not Found result
            if (music == null)
            {
                return NotFound();
            }

            // --- Simulate File Download ---
            // In a real application, you would retrieve the actual file content
            // from storage (e.g., file system, cloud storage) based on music.FileName
            // and return a FileResult.

            // For this example, we'll just return a dummy file content.
            // Replace this with actual file retrieval logic.
            var dummyFileContent = System.Text.Encoding.UTF8.GetBytes($"This is dummy content for {music.Title} ({music.FileName}). In a real app, this would be the PDF/image data.");
            var contentType = "application/pdf"; // Assuming PDF, change based on actual file type
            var fileDownloadName = music.FileName; // Use the stored file name

            // Return the file content as a FileContentResult
            return File(dummyFileContent, contentType, fileDownloadName);
        }
    }
}
