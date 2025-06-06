﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using tamb.Data;

#nullable disable

namespace tamb.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("tamb.Models.Instrument", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Condition")
                        .HasColumnType("text");

                    b.Property<string>("Manufacturer")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Model")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Notes")
                        .HasColumnType("text");

                    b.Property<int?>("PersonId")
                        .HasColumnType("integer");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int?>("Year")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("PersonId");

                    b.ToTable("Instruments");
                });

            modelBuilder.Entity("tamb.Models.Person", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("DatumRodjenja")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ImePrezime")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Persons");
                });

            modelBuilder.Entity("tamb.Models.Reservation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("EndDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("InstrumentId")
                        .HasColumnType("integer");

                    b.Property<int>("ReservedById")
                        .HasColumnType("integer");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("InstrumentId");

                    b.HasIndex("ReservedById");

                    b.ToTable("Reservations");
                });

            modelBuilder.Entity("tamb.Models.SheetMusic", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("id"));

                    b.Property<string>("Arranger")
                        .HasColumnType("text");

                    b.Property<string>("Composer")
                        .HasColumnType("text");

                    b.Property<DateTime>("DateAdded")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Genre")
                        .HasColumnType("text");

                    b.Property<string>("Instrumentation")
                        .HasColumnType("text");

                    b.Property<string>("Notes")
                        .HasColumnType("text");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("id");

                    b.ToTable("SheetMusic");
                });

            modelBuilder.Entity("tamb.Models.Instrument", b =>
                {
                    b.HasOne("tamb.Models.Person", null)
                        .WithMany("PlaysInstruments")
                        .HasForeignKey("PersonId");
                });

            modelBuilder.Entity("tamb.Models.Reservation", b =>
                {
                    b.HasOne("tamb.Models.Instrument", "Instrument")
                        .WithMany("Reservations")
                        .HasForeignKey("InstrumentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("tamb.Models.Person", "ReservedBy")
                        .WithMany("Rezervacije")
                        .HasForeignKey("ReservedById")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Instrument");

                    b.Navigation("ReservedBy");
                });

            modelBuilder.Entity("tamb.Models.Instrument", b =>
                {
                    b.Navigation("Reservations");
                });

            modelBuilder.Entity("tamb.Models.Person", b =>
                {
                    b.Navigation("PlaysInstruments");

                    b.Navigation("Rezervacije");
                });
#pragma warning restore 612, 618
        }
    }
}
