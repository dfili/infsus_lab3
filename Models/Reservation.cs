using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Versioning;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation; // Potrebno za [ForeignKey]

namespace tamb.Models
{
    // Represents a reservation for an instrument (basic structure)
    public class Reservation
    {
        // Unique identifier for the reservation
        public int Id { get; set; }

        // Foreign key to the Instrument being reserved
        [Display(Name = "Instrument")]
        public int InstrumentId { get; set; }

        // Navigation property to the reserved Instrument
        [ForeignKey("InstrumentId")]
        [ValidateNever]
        public Instrument? Instrument { get; set; }

        // User or entity making the reservation
        [Display(Name = "Rezervirao/la")]
        public int ReservedById { get; set; }
        [ForeignKey("ReservedById")]
        [ValidateNever]
        public Person? ReservedBy { get; set; }

        // Start date of the reservation
        [Display(Name = "Datum početka")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        // End date of the reservation
        [Display(Name = "Datum završetka")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        // Status of the reservation (e.g., "Pending", "Confirmed", "Cancelled")
        [Display(Name = "Status")]
        public string Status { get; set; } = "Confirmed"; // Default status
    }
}
