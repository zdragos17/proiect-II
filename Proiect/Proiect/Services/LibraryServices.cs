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
                        // 2. Generăm o pagină aleatorie între 1 și 50 pentru a aduce mereu alte cărți
                        Random rnd = new Random();
                        int randomPage = rnd.Next(1, 50);

                        // Adăugăm &page=... la finalul link-ului
                        string url = $"https://openlibrary.org/search.json?q=programming+fiction+science+history+art&limit=100&page={randomPage}";
                        HttpResponseMessage response = await client.GetAsync(url);

                        if (response.IsSuccessStatusCode)
                        {
                            string jsonString = await response.Content.ReadAsStringAsync();

                            using (JsonDocument doc = JsonDocument.Parse(jsonString))
                            {
                                var docs = doc.RootElement.GetProperty("docs");
                                int addedCount = 0;

                                foreach (var item in docs.EnumerateArray())
                                {
                                    // Dacă am adăugat deja 40 de cărți NOI, ne oprim forțat
                                    if (addedCount >= 40) break;

                                    if (!item.TryGetProperty("title", out var titleElement)) continue;
                                    string title = titleElement.GetString();

                                    // 3. FILTRUL ANTI-DUPLICATE: Dacă titlul există deja în baza de date, sărim la următoarea carte
                                    if (existingTitles.Contains(title.ToLower())) continue;

                                    // Extragem autorul
                                    string author = "Autor Necunoscut";
                                    if (item.TryGetProperty("author_name", out var authorElement) && authorElement.GetArrayLength() > 0)
                                    {
                                        author = authorElement[0].GetString();
                                    }

                                    // Extragem domeniul
                                    string subject = "General";
                                    if (item.TryGetProperty("subject", out var subjectElement) && subjectElement.GetArrayLength() > 0)
                                    {
                                        subject = subjectElement[0].GetString();
                                        // Limităm la 50 de caractere în caz că API-ul returnează o descriere prea lungă
                                        if (subject.Length > 50) subject = subject.Substring(0, 50); 
                                    }

                                    // 4. Construim cartea
                                    Book newBook = new Book
                                    {
                                        Title = title,
                                        Author = author,
                                        Subject = subject,
                                        Status = "Disponibil",
                                        ReservedBy = "" // String gol pentru a respecta regula bazei de date
                                    };

                                    db.Books.Add(newBook);
                                    
                                    // O adăugăm și în lista locală ca să nu adăugăm duplicate dacă API-ul are aceeași carte de 2 ori
                                    existingTitles.Add(title.ToLower()); 
                                    addedCount++;
                                }

                                if (addedCount > 0)
                                {
                                    await db.SaveChangesAsync(); // Acum le trimitem pe toate 40 în Azure
                                    MessageBox.Show($"Au fost descărcate și adăugate {addedCount} cărți noi cu succes!", "Succes API", MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                                else
                                {
                                    MessageBox.Show("Toate cărțile aduse de API există deja în baza de date. Nu s-au adăugat duplicate.", "Info API", MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("Eroare la conectarea cu serverele Open Library.", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string innerError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                MessageBox.Show($"Eroare la descărcarea cărților: {innerError}", "Eroare API", MessageBoxButton.OK, MessageBoxImage.Error);
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