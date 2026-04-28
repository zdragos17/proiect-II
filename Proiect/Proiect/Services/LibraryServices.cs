using Proiect.Data; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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

                // Căutăm cartea după titlu pentru a vedea dacă există deja
                var existingBook = db.Books.FirstOrDefault(b => b.Title == book.Title);

                if (existingBook == null)
                {
                    // Dacă nu există, o adăugăm ca intrare nouă
                    db.Books.Add(book);
                }
                else
                {
                    // Dacă există, facem UPDATE manual DOAR la coloanele permise
                    existingBook.Author = book.Author;
                    existingBook.Subject = book.Subject;

                    // Păstrăm statusul și persoana care a rezervat-o dacă acestea s-au modificat
                    if (!string.IsNullOrEmpty(book.Status))
                    {
                        existingBook.Status = book.Status;
                    }
                    if (book.ReservedBy != null)
                    {
                        existingBook.ReservedBy = book.ReservedBy;
                    }

                    // NOTĂ: NU ne atingem sub nicio formă de existingBook.Id!
                }
            }

            // Salvăm modificările
            db.SaveChanges();
        }
        // --- PRELUARE CĂRȚI DE PE INTERNET (API) ---
        private static readonly HttpClient _httpClient = new HttpClient();

        public async Task<List<Book>> FetchBooksFromAPI(string query, int maxResults = 10)
        {
            string url = $"https://www.googleapis.com/books/v1/volumes?q={query}&maxResults={maxResults}";

            try
            {
                // Setăm un "User-Agent" ca Google să creadă că suntem un browser normal
                if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
                {
                    _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
                }

                string responseJson = await _httpClient.GetStringAsync(url);
                var apiData = JsonSerializer.Deserialize<GoogleBooksResponse>(responseJson);

                List<Book> newBooks = new List<Book>();

                if (apiData != null && apiData.Items != null)
                {
                    foreach (var item in apiData.Items)
                    {
                        var info = item.VolumeInfo;
                        if (info == null || string.IsNullOrEmpty(info.Title)) continue;

                        Book newBook = new Book
                        {
                            Title = info.Title,
                            Author = info.Authors != null ? string.Join(", ", info.Authors) : "Autor Necunoscut",
                            Subject = info.Categories != null ? string.Join(", ", info.Categories) : "General",
                            Status = "Disponibil",
                            ReservedBy = ""
                        };

                        newBooks.Add(newBook);
                    }
                }
                return newBooks;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la preluarea cărților din API: {ex.Message}");
                return new List<Book>();
            }
        }

        // Funcția principală pe care o apelăm din meniu
        public async Task SeedDatabaseWithApiBooks()
        {
            try
            {
                // 1. Aducem prima tură de cărți
                var cartiRomanesti = await FetchBooksFromAPI("literatura+romana", 10);

                // 2. PAUZĂ DE 2 SECUNDE ca să nu ne blocheze Google
                await Task.Delay(2000);

                // 3. Aducem a doua tură de cărți
                var cartiStiintifice = await FetchBooksFromAPI("science", 10);

                // 4. Le combinăm într-o singură listă
                var toateCartile = cartiRomanesti.Concat(cartiStiintifice).ToList();

                // 5. Le salvăm în baza de date
                if (toateCartile.Any())
                {
                    SaveBooks(toateCartile); // Apelăm funcția ta
                    MessageBox.Show($"Au fost adăugate {toateCartile.Count} cărți noi de pe internet în baza de date!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                string innerError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                MessageBox.Show($"A apărut o problemă la procesarea cărților: {innerError}", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
    public class GoogleBooksResponse
    {
        [JsonPropertyName("items")]
        public List<ApiBookItem> Items { get; set; }
    }

    public class ApiBookItem
    {
        [JsonPropertyName("volumeInfo")]
        public VolumeInfo VolumeInfo { get; set; }
    }

    public class VolumeInfo
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("authors")]
        public List<string> Authors { get; set; }

        [JsonPropertyName("categories")]
        public List<string> Categories { get; set; }
    }

}