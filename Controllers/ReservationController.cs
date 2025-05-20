using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // Potrebno za SelectList
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using tamb.Data; // Vaš DbContext namespace
using tamb.Models; // Vaši modeli namespace
using System.Collections.Generic;

namespace  tamb.Controllers
{
    public class ReservationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReservationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Rezervacija
        // (Primjer Index akcije za prikaz liste rezervacija)
        public async Task<IActionResult> Index(int? instrumentFilter)
        {
            var instruments = await _context.Instruments
                                            .OrderBy(i => i.Name) // Order by name for the dropdown
                                            .Select(i => new SelectListItem
                                            {
                                                Value = i.Id.ToString(),
                                                Text = i.Name
                                            })
                                            .ToListAsync();
            // Add the "All Instruments" option at the beginning
            instruments.Insert(0, new SelectListItem { Value = "", Text = "Svi Instrumenti" });

            // Pass the instruments list to the view for the dropdown
            ViewBag.InstrumentFilter = instruments;

            // Include the Instrument navigation property to display Instrument Name in the table
            var rezervacije = _context.Reservations.Include(r => r.Instrument).AsQueryable();
            // Apply filter if an instrument is selected in the dropdown
            if (instrumentFilter.HasValue && instrumentFilter.Value > 0) // Check if a valid instrument ID is selected
            {
                rezervacije = rezervacije.Where(r => r.InstrumentId == instrumentFilter.Value);
            }
            return View(await rezervacije.ToListAsync());
        }


        // GET: Rezervacija/Create
        // Prikazuje formu za kreiranje nove rezervacije
        public IActionResult Create()
        {
            // Dohvati sve instrumente i pripremi ih za padajuću listu
            ViewBag.InstrumentId = new SelectList(_context.Instruments, "Id", "Name");
            ViewBag.ReservedById = new SelectList(_context.Persons, "Id", "ImePrezime");
            return View();
        }

