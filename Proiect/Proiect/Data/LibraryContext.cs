using Microsoft.EntityFrameworkCore;

namespace Proiect.Data
{
    public class LibraryContext : DbContext
    {
        public DbSet<StudySeat> StudySeat { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<BorrowedBook> BorrowedBooks { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Construiește calea absolută către folderul unde rulează aplicația (bin/Debug/...)
            string baseDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            string dbPath = System.IO.Path.Combine(baseDirectory, "biblioteca.db");

            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Setăm cheile primare conform structurii tale
            modelBuilder.Entity<User>().HasKey(u => u.Username);
            modelBuilder.Entity<Book>().HasKey(b => b.Id);
            modelBuilder.Entity<BorrowedBook>().HasKey(bb => new { bb.Username, bb.Title, bb.ReservationDate });
            modelBuilder.Entity<StudySeat>().HasKey(s => s.SeatNumber);
        }
    }
}