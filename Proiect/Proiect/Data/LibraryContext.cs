using Microsoft.EntityFrameworkCore;

namespace Proiect.Data
{
    public class LibraryContext : DbContext
    {
        public DbSet<StudySeat> StudySeat { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<BorrowedBook> BorrowedBooks { get; set; }

        public LibraryContext()
        {
            //Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
            "Server=tcp:biblioteca-server-iulia.database.windows.net,1433;" +
            "Initial Catalog=BibliotecaDB;" +
            "Persist Security Info=False;" +
            "User ID=bibliotecaadmin;" +
            "Password=Proiectii_db;" +
            "MultipleActiveResultSets=False;" +
            "Encrypt=True;" +
            "TrustServerCertificate=False;" +
            "Connection Timeout=30;",
             sqlOptions => sqlOptions.EnableRetryOnFailure()
    );
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasKey(u => u.Username);

            modelBuilder.Entity<User>()
                .Property(u => u.Username)
                .HasMaxLength(100);

            modelBuilder.Entity<Book>()
                .HasKey(b => b.Id);

            modelBuilder.Entity<BorrowedBook>()
                .HasKey(bb => new { bb.Username, bb.Title, bb.ReservationDate });

            modelBuilder.Entity<BorrowedBook>()
                .Property(bb => bb.Username)
                .HasMaxLength(100);

            modelBuilder.Entity<BorrowedBook>()
                .Property(bb => bb.Title)
                .HasMaxLength(200);

            modelBuilder.Entity<BorrowedBook>()
                .Property(bb => bb.ReservationDate)
                .HasMaxLength(50);

            modelBuilder.Entity<StudySeat>()
                .HasKey(s => s.SeatNumber);

            modelBuilder.Entity<StudySeat>()
                .Property(s => s.SeatNumber)
                .ValueGeneratedNever();
        }
    }
}