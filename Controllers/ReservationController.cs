using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // Potrebno za SelectList
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using tamb.Data; // Vaš DbContext namespace
using tamb.Models; // Vaši modeli namespace
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System; // Required for DateTime

namespace tamb.Controllers
{
    public class ReservationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReservationController> _logger; // Declare logger field


        public ReservationController(ApplicationDbContext context, ILogger<ReservationController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Reservation
        public async Task<IActionResult> Index(int? instrumentFilter)
        {
            _logger.LogInformation("Prikaz Rezervacija Index stranice. Filter instrumenta: {InstrumentFilter}", instrumentFilter);

            var instruments = await _context.Instruments
                                            .OrderBy(i => i.Name)
                                            .Select(i => new SelectListItem
                                            {
                                                Value = i.Id.ToString(),
                                                Text = i.Name
                                            })
                                            .ToListAsync();

            instruments.Insert(0, new SelectListItem { Value = "", Text = "Svi Instrumenti" });

            ViewBag.InstrumentFilter = instruments;

            var reservations = _context.Reservations
                                .Include(r => r.Instrument)
                                .Include(r => r.ReservedBy) // Include Person to display name
                                .AsQueryable();

            if (instrumentFilter.HasValue && instrumentFilter.Value > 0)
            {
                reservations = reservations.Where(r => r.InstrumentId == instrumentFilter.Value);
                _logger.LogDebug("Filtriranje rezervacija po instrumentu ID: {InstrumentId}", instrumentFilter.Value);
            }

            return View(await reservations.ToListAsync());
        }


        // GET: Reservation/Create
        public async Task<IActionResult> Create()
        {
            _logger.LogInformation("Prikaz forme za kreiranje nove rezervacije.");
            ViewBag.InstrumentId = new SelectList(await _context.Instruments.ToListAsync(), "Id", "Name");
            ViewBag.ReservedById = new SelectList(await _context.Persons.ToListAsync(), "Id", "ImePrezime"); // Corrected ViewBag name
            return View();
        }

        // POST: Reservation/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("InstrumentId,ReservedById,StartDate,EndDate,Status")] Reservation reservation) // Renamed parameter to reservation for consistency
        {
            _logger.LogInformation("Pokušaj kreiranja nove rezervacije za instrument ID: {InstrumentId}, osobu ID: {PersonId}", reservation.InstrumentId, reservation.ReservedById);

            // --- Server-side check for active reservations ---
            // Adjusted for nullable EndDate and correct UTC comparison
            // var activeReservationsCount = await _context.Reservations
            //     .CountAsync(r => r.ReservedById == reservation.ReservedById &&
            //                      (r.Status == "Confirmed" || r.Status == "Loaned") &&
            //                      (r.EndDate == null || r.EndDate.Value.ToUniversalTime().Date >= DateTime.UtcNow.Date));

            // if (activeReservationsCount >= 3)
            // {
            //     _logger.LogWarning("Osoba ID: {PersonId} ima previše aktivnih rezervacija ({Count}).", reservation.ReservedById, activeReservationsCount);
            //     ModelState.AddModelError("ReservedById", "Osoba već ima 3 ili više aktivnih rezervacija.");
            // }
            // -------------------------------------------------

            if (ModelState.IsValid)
            {
                _logger.LogInformation("Model Validation is successful. Proceeding to save the reservation.");
                try
                {
                    // ensure datetime.kind is not unspecified
                    if (reservation.StartDate.Kind == DateTimeKind.Unspecified)
                        reservation.StartDate = DateTime.SpecifyKind(reservation.StartDate, DateTimeKind.Utc);

                    if (reservation.EndDate.HasValue && reservation.EndDate.Value.Kind == DateTimeKind.Unspecified)
                        reservation.EndDate = DateTime.SpecifyKind(reservation.EndDate.Value, DateTimeKind.Utc);
                        
                    _context.Add(reservation);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Rezervacija ID: {ReservationId} uspješno kreirana.", reservation.Id);

                    // --- Update Instrument Status to Loaned ---
                    var instrument = await _context.Instruments.FindAsync(reservation.InstrumentId);
                    if (instrument != null && instrument.Status == "Available")
                    {
                        instrument.Status = "Loaned"; // Change status to Loaned
                        _context.Update(instrument); // Mark instrument for update
                        await _context.SaveChangesAsync(); // Save the instrument status change
                        _logger.LogInformation("Status instrumenta ID: {InstrumentId} promijenjen u 'Loaned' zbog rezervacije ID: {ReservationId}.", instrument.Id, reservation.Id);
                    }
                    else if (instrument == null)
                    {
                        _logger.LogWarning("Instrument s ID: {InstrumentId} nije pronađen prilikom ažuriranja statusa nakon rezervacije.", reservation.InstrumentId);
                    }
                    else
                    {
                        _logger.LogDebug("Status instrumenta ID: {InstrumentId} nije promijenjen jer nije bio 'Available' (trenutni status: {Status}).", instrument.Id, instrument.Status);
                    }
                    // -----------------------------------------

                    TempData["SuccessMessage"] = "Rezervacija uspješno kreirana i status instrumenta ažuriran!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Greška prilikom spremanja nove rezervacije u bazu podataka.");
                    ModelState.AddModelError("", "Došlo je do pogreške prilikom spremanja rezervacije. Molimo pokušajte ponovno.");
                }
            }
            else
            {
                // Log validation errors if ModelState is invalid
                _logger.LogWarning("Validacija modela za kreiranje rezervacije nije uspjela. Greške: {Errors}",
                    string.Join("; ", ModelState.Values
                                                .SelectMany(v => v.Errors)
                                                .Select(e => e.ErrorMessage)));
            }

            // If model state is not valid, repopulate the dropdowns
            ViewBag.InstrumentId = new SelectList(await _context.Instruments.ToListAsync(), "Id", "Name", reservation.InstrumentId);
            ViewBag.ReservedById = new SelectList(await _context.Persons.ToListAsync(), "Id", "ImePrezime", reservation.ReservedById); // Corrected ViewBag name

            return View(reservation);
        }

