using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
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

        // GET: Inventory/Create
        // Displays the form to create a new instrument
        public IActionResult Create()
        {
            return View();
        }

        // POST: Inventory/Create
        // Handles the form submission to add a new instrument to the database
        [HttpPost]
        [ValidateAntiForgeryToken] // Helps prevent Cross-Site Request Forgery attacks
        public async Task<IActionResult> Create([Bind("Name,Type,Manufacturer,Model,Year,Condition,Status,Notes,Price")] Instrument instrument) // Bind includes properties to protect against overposting
        {
            if (ModelState.IsValid) // Check if the submitted data is valid based on model annotations
            {
                // Add the new instrument to the DbContext
                _context.Add(instrument);
                // Save the changes to the database asynchronously
                await _context.SaveChangesAsync();
                // Redirect to the Index page after successful creation
                return RedirectToAction(nameof(Index));
            }
            // If model state is not valid, return the view with the entered data to show validation errors
            return View(instrument);
        }

        // --- Edit Actions ---

        // GET: Inventory/Edit/5
        // Displays the form to edit an existing instrument
        public async Task<IActionResult> Edit(int? id) // Nullable int for the ID
        {
            // If no ID is provided, return Not Found
            if (id == null)
            {
                return NotFound();
            }

            // Find the instrument by ID asynchronously
            var instrument = await _context.Instruments.FindAsync(id);

            // If the instrument is not found, return Not Found
            if (instrument == null)
            {
                return NotFound();
            }
            // Return the view with the instrument data for editing
            return View(instrument);
        }

        // POST: Inventory/Edit/5
        // Handles the form submission to update an existing instrument in the database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Type,Manufacturer,Model,Year,Condition,Status,Notes,Price")] Instrument instrument) // Include Id in Bind for updates
        {
            // Check if the ID in the route matches the ID in the submitted form data
            if (id != instrument.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid) // Check if the submitted data is valid
            {
                try
                {
                    // Update the instrument in the DbContext
                    _context.Update(instrument);
                    // Save the changes to the database asynchronously
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Handle concurrency conflicts if the record was changed by someone else
                    if (!InstrumentExists(instrument.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw; // Re-throw the exception if it's not a Not Found issue
                    }
                }
                // Redirect to the Index page after successful update
                return RedirectToAction(nameof(Index));
            }
            // If model state is not valid, return the view with the entered data
            return View(instrument);
        }

        // Helper method to check if an instrument exists (used in Edit)
        private bool InstrumentExists(int id)
        {
            return _context.Instruments.Any(e => e.Id == id);
        }

        // --- Delete Actions ---

        // GET: Inventory/Delete/5
        // Displays a confirmation page before deleting an instrument
        public async Task<IActionResult> Delete(int? id) // Nullable int for the ID
        {
            // If no ID is provided, return Not Found
            if (id == null)
            {
                return NotFound();
            }

            // Find the instrument by ID, including related data if necessary (not needed here)
            var instrument = await _context.Instruments
                .FirstOrDefaultAsync(m => m.Id == id);

            // If the instrument is not found, return Not Found
            if (instrument == null)
            {
                return NotFound();
            }
            // Return the confirmation view with the instrument data
            return View(instrument);
        }

        // POST: Inventory/Delete/5
        // Handles the confirmation and deletes the instrument from the database
        [HttpPost, ActionName("Delete")] // ActionName specifies the action name for routing
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) // Takes the ID to delete
        {
            // Find the instrument to delete by ID
            var instrument = await _context.Instruments.FindAsync(id);

            // If the instrument is found
            if (instrument != null)
            {
                // Remove the instrument from the DbContext
                _context.Instruments.Remove(instrument);
            }

            // Save the changes to the database asynchronously
            await _context.SaveChangesAsync();
            // Redirect to the Index page after deletion
            return RedirectToAction(nameof(Index));
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
