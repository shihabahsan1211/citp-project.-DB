using Microsoft.EntityFrameworkCore;
using Movie.Infrastructure.Models;

namespace Movie.Infrastructure.Data
{
    public class ImdbDbContext : DbContext
    {
        public ImdbDbContext(DbContextOptions<ImdbDbContext> options)
            : base(options) { }

        // IMDB (read-only) entities – schema: movie
        public DbSet<Title> Titles { get; set; } = null!;
        public DbSet<Person> Persons { get; set; } = null!;

        // Auth entity – schema: framework
        public DbSet<AppUser> AppUsers { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder b)
        {
            // ---- IMDB schema ----
            b.Entity<Title>()
                .ToTable("title", "movie")
                .HasKey(t => t.Tconst);

            b.Entity<Person>()
                .ToTable("person", "movie")
                .HasKey(p => p.Nconst);

            // ---- Framework schema (REAL TABLE) ----
            b.Entity<AppUser>(e =>
            {
                e.ToTable("app_user", "framework");

                // ✅ REAL DB column name
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("user_id");

                e.Property(x => x.Username)
                    .HasColumnName("username")
                    .IsRequired()
                    .HasMaxLength(50);

                e.Property(x => x.Email)
                    .HasColumnName("email")
                    .IsRequired()
                    .HasMaxLength(120);

                e.Property(x => x.PasswordHash)
                    .HasColumnName("password_hash")
                    .IsRequired();

                e.HasIndex(x => x.Username).IsUnique();
                e.HasIndex(x => x.Email).IsUnique();
            });
        }
    }
}