        // POST: Rezervacija/Create
        // Obrađuje slanje forme za dodavanje nove rezervacije
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("InstrumentId,ReservedById,StartDate,EndDate,Status")] Reservation rezervacija)
        {
            Console.WriteLine("Create action started.");
    
            var activeReservationsCount = await _context.Reservations
                .CountAsync(r => r.ReservedById == rezervacija.ReservedById &&
                                 (r.Status == "Confirmed" || r.Status == "Loaned") &&
                                 (r.EndDate.ToUniversalTime().Date >= DateTime.UtcNow.Date)); // Use DateTime.UtcNow.Date for comparison

            if (activeReservationsCount >= 3) // Check against the limit (e.g., 3)
            {
                // Add a model error if the person has too many active reservations
                ModelState.AddModelError("ReservedById", "Osoba već ima 3 ili više aktivnih rezervacija.");
                 // Note: This error message will be displayed by asp-validation-for="PersonId" in the view
            }
            if (ModelState.IsValid)
            {
                Console.WriteLine("Model is valid, proceeding to save the reservation.");
                _context.Add(rezervacija);
                await _context.SaveChangesAsync();

                var instrument = await _context.Instruments.FindAsync(rezervacija.InstrumentId);
                if (instrument != null && instrument.Status == "Available")
                {
                    instrument.Status = "Loaned"; // Change status to Loaned
                    await _context.SaveChangesAsync(); // Save the instrument status change
                }

                TempData["SuccessMessage"] = "Rezervacija uspješno kreirana i status instrumenta ažuriran!";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(error.ErrorMessage);
                }
            }
            ViewBag.InstrumentId = new SelectList(_context.Instruments, "Id", "Name", rezervacija.InstrumentId);
            ViewBag.ReservedById = new SelectList(_context.Persons, "Id", "ImePrezime", rezervacija.ReservedById);
            return View(rezervacija);
        }

        [HttpGet] // Use HttpGet for idempotent operation
        public async Task<JsonResult> CheckActiveReservations(int personId)
        {
            // Define what "active" means: Status is Confirmed or Loaned AND EndDate is null or in the future
            var activeReservationsCount = await _context.Reservations
                .CountAsync(r => r.ReservedById == personId &&
                                 (r.Status == "Confirmed" || r.Status == "Loaned") &&
                                 (r.EndDate.ToUniversalTime().Date >= DateTime.UtcNow.Date)); // Use DateTime.UtcNow.Date for comparison

            // Return a JSON object indicating if the person can reserve and a message
            if (activeReservationsCount >= 3) // Check against the limit (e.g., 3)
            {
                return Json(new { canReserve = false, message = "Osoba već ima 3 ili više aktivnih rezervacija." });
            }
            else
            {
                // You could return the current count if helpful
                 return Json(new { canReserve = true, message = "" });
            }
        }


        // GET: Rezervacija/Edit/5
        // Prikazuje formu za uređivanje postojeće rezervacije
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rezervacija = await _context.Reservations.FindAsync(id);
            if (rezervacija == null)
            {
                return NotFound();
            }
            // Dohvati instrumente i pripremi ih za padajuću listu,
            // s označenim trenutno odabranim instrumentom
            ViewBag.InstrumentId = new SelectList(_context.Instruments, "Id", "Name", rezervacija.InstrumentId);
            ViewBag.PersonId = new SelectList(_context.Persons, "Id", "ImePrezime", rezervacija.ReservedById); // Added PersonId dropdown
            
            return View(rezervacija);
        }

        // POST: Rezervacija/Edit/5
        // Obrađuje slanje forme za ažuriranje postojeće rezervacije
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,InstrumentId,ReservedById,StartDate,EndDate,Status")] Reservation rezervacija)
        {
            if (id != rezervacija.Id)
            {
                return NotFound();
            }

            var activeReservationsCount = await _context.Reservations
                .CountAsync(r => r.ReservedById == rezervacija.ReservedById &&
                                 r.Id != rezervacija.Id && // Exclude the current reservation being edited
                                 (r.Status == "Confirmed" || r.Status == "Loaned") &&
                                 (r.EndDate.ToUniversalTime().Date >= DateTime.UtcNow.Date));

            if (activeReservationsCount >= 3)
            {
                ModelState.AddModelError("ReservedById", "Osoba već ima 3 ili više aktivnih rezervacija (isključujući ovu rezervaciju).");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var originalReservation = await _context.Reservations.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id); // Fetch original to compare status
                    if (originalReservation != null && originalReservation.Status != "Loaned" && rezervacija.Status == "Loaned")
                    {
                        var instrument = await _context.Instruments.FindAsync(rezervacija.InstrumentId);
                        if (instrument != null && instrument.Status != "Loaned") // Only change if not already Loaned
                        {
                             instrument.Status = "Loaned";
                             _context.Update(instrument); // Mark instrument for update
                        }
                    }
                     // If the status is changing from Loaned to something else (e.g., Cancelled, Returned - you might add a 'Returned' status)
                     // You might want to update the instrument status back to Available.
                     if (originalReservation != null && originalReservation.Status == "Loaned" && rezervacija.Status != "Loaned")
                     {
                         // Check if there are *other* active reservations for this instrument
                         var otherActiveReservations = await _context.Reservations
                             .AnyAsync(r => r.InstrumentId == rezervacija.InstrumentId &&
                                            r.Id != rezervacija.Id && // Exclude the current reservation
                                            (r.Status == "Confirmed" || r.Status == "Loaned") &&
                                            (r.EndDate.ToUniversalTime().Date >= DateTime.UtcNow.Date));

                          if (!otherActiveReservations) // If no other active reservations for this instrument
                          {
                              var instrument = await _context.Instruments.FindAsync(rezervacija.InstrumentId);
                              if (instrument != null && instrument.Status == "Loaned") // Only change if currently Loaned
                              {
                                  instrument.Status = "Available"; // Change status back to Available
                                  _context.Update(instrument); // Mark instrument for update
                              }
                          }
                     }


                    _context.Update(rezervacija); // Mark reservation for update
                    await _context.SaveChangesAsync(); // Save all pending changes (reservation and potentially instrument)


                    TempData["SuccessMessage"] = "Rezervacija uspješno ažurirana!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RezervacijaExists(rezervacija.Id))
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
            // Ako validacija ne uspije, ponovno dohvati instrumente za padajuću listu
            ViewBag.InstrumentId = new SelectList(_context.Instruments, "Id", "Name", rezervacija.InstrumentId);
            ViewBag.PersonId = new SelectList(await _context.Persons.ToListAsync(), "Id", "ImePrezime", rezervacija.ReservedById); // Added PersonId dropdown

            return View(rezervacija);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rezervacija = await _context.Reservations
                .Include(r => r.Instrument) // Include Instrument to display its name
                .Include(r => r.ReservedBy) // Include Person to display their name
                .FirstOrDefaultAsync(m => m.Id == id);

            if (rezervacija == null)
            {
                return NotFound();
            }

            return View(rezervacija);
        }

        // POST: Rezervacija/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rezervacija = await _context.Reservations.FindAsync(id);
            if (rezervacija != null)
            {
                 // Optional: Logic to potentially change instrument status back to Available if this was the last active reservation
                 // This would require checking if there are other active reservations for this instrument *before* deleting this one.
                 // For simplicity, we'll skip this for now, but it's a consideration.

                _context.Reservations.Remove(rezervacija);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Rezervacija uspješno izbrisana!";

            return RedirectToAction(nameof(Index));
        }


        // Helper method to check if a reservation exists
        private bool RezervacijaExists(int id)
        {
            return _context.Reservations.Any(e => e.Id == id);
        }

        // (Dodajte Delete i Details akcije ako su potrebne)
    }
}
