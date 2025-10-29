using Microsoft.EntityFrameworkCore;
using Movie.Infrastructure.Models;   // Title, Person, AppUser, RefreshToken

namespace Movie.Infrastructure.Data
{
    public class ImdbDbContext : DbContext
    {
        public ImdbDbContext(DbContextOptions<ImdbDbContext> options)
            : base(options) { }

        // IMDB (read-only) entities – schema: movie
        public DbSet<Title> Titles { get; set; } = null!;
        public DbSet<Person> Persons { get; set; } = null!;

        // Auth / framework entities – schema: framework
        public DbSet<AppUser> AppUsers { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder b)
        {
            // ---- IMDB schema ----
            b.Entity<Title>()
                .ToTable("title", "movie")
                .HasKey(t => t.Tconst);

            b.Entity<Person>()
                .ToTable("person", "movie")
                .HasKey(p => p.Nconst);

            // ---- Framework schema (Auth) ----
            b.Entity<AppUser>(e =>
            {
                e.ToTable("users", "framework");
                e.HasKey(x => x.Id);

                e.Property(x => x.Username).IsRequired().HasMaxLength(50);
                e.Property(x => x.Email).IsRequired().HasMaxLength(120);
                e.Property(x => x.PasswordHash).IsRequired();

                e.HasIndex(x => x.Username).IsUnique();
                e.HasIndex(x => x.Email).IsUnique();
            });

            b.Entity<RefreshToken>(e =>
            {
                e.ToTable("refresh_tokens", "framework");
                e.HasKey(x => x.Id);

                e.Property(x => x.Token).IsRequired();
                e.Property(x => x.ExpiresAt).IsRequired();

                e.HasOne(x => x.User)
                 .WithMany(u => u.RefreshTokens)
                 .HasForeignKey(x => x.UserId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasIndex(x => x.Token).IsUnique();
            });
        }
    }
}
