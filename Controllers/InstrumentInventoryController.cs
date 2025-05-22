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

        public async Task<IActionResult> Index(string? searchString)
        {
            var instruments = _context.Instruments.AsQueryable(); 
            
            if (!string.IsNullOrEmpty(searchString))
            {
                ViewData["CurrentFilter"] = searchString;
                instruments = instruments.Where(s => s.Name.ToLower().Contains(searchString.ToLower()) ||
                                s.Type.ToLower().Contains(searchString.ToLower()) ||
                                s.Manufacturer.ToLower().Contains(searchString.ToLower()));
            }

            return View(await instruments.ToListAsync());
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Type,Manufacturer,Model,Year,Condition,Notes,Price")] Instrument instrument)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(instrument);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }

            return View(instrument);
        }

        // GET: Inventory/Edit/5
        // Displays the form to edit an existing instrument
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var instrument = await _context.Instruments.FindAsync(id);
            if (instrument == null)
            {
                return NotFound();
            }
            return View(instrument);
        }

        // POST: Inventory/Edit/5
        // Handles the form submission to update an existing instrument in the database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Type,Manufacturer,Model,Year,Condition,Notes")] Instrument instrument)
        {
            // Check if the ID in the route matches the ID in the submitted form data
            if (id != instrument.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(instrument);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Instrument uspješno ažuriran!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InstrumentExists(instrument.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
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


        // GET: Inventory/Delete/5
        public async Task<IActionResult> Delete(int? id) // Nullable int for the ID
        {
            if (id == null)
            {
                return NotFound();
            }

            var instrument = await _context.Instruments
                .FirstOrDefaultAsync(m => m.Id == id);

            if (instrument == null)
            {
                return NotFound();
            }
            return View(instrument);
        }

        // POST: Inventory/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) // Takes the ID to delete
        {

            var instrument = await _context.Instruments.FindAsync(id);
            TempData["SuccessMessage"] = "Instrument uspješno izbrisan!";
            if (instrument != null)
            {
                _context.Instruments.Remove(instrument);
            }

            await _context.SaveChangesAsync();
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

        [HttpGet]
        public IActionResult Reserve(int instrumentId)
        {
            return RedirectToAction("Create", "Reservation", new { id = instrumentId });
        }
    }
}
