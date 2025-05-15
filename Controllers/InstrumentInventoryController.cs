using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using Tamb.Models; // Make sure to include your models namespace

namespace Tamb.Controllers
{
    // Controller to handle inventory-related requests
    public class InstrumentInventoryController : Controller
    {
        // --- In-memory Data Store (Replace with Database Integration) ---
        private static List<Instrument> _instruments = new List<Instrument>
        {
            new Instrument { Id = 1, Name = "Acoustic Guitar", Type = "String", Manufacturer = "Fender", Model = "Stratocaster", Year = 2020, Condition = "Excellent", Status = "Available", Notes = "Classic acoustic sound."},
            new Instrument { Id = 2, Name = "Digital Piano", Type = "Keyboard", Manufacturer = "Yamaha", Model = "P-45", Year = 2021, Condition = "Good", Status = "Available", Notes = "88 weighted keys."},
            new Instrument { Id = 3, Name = "Drum Kit", Type = "Percussion", Manufacturer = "Pearl", Model = "Export", Year = 2018, Condition = "Fair", Status = "Loaned", Notes = "Full 5-piece kit.", },
            new Instrument { Id = 4, Name = "Violin", Type = "String", Manufacturer = "Stentor", Model = "Student I", Year = 2022, Condition = "Excellent", Status = "Available", Notes = "Beginner violin."},
            new Instrument { Id = 5, Name = "Bass Guitar", Type = "String", Manufacturer = "Ibanez", Model = "SR300", Year = 2019, Condition = "Good", Status = "Available", Notes = "Active pickups."}
        };
        // --------------------------------------------------------------

        // Action method for the Inventory Index page
        // Handles displaying the list of instruments and search
        public IActionResult Index(string searchString)
        {
            // Get all instruments
            var instruments = from i in _instruments
                              select i;

            // Apply search filter if a search string is provided
            if (!string.IsNullOrEmpty(searchString))
            {
                instruments = instruments.Where(s => s.Name.Contains(searchString) || s.Type.Contains(searchString) || s.Manufacturer.Contains(searchString));
            }

            // Pass the filtered list of instruments to the view
            return View(instruments.ToList());
        }

        // Action method for the Instrument Details page
        // Displays specifics of a single instrument
        public IActionResult Details(int? id) // Nullable int for the ID
        {
            // If no ID is provided, return a Not Found result
            if (id == null)
            {
                return NotFound();
            }

            // Find the instrument by ID in the in-memory list
            var instrument = _instruments.FirstOrDefault(i => i.Id == id);

            // If the instrument is not found, return a Not Found result
            if (instrument == null)
            {
                return NotFound();
            }

            // Pass the found instrument object to the view
            return View(instrument);
        }

        // --- Placeholder for Reservation Action ---
        // This action would handle the reservation logic (e.g., updating status, saving to database)
        [HttpPost] // This action should be called via a POST request (e.g., from a form)
        public IActionResult Reserve(int instrumentId)
        {
            // In a real application:
            // 1. Find the instrument by instrumentId.
            // 2. Check if it's available.
            // 3. Create a new Reservation object.
            // 4. Save the reservation to the database.
            // 5. Update the instrument status (e.g., to "Reserved").
            // 6. Redirect the user to a confirmation page or back to the details page.

            // For this example, we'll just simulate a successful reservation and redirect
            // In a real app, add error handling and proper logic.

            var instrument = _instruments.FirstOrDefault(i => i.Id == instrumentId);
            if (instrument != null && instrument.Status == "Available")
            {
                instrument.Status = "Reserved"; // Simple status update
                // Add logic to create and save Reservation object here
                ViewData["ReservationStatus"] = "Success"; // Pass success message to view (optional)
            }
            else
            {
                 ViewData["ReservationStatus"] = "Failed"; // Pass failure message (optional)
            }


            // Redirect back to the details page for the reserved instrument
            return RedirectToAction("Details", new { id = instrumentId });
        }
        // -----------------------------------------
    }
}