        [HttpGet]
        public async Task<JsonResult> CheckActiveReservations(int personId)
        {
            _logger.LogDebug("Provjera aktivnih rezervacija za osobu ID: {PersonId}", personId);
            var activeReservationsCount = await _context.Reservations
                .CountAsync(r => r.ReservedById == personId &&
                                 (r.Status == "Confirmed" || r.Status == "Loaned") &&
                                 (r.EndDate == null || r.EndDate.Value.ToUniversalTime().Date >= DateTime.UtcNow.Date));

            if (activeReservationsCount >= 3)
            {
                _logger.LogInformation("Osoba ID: {PersonId} ima {Count} aktivnih rezervacija. Prekoračen limit.", personId, activeReservationsCount);
                return Json(new { canReserve = false, message = "Osoba već ima 3 ili više aktivnih rezervacija." });
            }
            else
            {
                _logger.LogDebug("Osoba ID: {PersonId} ima {Count} aktivnih rezervacija. Može rezervirati.", personId, activeReservationsCount);
                return Json(new { canReserve = true, message = "" });
            }
        }


        // GET: Reservation/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            _logger.LogInformation("Prikaz forme za uređivanje rezervacije ID: {ReservationId}", id);
            if (id == null)
            {
                _logger.LogWarning("Pokušaj uređivanja rezervacije bez ID-a.");
                return NotFound();
            }

            var reservation = await _context.Reservations.FindAsync(id); // Renamed variable
            if (reservation == null)
            {
                _logger.LogWarning("Rezervacija ID: {ReservationId} nije pronađena za uređivanje.", id);
                return NotFound();
            }
            ViewBag.InstrumentId = new SelectList(await _context.Instruments.ToListAsync(), "Id", "Name", reservation.InstrumentId);
            ViewBag.ReservedById = new SelectList(await _context.Persons.ToListAsync(), "Id", "ImePrezime", reservation.ReservedById); // Corrected ViewBag name and used ReservedById
            
