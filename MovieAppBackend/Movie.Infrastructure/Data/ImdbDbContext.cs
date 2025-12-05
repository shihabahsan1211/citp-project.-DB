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

        // Auth + user framework entities – schema: framework
        public DbSet<AppUser> AppUsers { get; set; } = null!;
        public DbSet<UserTitleRating> UserTitleRatings { get; set; } = null!;
        public DbSet<UserPersonRating> UserPersonRatings { get; set; } = null!;
        public DbSet<UserBookmarkTitle> UserBookmarkTitles { get; set; } = null!;
        public DbSet<UserBookmarkPerson> UserBookmarkPersons { get; set; } = null!;
        public DbSet<UserSearchHistory> UserSearchHistories { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder b)
        {
            // ---- IMDB schema ----
            b.Entity<Title>()
                .ToTable("title", "movie")
                .HasKey(t => t.Tconst);

            b.Entity<Person>()
                .ToTable("person", "movie")
                .HasKey(p => p.Nconst);

            // ---- framework.app_user ----
            b.Entity<AppUser>(e =>
            {
                e.ToTable("app_user", "framework");

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

            // ---- framework.user_title_rating ----
            b.Entity<UserTitleRating>(e =>
            {
                e.ToTable("user_title_rating", "framework");

                e.HasKey(x => x.RatingId);
                e.Property(x => x.RatingId).HasColumnName("rating_id");
                e.Property(x => x.UserId).HasColumnName("user_id");
                e.Property(x => x.Tconst).HasColumnName("tconst");
                e.Property(x => x.Rating).HasColumnName("rating");
                e.Property(x => x.RatedAt).HasColumnName("rated_at");
            });

            // ---- framework.user_person_rating ----
            b.Entity<UserPersonRating>(e =>
            {
                e.ToTable("user_person_rating", "framework");

                e.HasKey(x => x.RatingId);
                e.Property(x => x.RatingId).HasColumnName("rating_id");
                e.Property(x => x.UserId).HasColumnName("user_id");
                e.Property(x => x.Nconst).HasColumnName("nconst");
                e.Property(x => x.Rating).HasColumnName("rating");
                e.Property(x => x.RatedAt).HasColumnName("rated_at");
            });

            // ---- framework.user_bookmark_title ----
            b.Entity<UserBookmarkTitle>(e =>
            {
                e.ToTable("user_bookmark_title", "framework");

                // composite key (user_id + tconst)
                e.HasKey(x => new { x.UserId, x.Tconst });

                e.Property(x => x.UserId).HasColumnName("user_id");
                e.Property(x => x.Tconst).HasColumnName("tconst");
                e.Property(x => x.BookmarkedAt).HasColumnName("bookmarked_at");
            });

            // ---- framework.user_bookmark_person ----
            b.Entity<UserBookmarkPerson>(e =>
            {
                e.ToTable("user_bookmark_person", "framework");

                e.HasKey(x => new { x.UserId, x.Nconst });

                e.Property(x => x.UserId).HasColumnName("user_id");
                e.Property(x => x.Nconst).HasColumnName("nconst");
            });

            // ---- framework.user_search_history ----
            b.Entity<UserSearchHistory>(e =>
            {
                e.ToTable("user_search_history", "framework");

                e.HasKey(x => x.SearchId);
                e.Property(x => x.SearchId).HasColumnName("search_id");
                e.Property(x => x.UserId).HasColumnName("user_id");
                e.Property(x => x.QueryText).HasColumnName("query_text");
                e.Property(x => x.SearchedAt).HasColumnName("search_time");
            });
        }
    }
}
