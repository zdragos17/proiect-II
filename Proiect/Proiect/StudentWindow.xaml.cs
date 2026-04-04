using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Globalization;
using System.Text;

namespace Proiect
{
    public partial class StudentWindow : Window
    {
        private readonly string booksFilePath = "books.json";
        private readonly string borrowedBooksFilePath = "borrowedBooks.json";

        private string currentUsername;
        private List<Book> books = new List<Book>();

        public StudentWindow(string username)
        {
            InitializeComponent();
            currentUsername = username;
            WelcomeTextBlock.Text = $"Bun venit, {currentUsername}!";

            EnsureBooksFileExists();
            LoadBooks();
            RemoveExpiredReservations();
            RefreshBooksGrid();

            this.Activate();
            this.Focus();
        }
        private string NormalizeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            string normalized = text.ToLower().Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();

            foreach (char c in normalized)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(c);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
        private void EnsureBooksFileExists()
        {
            if (!File.Exists(booksFilePath))
            {
                List<Book> defaultBooks = new List<Book>
                {
                    new Book { Title = "Ion", Author = "Liviu Rebreanu", Subject = "Literatură", IsReserved = false, ReservedBy = "" },
                    new Book { Title = "Moromeții", Author = "Marin Preda", Subject = "Literatură", IsReserved = false, ReservedBy = "" },
                    new Book { Title = "Programare C#", Author = "Andrew Troelsen", Subject = "Informatică", IsReserved = false, ReservedBy = "" },
                    new Book { Title = "Algoritmi", Author = "Thomas H. Cormen", Subject = "Informatică", IsReserved = false, ReservedBy = "" },
                    new Book { Title = "Analiză Matematică", Author = "Autor Necunoscut", Subject = "Matematică", IsReserved = false, ReservedBy = "" }
                };

                SaveBooks(defaultBooks);
            }
        }

        private void LoadBooks()
        {
            if (!File.Exists(booksFilePath))
            {
                books = new List<Book>();
                return;
            }

            string json = File.ReadAllText(booksFilePath);

            if (string.IsNullOrWhiteSpace(json))
            {
                books = new List<Book>();
                return;
            }

            books = JsonSerializer.Deserialize<List<Book>>(json) ?? new List<Book>();
        }

