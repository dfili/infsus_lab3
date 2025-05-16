using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using tamb.Models; // Make sure to include your models namespace
using tamb.Data; // Include your data context namespace

namespace tamb.Controllers
{
    
    // Controller to handle inventory-related requests
    public class InstrumentInventoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InstrumentInventoryController(ApplicationDbContext context)
        {
            _context = context;
        }
        // Action method for the Inventory Index page
        // Handles displaying the list of instruments and search
        public IActionResult Index(string searchString)
        {
            var instruments = _context.Instruments.AsQueryable(); // Start query from DbContext

            if (!string.IsNullOrEmpty(searchString))
            {
                instruments = instruments.Where(s => s.Name.Contains(searchString) || s.Type.Contains(searchString) || s.Manufacturer.Contains(searchString));
            }

            return View(instruments.ToList()); // Execute the query and pass to view
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
            var instrument = _context.Instruments.AsQueryable().FirstOrDefault(i => i.Id == id);

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

            var instrument = _context.Instruments.AsQueryable().FirstOrDefault(i => i.Id == instrumentId);
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
