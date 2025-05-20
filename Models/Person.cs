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

        // --- Navigation Properties ---

        // Collection of Instruments this person knows how to play
        // This represents a many-to-many relationship (a person can play many instruments, an instrument can be played by many people)
        // In the database, this would typically be represented by a linking table.
        // For the model, a simple list is sufficient to represent the relationship.
        // Note: You would need to manage the linking table in your DbContext and migrations.
        [Display(Name = "Instrumenti koje svira")]
        public ICollection<Instrument> PlaysInstruments { get; set; } = new List<Instrument>();

        // Collection of Reservations made by this person
        // This represents a one-to-many relationship (one person can have many reservations)
        // In the database, this requires a foreign key in the Rezervacija table pointing back to the Person table.
        [Display(Name = "Rezervacije")]
        public ICollection<Reservation> Rezervacije { get; set; } = new List<Reservation>();
    }
}