        private void SaveBooks(List<Book> booksToSave)
        {
            string json = JsonSerializer.Serialize(booksToSave, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(booksFilePath, json);
        }

        private List<BorrowedBook> LoadBorrowedBooks()
        {
            if (!File.Exists(borrowedBooksFilePath))
                return new List<BorrowedBook>();

            string json = File.ReadAllText(borrowedBooksFilePath);

            if (string.IsNullOrWhiteSpace(json))
                return new List<BorrowedBook>();

            return JsonSerializer.Deserialize<List<BorrowedBook>>(json) ?? new List<BorrowedBook>();
        }

        private void SaveBorrowedBooks(List<BorrowedBook> borrowedBooks)
        {
            string json = JsonSerializer.Serialize(borrowedBooks, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(borrowedBooksFilePath, json);
        }

        private void RemoveExpiredReservations()
        {
            bool booksChanged = false;
            bool borrowedChanged = false;

            List<BorrowedBook> borrowedBooks = LoadBorrowedBooks();

            foreach (Book book in books)
            {
                if (book.IsReserved && book.ReservationDate.HasValue)
                {
                    TimeSpan elapsed = DateTime.Now - book.ReservationDate.Value;

                    if (elapsed > TimeSpan.FromHours(24))
                    {
                        bool isStillOnlyReserved = borrowedBooks.Any(b =>
                            b.Title == book.Title &&
                            b.Username == book.ReservedBy &&
                            b.Status == "Rezervata");

                        if (isStillOnlyReserved)
                        {
                            book.IsReserved = false;
                            book.ReservedBy = "";
                            book.ReservationDate = null;
                            booksChanged = true;
                        }
                    }
                }
            }

            foreach (BorrowedBook borrowedBook in borrowedBooks)
            {
                if (borrowedBook.Status == "Rezervata" && borrowedBook.ReservationDate.HasValue)
                {
                    TimeSpan elapsed = DateTime.Now - borrowedBook.ReservationDate.Value;

                    if (elapsed > TimeSpan.FromHours(24))
                    {
                        borrowedBook.Status = "Expirata";
                        borrowedChanged = true;
                    }
                }
            }

            if (booksChanged)
            {
                SaveBooks(books);
            }

            if (borrowedChanged)
            {
                SaveBorrowedBooks(borrowedBooks);
            }
        }

        private void RefreshBooksGrid()
        {
            LoadBooks();
            RemoveExpiredReservations();
            LoadBooks();

            List<BorrowedBook> borrowedBooks = LoadBorrowedBooks();

            foreach (Book book in books)
            {
                BorrowedBook latestRecord = borrowedBooks
                    .Where(b => b.Title == book.Title && b.Status != "Expirata")
                    .OrderByDescending(b => b.ReservationDate)
                    .FirstOrDefault();

                if (latestRecord != null)
                {
                    book.Status = latestRecord.Status;
                    book.ReservationDate = latestRecord.ReservationDate;
                }
                else
                {
                    book.Status = "Disponibila";
                    book.ReservationDate = null;
                }
            }

            BooksDataGrid.ItemsSource = null;
            BooksDataGrid.ItemsSource = books;
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchText = NormalizeText(SearchTextBox.Text.Trim());

            if (string.IsNullOrWhiteSpace(searchText))
            {
                MessageBox.Show("Introdu un titlu, autor sau domeniu!.");
                return;
            }

            var filteredBooks = books.Where(b =>
                NormalizeText(b.Title).Contains(searchText) ||
                NormalizeText(b.Author).Contains(searchText) ||
                NormalizeText(b.Subject).Contains(searchText))
                .ToList();

            BooksDataGrid.ItemsSource = null;
            BooksDataGrid.ItemsSource = filteredBooks;
        }

        private void ShowAllButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Clear();
            RefreshBooksGrid();
        }
        private void SearchTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string searchText = NormalizeText(SearchTextBox.Text.Trim());

            if (string.IsNullOrWhiteSpace(searchText))
            {
                RefreshBooksGrid();
                return;
            }

            var filteredBooks = books.Where(b =>
                NormalizeText(b.Title).Contains(searchText) ||
                NormalizeText(b.Author).Contains(searchText) ||
                NormalizeText(b.Subject).Contains(searchText))
                .ToList();

            BooksDataGrid.ItemsSource = null;
            BooksDataGrid.ItemsSource = filteredBooks;
        }
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshBooksGrid();
        }
        private void ReserveButton_Click(object sender, RoutedEventArgs e)
        {
            Book selectedBook = BooksDataGrid.SelectedItem as Book;

            if (selectedBook == null)
            {
                MessageBox.Show("Selectează o carte.");
                return;
            }

            if (selectedBook.IsReserved)
            {
                MessageBox.Show("Cartea este deja rezervată.");
                return;
            }

            selectedBook.IsReserved = true;
            selectedBook.ReservedBy = currentUsername;
            selectedBook.ReservationDate = DateTime.Now;
            selectedBook.Status = "Rezervata";

            SaveBooks(books);

            List<BorrowedBook> borrowedBooks = LoadBorrowedBooks();

            borrowedBooks.Add(new BorrowedBook
            {
                Username = currentUsername,
                Title = selectedBook.Title,
                Author = selectedBook.Author,
                Status = "Rezervata",
                ReservationDate = DateTime.Now,
                BorrowDate = null
            });

            SaveBorrowedBooks(borrowedBooks);

            RefreshBooksGrid();

            MessageBox.Show("Cartea a fost rezervată. Ai 24 de ore să o ridici.");
        }

        private void MyBooksButton_Click(object sender, RoutedEventArgs e)
        {
            MyBooksWindow myBooksWindow = new MyBooksWindow(currentUsername);
            myBooksWindow.Show();
        }
    }
}