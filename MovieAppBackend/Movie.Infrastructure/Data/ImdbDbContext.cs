using Microsoft.EntityFrameworkCore;
using Movie.Infrastructure.Models;  // Imports the Title & Person models

namespace Movie.Infrastructure.Data
{
    public class ImdbDbContext : DbContext
    {
        public ImdbDbContext(DbContextOptions<ImdbDbContext> options)
            : base(options) { }

        public DbSet<Title> Titles { get; set; } = null!;
        public DbSet<Person> Persons { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // IMDB data is stored under the "movie" schema
            modelBuilder.Entity<Title>()
                .ToTable("title", schema: "movie")
                .HasKey(t => t.Tconst);  // Define primary key for Title table

            modelBuilder.Entity<Person>()
                .ToTable("person", schema: "movie")
                .HasKey(p => p.Nconst);  // Define primary key for Person table
        }
    }
}
