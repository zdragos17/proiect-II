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

                if (existingBook == null)
                {
                    db.Books.Add(book);
                }
                else
                {
                    existingBook.Author = book.Author;
                    existingBook.Subject = book.Subject;

                    if (!string.IsNullOrEmpty(book.Status))
                    {
                        existingBook.Status = book.Status;
                    }
                    if (book.ReservedBy != null)
                    {
                        existingBook.ReservedBy = book.ReservedBy;
                    }

                }
            }

            db.SaveChanges();
        }
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

        public async Task SeedDatabaseWithApiBooks()
        {
            try
            {
                using (var db = new LibraryContext())
                {
                    var existingTitles = db.Books.Select(b => b.Title.ToLower()).ToList();

                    using (HttpClient client = new HttpClient())
                    {
                        // 1. Definim categoriile pentru API și traducerea lor curată pentru baza ta de date
                        var domenii = new System.Collections.Generic.Dictionary<string, string>
                {
                    { "programming", "Informatică" },
                    { "science", "Știință" },
                    { "history", "Istorie" },
                    { "art", "Artă" },
                    { "fiction", "Ficțiune" },
                    { "fantasy", "Fantezie" },
                    { "mystery", "Mister" },
                    { "philosophy", "Filosofie" }
                };

                        Random rnd = new Random();
                        var keys = System.Linq.Enumerable.ToList(domenii.Keys);

                        // Alegem un cuvânt cheie random pentru API
                        string searchKeyword = keys[rnd.Next(keys.Count)];
                        // Luăm traducerea frumoasă pentru a o pune în baza de date
                        string domeniuCurat = domenii[searchKeyword];

                        int randomPage = rnd.Next(1, 4);

                        string url = $"https://openlibrary.org/search.json?q={searchKeyword}&limit=100&page={randomPage}";
                        HttpResponseMessage response = await client.GetAsync(url);

                        if (response.IsSuccessStatusCode)
                        {
                            string jsonString = await response.Content.ReadAsStringAsync();

                            using (System.Text.Json.JsonDocument doc = System.Text.Json.JsonDocument.Parse(jsonString))
                            {
                                var docs = doc.RootElement.GetProperty("docs");
                                int addedCount = 0;

                                foreach (var item in docs.EnumerateArray())
                                {
                                    if (addedCount >= 40) break;

                                    if (!item.TryGetProperty("title", out var titleElement)) continue;
                                    string title = titleElement.GetString();

                                    if (existingTitles.Contains(title.ToLower())) continue;

                                    string author = "Autor Necunoscut";
                                    if (item.TryGetProperty("author_name", out var authorElement) && authorElement.GetArrayLength() > 0)
                                    {
                                        author = authorElement[0].GetString();
                                    }

                                    // 2. FORȚĂM domeniul tradus perfect, ignorând ce zice API-ul
                                    Book newBook = new Book
                                    {
                                        Title = title,
                                        Author = author,
                                        Subject = domeniuCurat, // Aici punem "Ficțiune", "Istorie", etc.
                                        Status = "Disponibil",
                                        ReservedBy = ""
                                    };

                                    db.Books.Add(newBook);
                                    existingTitles.Add(title.ToLower());
                                    addedCount++;
                                }

                                if (addedCount > 0)
                                {
                                    await db.SaveChangesAsync();
                                    System.Windows.MessageBox.Show($"S-au adăugat {addedCount} cărți noi din domeniul '{domeniuCurat}'!", "Succes API", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                string innerError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                System.Windows.MessageBox.Show($"Eroare: {innerError}", "Eroare API", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }

           

        }


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