            return View(reservation);
        }

        // POST: Reservation/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,InstrumentId,ReservedById,StartDate,EndDate,Status")] Reservation reservation) // Renamed parameter
        {
            _logger.LogInformation("Pokušaj ažuriranja rezervacije ID: {ReservationId}", id);

            if (id != reservation.Id)
            {
                _logger.LogWarning("ID rezervacije u ruti ({RouteId}) ne podudara se s ID-om u modelu ({ModelId}).", id, reservation.Id);
                return NotFound();
            }

            var activeReservationsCount = await _context.Reservations
                .CountAsync(r => r.ReservedById == reservation.ReservedById &&
                                 r.Id != reservation.Id &&
                                 (r.Status == "Confirmed" || r.Status == "Loaned") &&
                                 (r.EndDate == null || r.EndDate.Value.ToUniversalTime().Date >= DateTime.UtcNow.Date));

            if (activeReservationsCount >= 3)
            {
                _logger.LogWarning("Osoba ID: {PersonId} ima previše aktivnih rezervacija ({Count}) (isključujući trenutnu rezervaciju ID: {ReservationId}).", reservation.ReservedById, activeReservationsCount, reservation.Id);
                ModelState.AddModelError("ReservedById", "Osoba već ima 3 ili više aktivnih rezervacija (isključujući ovu rezervaciju).");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var originalReservation = await _context.Reservations.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
                    if (originalReservation != null && originalReservation.Status != "Loaned" && reservation.Status == "Loaned")
                    {
                        var instrument = await _context.Instruments.FindAsync(reservation.InstrumentId);
                        if (instrument != null && instrument.Status != "Loaned")
                        {
                             instrument.Status = "Loaned";
                             _context.Update(instrument);
                             _logger.LogInformation("Status instrumenta ID: {InstrumentId} promijenjen u 'Loaned' zbog ažuriranja rezervacije ID: {ReservationId}.", instrument.Id, reservation.Id);
                        }
                    }
                     if (originalReservation != null && originalReservation.Status == "Loaned" && reservation.Status != "Loaned")
                     {
                         var otherActiveReservations = await _context.Reservations
                             .AnyAsync(r => r.InstrumentId == reservation.InstrumentId &&
                                            r.Id != reservation.Id &&
                                            (r.Status == "Confirmed" || r.Status == "Loaned") &&
                                            (r.EndDate == null || r.EndDate.Value.ToUniversalTime().Date >= DateTime.UtcNow.Date));

                          if (!otherActiveReservations)
                          {
                              var instrument = await _context.Instruments.FindAsync(reservation.InstrumentId);
                              if (instrument != null && instrument.Status == "Loaned")
                              {
                                  instrument.Status = "Available";
                                  _context.Update(instrument);
                                  _logger.LogInformation("Status instrumenta ID: {InstrumentId} promijenjen u 'Available' jer nema drugih aktivnih rezervacija za rezervaciju ID: {ReservationId}.", instrument.Id, reservation.Id);
                              }
                          }
                     }

                    _context.Update(reservation);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Rezervacija ID: {ReservationId} uspješno ažurirana.", reservation.Id);

                    TempData["SuccessMessage"] = "Rezervacija uspješno ažurirana!";
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!ReservationExists(reservation.Id)) // Corrected helper method call
                    {
                        _logger.LogWarning(ex, "Pokušaj ažuriranja nepostojeće rezervacije ID: {ReservationId}. Konkurentnost.", reservation.Id);
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError(ex, "Greška konkurentnosti prilikom ažuriranja rezervacije ID: {ReservationId}.", reservation.Id);
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Greška prilikom ažuriranja rezervacije ID: {ReservationId} u bazi podataka.", reservation.Id);
                    ModelState.AddModelError("", "Došlo je do pogreške prilikom spremanja rezervacije. Molimo pokušajte ponovno.");
                }
            }
            else
            {
                _logger.LogWarning("Validacija modela za ažuriranje rezervacije ID: {ReservationId} nije uspjela. Greške: {Errors}",
                    reservation.Id,
                    string.Join("; ", ModelState.Values
                                                .SelectMany(v => v.Errors)
                                                .Select(e => e.ErrorMessage)));
            }

            ViewBag.InstrumentId = new SelectList(await _context.Instruments.ToListAsync(), "Id", "Name", reservation.InstrumentId);
            ViewBag.ReservedById = new SelectList(await _context.Persons.ToListAsync(), "Id", "ImePrezime", reservation.ReservedById); // Corrected ViewBag name

            return View(reservation);
        }

         // GET: Reservation/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            _logger.LogInformation("Prikaz potvrde brisanja za rezervaciju ID: {ReservationId}", id);
            if (id == null)
            {
                _logger.LogWarning("Pokušaj brisanja rezervacije bez ID-a.");
                return NotFound();
            }

            var reservation = await _context.Reservations
                .Include(r => r.Instrument)
                .Include(r => r.ReservedBy)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (reservation == null)
            {
                _logger.LogWarning("Rezervacija ID: {ReservationId} nije pronađena za brisanje.", id);
                return NotFound();
            }

            return View(reservation);
        }

        // POST: Reservation/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            _logger.LogInformation("Pokušaj brisanja rezervacije ID: {ReservationId}.", id);
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation != null)
            {
                _context.Reservations.Remove(reservation);
            }

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Rezervacija ID: {ReservationId} uspješno izbrisana.", id);
                TempData["SuccessMessage"] = "Rezervacija uspješno izbrisana!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška prilikom brisanja rezervacije ID: {ReservationId} iz baze podataka.", id);
                TempData["ErrorMessage"] = "Došlo je do pogreške prilikom brisanja rezervacije.";
            }

            return RedirectToAction(nameof(Index));
        }


        private bool ReservationExists(int id) // Renamed helper method for consistency
        {
            return _context.Reservations.Any(e => e.Id == id);
        }
    }
}
