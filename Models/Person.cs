using System;
using System.Collections.Generic; // Required for List<T>
using System.ComponentModel.DataAnnotations; // Required for data annotations

namespace tamb.Models
{
    // Represents a person who interacts with the inventory system
    public class Person
    {
        // Unique identifier for the person
        public int Id { get; set; }

        // Full name of the person
        [Required] // Name is required
        [Display(Name = "Ime i Prezime")] // Display label for views
        public string ImePrezime { get; set; } = string.Empty; // Default to empty string

        // Contact information (e.g., email, phone number)
        [Display(Name = "Kontakt")]
        public string PhoneNumber { get; set; } = string.Empty; // Default to empty string
        public string Email { get; set; } = string.Empty;
        // Date of birth
        [Display(Name = "Datum rođenja")]
        [DataType(DataType.Date)]
        public DateTime? DatumRodjenja { get; set; } // Nullable DateTime

        [Display(Name = "Instrumenti koje svira")]
        public ICollection<Instrument> PlaysInstruments { get; set; } = new List<Instrument>();

        [Display(Name = "Rezervacije")]
        public ICollection<Reservation> Rezervacije { get; set; } = new List<Reservation>();
    }
}
