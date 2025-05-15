using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // Required for data annotations if needed later

namespace Tamb.Models
{
    // Represents an instrument in the inventory
    public class Instrument
    {
        // Unique identifier for the instrument
        public int Id { get; set; }

        // Name of the instrument (e.g., "Guitar", "Piano")
        [Required] // Example data annotation: Name is required
        public string Name { get; set; } = string.Empty;

        // Type of the instrument (e.g., "String", "Keyboard")
        public string Type { get; set; } = string.Empty;

        // Manufacturer of the instrument
        public string Manufacturer { get; set; } = string.Empty;

        // Model number or name
        public string? Model { get; set; }

        // Year of manufacture (optional)
        public int? Year { get; set; } // Nullable integer

        // Condition of the instrument (e.g., "Excellent", "Good", "Fair")
        public string? Condition { get; set; }

        // Current status (e.g., "Available", "Reserved", "Loaned")
        public string? Status { get; set; }

        // Any additional notes or specifics
        public string? Notes { get; set; }

        public List<Reservation>? Reservations { get; set; } // Navigation property for related reservations

    }
}
