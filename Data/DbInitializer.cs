    using tamb.Data;
    using tamb.Models;
    using System;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection; // For IServiceScopeFactory

    namespace tamb.Data
    {
        public static class DbInitializer
        {
            public static void Initialize(IServiceProvider serviceProvider)
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    context.Database.EnsureCreated(); // Ensures the database exists and applies pending migrations if any

                    // Look for any instruments.
                    if (context.Instruments.Any())
                    {
                        return;   // DB has been seeded
                    }

                    // Seed Instruments
                    var instruments = new Instrument[]
                    {
                        new Instrument{Name="Prim",Type="Tambura",Manufacturer="Katulić",Model="",Year=1990,Condition="Good",Notes=""},
                        new Instrument{Name="Brač",Type="Tambura",Manufacturer="Katulić",Model="standard",Year=2006,Condition="Excellent",Notes=""},
                        new Instrument{Name="Kontra",Type="Tambura",Manufacturer="Nepoznato",Model="",Year=null,Condition="Good",Notes="Mirko nabavio"},
                        new Instrument{Name="Basprim",Type="Tambura",Manufacturer="Fender",Model="Antique",Year=2015,Condition="Excellent",Notes=""},
                        new Instrument{Name="Bas",Type="Tambura",Manufacturer="Yamaha",Model="",Year=2000,Condition="Good",Notes=""},
                        new Instrument{Name="Kontra",Type="Tambura",Manufacturer="Nepoznato",Model="",Year=1999,Condition="Good",Notes=""},
                        new Instrument{Name="Harmonika",Type="Harmonika",Manufacturer="Pigini",Model="PRELUDIO P 30",Year=2023,Condition="Excellent",Notes=""},
                        new Instrument{Name="Prim",Type="Tambura",Manufacturer="Katulić",Model="Novi",Year=2000,Condition="Excellent",Notes=""}
                    };
                    context.Instruments.AddRange(instruments);
                    context.SaveChanges(); // Save instruments first to get IDs

                    // Seed Persons
                    var persons = new Person[]
                    {
                        new Person{ImePrezime="Marko Markić", Email="marko@gmail.com", PhoneNumber="123456789", DatumRodjenja=DateTime.SpecifyKind(new DateTime(2004, 11, 5), DateTimeKind.Utc)},
                        new Person{ImePrezime="Pero Perić", Email="pero@tamb.hr", PhoneNumber="234567891", DatumRodjenja=DateTime.SpecifyKind(new DateTime(2002, 11, 27), DateTimeKind.Utc)},
                        new Person{ImePrezime="Iva Ivić", Email="iva@tamb.hr", PhoneNumber="345678912", DatumRodjenja=DateTime.SpecifyKind(new DateTime(2000, 06, 21), DateTimeKind.Utc)},
                        
                    };
                    context.Persons.AddRange(persons);
                    context.SaveChanges(); // Save persons first to get IDs

                    // Seed Reservations (ensure IDs match seeded data)
                    var reservations = new Reservation[]
                    {
                        new Reservation{InstrumentId=3, ReservedById=1, StartDate=DateTime.SpecifyKind(new DateTime(2025, 5, 23), DateTimeKind.Utc), EndDate=DateTime.SpecifyKind(new DateTime(2025, 5, 25), DateTimeKind.Utc), Status="Confimed"},
                        new Reservation{InstrumentId=7, ReservedById=1, StartDate=DateTime.SpecifyKind(new DateTime(2025, 5, 23), DateTimeKind.Utc), EndDate=DateTime.SpecifyKind(new DateTime(2025, 5, 27), DateTimeKind.Utc), Status="Confirmed"}, 
                        new Reservation{InstrumentId=8, ReservedById=1, StartDate=DateTime.SpecifyKind(new DateTime(2025, 5, 20), DateTimeKind.Utc), EndDate=DateTime.SpecifyKind(new DateTime(2025, 6, 1), DateTimeKind.Utc), Status="Confirmed"},
                    };
                    context.Reservations.AddRange(reservations);
                    context.SaveChanges();
                }
            }
        }
    }
    