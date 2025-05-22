using Microsoft.EntityFrameworkCore;
using tamb.Models; 

namespace tamb.Data
{
    // Your DbContext class, inheriting from EF Core's DbContext
    public class ApplicationDbContext : DbContext
    {
        // Constructor that accepts DbContextOptions
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet properties for each model you want to map to a database table
        // The name of the DbSet property will typically be the table name (e.g., "Instruments", "Reservations")
        public DbSet<Instrument> Instruments { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<SheetMusic> SheetMusic { get; set; }
        public DbSet<Person> Persons { get; set; }
    }
}
