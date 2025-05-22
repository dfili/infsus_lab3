using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // Potrebno za SelectList
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using tamb.Data; // Vaš DbContext namespace
using tamb.Models; // Vaši modeli namespace
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.EntityFrameworkCore.Metadata.Internal; // Required for DateTime

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
        public async Task<IActionResult> Create(int? instrumentId)
        {
             _logger.LogInformation("Prikaz forme za kreiranje nove rezervacije.");
            var allInstruments = await _context.Instruments.OrderBy(i => i.Name).ToListAsync();
            ViewBag.InstrumentId = new SelectList(allInstruments, "Id", "Name", instrumentId); // Pass instrumentId for pre-selection
            ViewBag.ReservedById = new SelectList(await _context.Persons.ToListAsync(), "Id", "ImePrezime");
           
            return View();
        }

        // POST: Reservation/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("InstrumentId,ReservedById,StartDate,EndDate,Status")] Reservation reservation) // Renamed parameter to reservation for consistency
        {
            _logger.LogInformation("Pokušaj kreiranja nove rezervacije za instrument ID: {InstrumentId}, osobu ID: {PersonId}", reservation.InstrumentId, reservation.ReservedById);

            // --- 1. Validate Date Order ---
            if (reservation.EndDate.HasValue && reservation.StartDate > reservation.EndDate.Value)
            {
                ModelState.AddModelError("EndDate", "Datum završetka ne može biti prije datuma početka.");
                _logger.LogWarning("Kreiranje rezervacije: Datum završetka ({EndDate}) je prije datuma početka ({StartDate}).", reservation.EndDate, reservation.StartDate);
            }

            // --- 2. Check for Overlapping Reservations for the Instrument ---
            // Only perform this check if InstrumentId is valid and dates are valid
            if (reservation.InstrumentId > 0 && ModelState.IsValid)
            {
                // Convert dates to UTC for comparison with database 'timestamp with time zone'
                var newReservationStartDateUtc = DateTime.SpecifyKind(reservation.StartDate, DateTimeKind.Utc);
                var newReservationEndDateUtc = reservation.EndDate.HasValue ? DateTime.SpecifyKind(reservation.EndDate.Value, DateTimeKind.Utc) : (DateTime?)null;

                var overlappingReservation = await _context.Reservations
                    .Where(r => r.InstrumentId == reservation.InstrumentId &&
                                (r.Status == "Confirmed" || r.Status == "Loaned") && // Only consider active/loaned reservations
                                // Check for overlap: (StartA <= EndB) AND (EndA >= StartB)
                                newReservationStartDateUtc <= (r.EndDate == null ? DateTime.MaxValue : r.EndDate.Value.ToUniversalTime()) &&
                                (newReservationEndDateUtc == null ? DateTime.MaxValue : newReservationEndDateUtc.Value) >= r.StartDate.ToUniversalTime())
                    .AnyAsync();

                if (overlappingReservation)
                {
                    ModelState.AddModelError("InstrumentId", "Instrument je već rezerviran ili posuđen u odabranom terminu.");
                    _logger.LogWarning("Kreiranje rezervacije: Instrument ID: {InstrumentId} je nedostupan zbog preklapanja s postojećom rezervacijom.", reservation.InstrumentId);
                }
            }
            
            // --- 3. Validate Person's Active Reservations ---
            // Only perform this check if a person is selected and the instrument is valid
            if (reservation.ReservedById > 0 && ModelState.IsValid) // Check ModelState.IsValid to avoid redundant errors
            {
                var activeReservationsCount = await _context.Reservations
                    .CountAsync(r => r.ReservedById == reservation.ReservedById &&
                                     (r.Status == "Confirmed" || r.Status == "Loaned") &&
                                     (r.EndDate == null || r.EndDate.Value.ToUniversalTime().Date >= DateTime.UtcNow.Date));

                if (activeReservationsCount >= 3)
                {
                    _logger.LogWarning("Osoba ID: {PersonId} ima previše aktivnih rezervacija ({Count}).", reservation.ReservedById, activeReservationsCount);
                    ModelState.AddModelError("ReservedById", $"Osoba već ima {activeReservationsCount} aktivnih rezervacija (limit je 3).");
                }
            }


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

                    TempData["SuccessMessage"] = "Rezervacija uspješno kreirana!";
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

            // --- 1. Validate Date Order ---
            if (reservation.EndDate.HasValue && reservation.StartDate > reservation.EndDate.Value)
            {
                ModelState.AddModelError("EndDate", "Datum završetka ne može biti prije datuma početka.");
                _logger.LogWarning("Ažuriranje rezervacije: Datum završetka ({EndDate}) je prije datuma početka ({StartDate}).", reservation.EndDate, reservation.StartDate);
            }

            // --- 2. Check for Overlapping Reservations for the Instrument (excluding the current reservation) ---
            if (reservation.InstrumentId > 0 && ModelState.IsValid)
            {
                var newReservationStartDateUtc = DateTime.SpecifyKind(reservation.StartDate, DateTimeKind.Utc);
                var newReservationEndDateUtc = reservation.EndDate.HasValue ? DateTime.SpecifyKind(reservation.EndDate.Value, DateTimeKind.Utc) : (DateTime?)null;

                var overlappingReservation = await _context.Reservations
                    .Where(r => r.InstrumentId == reservation.InstrumentId &&
                                r.Id != id && // Exclude the reservation being edited
                                (r.Status == "Confirmed" || r.Status == "Loaned") && // Only consider active/loaned reservations
                                // Check for overlap: (StartA <= EndB) AND (EndA >= StartB)
                                newReservationStartDateUtc <= (r.EndDate == null ? DateTime.MaxValue : r.EndDate.Value.ToUniversalTime()) &&
                                (newReservationEndDateUtc == null ? DateTime.MaxValue : newReservationEndDateUtc.Value) >= r.StartDate.ToUniversalTime())
                    .AnyAsync();

                if (overlappingReservation)
                {
                    ModelState.AddModelError("InstrumentId", "Instrument je već rezerviran ili posuđen u odabranom terminu.");
                    _logger.LogWarning("Ažuriranje rezervacije: Instrument ID: {InstrumentId} je nedostupan zbog preklapanja s postojećom rezervacijom (isključujući trenutnu).", reservation.InstrumentId);
                }
            }
            
            // --- 3. Validate Person's Active Reservations ---
            // Only perform this check if a person is selected and instrument is valid
            if (reservation.ReservedById > 0 && ModelState.IsValid)
            {
                var activeReservationsCount = await _context.Reservations
                    .CountAsync(r => r.ReservedById == reservation.ReservedById &&
                                     r.Id != reservation.Id && // Exclude the current reservation being edited
                                     (r.Status == "Confirmed" || r.Status == "Loaned") &&
                                     (r.EndDate == null || r.EndDate.Value.ToUniversalTime().Date >= DateTime.UtcNow.Date));

                if (activeReservationsCount >= 3)
                {
                    _logger.LogWarning("Osoba ID: {PersonId} ima previše aktivnih rezervacija ({Count}) (isključujući trenutnu rezervaciju ID: {ReservationId}).", reservation.ReservedById, activeReservationsCount, reservation.Id);
                    ModelState.AddModelError("ReservedById", $"Osoba već ima {activeReservationsCount} aktivnih rezervacija (limit je 3).");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Fix datetime kind
                    if (reservation.StartDate.Kind == DateTimeKind.Unspecified)
                        reservation.StartDate = DateTime.SpecifyKind(reservation.StartDate, DateTimeKind.Utc);

                    if (reservation.EndDate.HasValue && reservation.EndDate.Value.Kind == DateTimeKind.Unspecified)
                        reservation.EndDate = DateTime.SpecifyKind(reservation.EndDate.Value, DateTimeKind.Utc);

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
