using Proiect.Data; // Asigură-te că namespace-ul folderului Data e corect
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Proiect.Services
{
    public class LibraryService
    {
        // --- GESTIUNE CARTI (BOOKS) ---
        public List<Book> GetAllBooks()
        {
            using var db = new LibraryContext();
            return db.Books.ToList();
        }

        public void SaveBooks(List<Book> booksToSave)
        {
            using var db = new LibraryContext();
            foreach (var book in booksToSave)
            {
                if (string.IsNullOrEmpty(book.Status)) book.Status = "Disponibil";

                var existingBook = db.Books.FirstOrDefault(b => b.Title == book.Title);
                if (existingBook == null) db.Books.Add(book);
                else db.Entry(existingBook).CurrentValues.SetValues(book);
            }
            db.SaveChanges();
        }

        // --- GESTIUNE IMPRUMUTURI (BORROWED BOOKS) ---
        public List<BorrowedBook> GetBorrowedBooks()
        {
            using var db = new LibraryContext();
            return db.BorrowedBooks.ToList();
        }

        public void SaveBorrowedBooks(List<BorrowedBook> borrowedBooksToSave)
        {
            using var db = new LibraryContext();
            foreach (var borrowed in borrowedBooksToSave)
            {
                var existing = db.BorrowedBooks.FirstOrDefault(bb =>
                    bb.Username == borrowed.Username &&
                    bb.Title == borrowed.Title &&
                    bb.ReservationDate == borrowed.ReservationDate);

                if (existing == null)
                {
                    db.BorrowedBooks.Add(borrowed);
                }
                else
                {
                    db.Entry(existing).CurrentValues.SetValues(borrowed);
                }
            }

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la salvarea împrumutului: " + ex.InnerException?.Message ?? ex.Message);
            }
        }

        // --- GESTIUNE UTILIZATORI ---
        public List<User> GetAllUsers()
        {
            using var db = new LibraryContext();
            return db.Users.ToList();
        }

        public void SaveUsers(List<User> usersToSave)
        {
            using var db = new LibraryContext();

            foreach (var user in usersToSave)
            {
                var existingUser = db.Users.FirstOrDefault(u => u.Username == user.Username);

                if (existingUser == null)
                {
                    db.Users.Add(user);
                }
                else
                {
                    db.Entry(existingUser).CurrentValues.SetValues(user);
                }
            }

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la salvarea în baza de date: " + ex.Message);
            }
        }

        // --- GESTIUNE LOCURI DE STUDIU (STUDY SEATS) ---
        // Notă: Dacă vrei ca și locurile de studiu să fie în baza de date, 
        // trebuie să adaugi DbSet<StudySeat> în LibraryContext.cs și să faci o migrare nouă.

        public List<StudySeat> GetAllSeats()
        {
            using var db = new LibraryContext();
            var seats = db.StudySeat.ToList();

            if (!seats.Any())
            {
                for (int i = 1; i <= 20; i++)
                {
                    seats.Add(new StudySeat { SeatNumber = i, IsReserved = false, ReservedBy = "" });
                }
                db.StudySeat.AddRange(seats);
                db.SaveChanges();
            }
            return seats;
        }

        public void SaveSeats(List<StudySeat> seatsToSave)
        {
            using var db = new LibraryContext();
            db.StudySeat.UpdateRange(seatsToSave);
            db.SaveChanges();
        }

        public void RemoveExpiredSeatReservations()
        {
            using var db = new LibraryContext();
            var seats = db.StudySeat.Where(s => s.IsReserved && s.ReservationDate.HasValue).ToList();
            bool changed = false;

            foreach (var seat in seats)
            {
                TimeSpan elapsed = DateTime.Now - seat.ReservationDate.Value;
                if (elapsed > TimeSpan.FromHours(4))
                {
                    seat.IsReserved = false;
                    seat.ReservedBy = "";
                    seat.ReservationDate = null;
                    changed = true;
                }
            }

            if (changed)
            {
                db.SaveChanges();
            }
        }
    }
}