using Microsoft.EntityFrameworkCore;
using tamb.Models; // Your models namespace

namespace tamb.Data // You might create a 'Data' folder for this
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
        public DbSet<Person> Persons { get; set; } // Assuming you have a Person model


        // protected override void OnModelCreating(ModelBuilder modelBuilder)
        // {
        //     modelBuilder.Entity<Person>()
        //         .HasMany(p => p.Rezervacije)
        //         .WithOne(r => r.Person)
        //         .HasForeignKey(r => r.PersonId);
        //     base.OnModelCreating(modelBuilder);
        // }
    }
}
