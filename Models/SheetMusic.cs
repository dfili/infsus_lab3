using System.ComponentModel.DataAnnotations; // Required for data annotations if needed later

namespace tamb.Models
{
    // Represents a piece of digital sheet music in the inventory
    public class SheetMusic
    {
        // Unique identifier for the sheet music
        public int id { get; set; }

        // Title of the musical piece
        [Required] // Example data annotation: Title is required
        public string? Title { get; set; }
        // Composer(s) of the piece
        public string? Composer { get; set; }
        // Arranger(s) of the piece (optional)
        public string? Arranger { get; set; }
        // Instrument(s) the sheet music is for
        public string? Instrumentation { get; set; }
        // Genre of the music (e.g., "Classical", "Jazz", "Pop")
        public string? Genre { get; set; }
        // File name or path for the digital file (e.g., "Beethoven_Symphony5.pdf")
        // In a real application, this would likely be a path to storage or a unique identifier
        [Required]
        public string? FileName { get; set; }
        // Date the sheet music was added to the inventory
        public DateTime DateAdded { get; set; }

        // Any additional notes
        public string? Notes { get; set; }
    }
}
