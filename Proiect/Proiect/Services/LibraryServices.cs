using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Proiect;

namespace Proiect.Services
{
    public class LibraryService
    {
        private readonly string booksFilePath = "books.json";
        private readonly string borrowedBooksFilePath = "borrowedBooks.json";
        private readonly string usersFilePath = "users.json";
        private readonly string studySeatsFilePath = "studySeats.json";

        // --- GESTIUNE CARTI (BOOKS) ---
        public List<Book> GetAllBooks()
        {
            if (!File.Exists(booksFilePath)) return new List<Book>();
            string json = File.ReadAllText(booksFilePath);
            return string.IsNullOrWhiteSpace(json) ? new List<Book>() : JsonSerializer.Deserialize<List<Book>>(json) ?? new List<Book>();
        }

        public void SaveBooks(List<Book> booksToSave)
        {
            string json = JsonSerializer.Serialize(booksToSave, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(booksFilePath, json);
        }

        // --- GESTIUNE IMPRUMUTURI (BORROWED BOOKS) ---
        public List<BorrowedBook> GetBorrowedBooks()
        {
            if (!File.Exists(borrowedBooksFilePath)) return new List<BorrowedBook>();
            string json = File.ReadAllText(borrowedBooksFilePath);
            return string.IsNullOrWhiteSpace(json) ? new List<BorrowedBook>() : JsonSerializer.Deserialize<List<BorrowedBook>>(json) ?? new List<BorrowedBook>();
        }

        public void SaveBorrowedBooks(List<BorrowedBook> borrowedBooksToSave)
        {
            string json = JsonSerializer.Serialize(borrowedBooksToSave, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(borrowedBooksFilePath, json);
        }

        // --- GESTIUNE UTILIZATORI ---
        public List<User> GetAllUsers()
        {
            if (!File.Exists(usersFilePath)) return new List<User>();
            string json = File.ReadAllText(usersFilePath);
            return string.IsNullOrWhiteSpace(json) ? new List<User>() : JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
        }

        public void SaveUsers(List<User> usersToSave)
        {
            string json = JsonSerializer.Serialize(usersToSave, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(usersFilePath, json);
        }
        // --- GESTIUNE LOCURI DE STUDIU (STUDY SEATS) ---

        public List<StudySeat> GetAllSeats()
        {
            if (!File.Exists(studySeatsFilePath))
            {
                
                var initialSeats = new List<StudySeat>();
                for (int i = 1; i <= 20; i++)
                {
                    initialSeats.Add(new StudySeat { SeatNumber = i, IsReserved = false, ReservedBy = "" });
                }
                SaveSeats(initialSeats);
                return initialSeats;
            }

            string json = File.ReadAllText(studySeatsFilePath);
            return string.IsNullOrWhiteSpace(json) ? new List<StudySeat>() : JsonSerializer.Deserialize<List<StudySeat>>(json) ?? new List<StudySeat>();
        }

        public void SaveSeats(List<StudySeat> seatsToSave)
        {
            string json = JsonSerializer.Serialize(seatsToSave, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(studySeatsFilePath, json);
        }

        public void RemoveExpiredSeatReservations()
        {
            var seats = GetAllSeats();
            bool changed = false;

            foreach (var seat in seats)
            {
                if (seat.IsReserved && seat.ReservationDate.HasValue)
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
            }

            if (changed)
            {
                SaveSeats(seats);
            }
        }
    }
}