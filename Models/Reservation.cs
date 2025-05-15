using System;

namespace Tamb.Models
{
    // Represents a reservation for an instrument (basic structure)
    public class Reservation
    {
        // Unique identifier for the reservation
        public int Id { get; set; }

        // Foreign key to the Instrument being reserved
        public int InstrumentId { get; set; }

        // Navigation property to the reserved Instrument
        public Instrument? Instrument { get; set; }

        // User or entity making the reservation
        public string? ReservedBy { get; set; }

        // Start date of the reservation
        public DateTime StartDate { get; set; }

        // End date of the reservation
        public DateTime EndDate { get; set; }

        // Status of the reservation (e.g., "Available", "Pending", "Confirmed", "Cancelled")
        public string Status { get; set; } = "Available"; // Default status
    }
}
