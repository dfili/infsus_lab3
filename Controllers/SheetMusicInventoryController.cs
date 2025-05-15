using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using Tamb.Models; // Make sure to include your models namespace
using System.IO; // Required for file handling (for download simulation)

namespace Tamb.Controllers
{
    // Controller to handle sheet music-related requests
    public class SheetMusicInventoryController : Controller
    {
        // --- In-memory Data Store (Replace with Database Integration) ---
        private static List<SheetMusic> _sheetMusic = new List<SheetMusic>
        {
            new SheetMusic { Id = 1, Title = "Für Elise", Composer = "Ludwig van Beethoven", Instrumentation = "Piano", Genre = "Classical", FileName = "fur_elise.pdf", DateAdded = DateTime.UtcNow.AddDays(-30), Notes = "Popular piano piece." },
            new SheetMusic { Id = 2, Title = "Take Five", Composer = "Paul Desmond", Arranger = "Dave Brubeck Quartet", Instrumentation = "Alto Saxophone, Piano, Bass, Drums", Genre = "Jazz", FileName = "take_five.pdf", DateAdded = DateTime.UtcNow.AddDays(-15), Notes = "Famous jazz standard." },
            new SheetMusic { Id = 3, Title = "Bohemian Rhapsody", Composer = "Freddie Mercury", Instrumentation = "Piano, Vocals", Genre = "Rock", FileName = "bohemian_rhapsody_piano.pdf", DateAdded = DateTime.UtcNow.AddDays(-5), Notes = "Piano/Vocal arrangement." }
        };
        // --------------------------------------------------------------

        // Action method for the Sheet Music Inventory Index page
        // Handles displaying the list of sheet music
        public IActionResult Index()
        {
            // Pass the list of sheet music to the view
            return View(_sheetMusic.ToList());
        }

        // Action method to simulate downloading a sheet music file
        public IActionResult Download(int? id) // Nullable int for the ID
        {
            // If no ID is provided, return a Not Found result
            if (id == null)
            {
                return NotFound();
            }

            // Find the sheet music by ID in the in-memory list
            var music = _sheetMusic.FirstOrDefault(m => m.Id == id);

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
