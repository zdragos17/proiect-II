using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Globalization;

namespace Proiect
{
    public partial class StudentWindow : Window
    {
        private readonly Services.LibraryService _libraryService = new Services.LibraryService();
        private string currentUsername;
        private List<Book> books = new List<Book>();

        public StudentWindow(string username)
        {
            InitializeComponent();
            currentUsername = username;
            WelcomeTextBlock.Text = $"Bun venit, {currentUsername}!";

            EnsureBooksFileExists();
            books = _libraryService.GetAllBooks();
            RemoveExpiredReservations();
            RefreshBooksGrid();

            this.Activate();
            this.Focus();
        }

        private string NormalizeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            string normalized = text.ToLower().Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();
            foreach (char c in normalized)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(c);
                if (uc != UnicodeCategory.NonSpacingMark) sb.Append(c);
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        private void EnsureBooksFileExists()
        {
            var existingBooks = _libraryService.GetAllBooks();
            if (existingBooks.Count == 0)
            {
                List<Book> defaultBooks = new List<Book>
                {
                    new Book { Title = "Ion", Author = "Liviu Rebreanu", Subject = "Literatură", IsReserved = false, ReservedBy = "" },
                    new Book { Title = "Moromeții", Author = "Marin Preda", Subject = "Literatură", IsReserved = false, ReservedBy = "" },
                    new Book { Title = "Programare C#", Author = "Andrew Troelsen", Subject = "Informatică", IsReserved = false, ReservedBy = "" },
                    new Book { Title = "Algoritmi", Author = "Thomas H. Cormen", Subject = "Informatică", IsReserved = false, ReservedBy = "" },
                    new Book { Title = "Analiză Matematică", Author = "Autor Necunoscut", Subject = "Matematică", IsReserved = false, ReservedBy = "" }
                };
                _libraryService.SaveBooks(defaultBooks);
            }
        }

        private void RemoveExpiredReservations()
        {
            bool booksChanged = false;
            bool borrowedChanged = false;

            List<BorrowedBook> borrowedBooks = _libraryService.GetBorrowedBooks();

            foreach (Book book in books)
            {
                if (book.IsReserved && book.ReservationDate.HasValue)
                {
                    TimeSpan elapsed = DateTime.Now - book.ReservationDate.Value;
                    if (elapsed > TimeSpan.FromHours(24))
                    {
                        bool isStillOnlyReserved = borrowedBooks.Any(b => b.Title == book.Title && b.Username == book.ReservedBy && b.Status == "Rezervata");
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

            if (booksChanged) _libraryService.SaveBooks(books);
            if (borrowedChanged) _libraryService.SaveBorrowedBooks(borrowedBooks);
        }

        private void RefreshBooksGrid()
        {
            books = _libraryService.GetAllBooks();
            RemoveExpiredReservations();
            books = _libraryService.GetAllBooks();

            List<BorrowedBook> borrowedBooks = _libraryService.GetBorrowedBooks();

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
                    book.BorrowDate = latestRecord.BorrowDate;
                }
                else
                {
                    book.Status = "Disponibila";
                    book.ReservationDate = null;
                    book.BorrowDate = null;
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
                MessageBox.Show("Introdu un titlu, autor sau domeniu.");
                return;
            }

            var filteredBooks = books.Where(b =>
                NormalizeText(b.Title).Contains(searchText) ||
                NormalizeText(b.Author).Contains(searchText) ||
                NormalizeText(b.Subject).Contains(searchText)).ToList();

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
                NormalizeText(b.Subject).Contains(searchText)).ToList();

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

            if (selectedBook.Status == "Imprumutata" || selectedBook.Status == "Împrumutată")
            {
                MessageBox.Show("Cartea nu este disponibilă pentru rezervare.");
                return;
            }

            if (selectedBook.IsReserved || selectedBook.Status == "Rezervata" || selectedBook.Status == "Rezervată")
            {
                MessageBox.Show("Cartea este deja rezervată.");
                return;
            }

            selectedBook.IsReserved = true;
            selectedBook.ReservedBy = currentUsername;
            selectedBook.ReservationDate = DateTime.Now;
            selectedBook.Status = "Rezervata";

            _libraryService.SaveBooks(books);

            List<BorrowedBook> borrowedBooks = _libraryService.GetBorrowedBooks();
            borrowedBooks.Add(new BorrowedBook
            {
                Username = currentUsername,
                Title = selectedBook.Title,
                Author = selectedBook.Author,
                Status = "Rezervata",
                ReservationDate = DateTime.Now,
                BorrowDate = null
            });

            _libraryService.SaveBorrowedBooks(borrowedBooks);
            RefreshBooksGrid();

            MessageBox.Show("Cartea a fost rezervată. Ai 24 de ore să o ridici.");
        }

        private void MyBooksButton_Click(object sender, RoutedEventArgs e)
        {
            MyBooksWindow myBooksWindow = new MyBooksWindow(currentUsername);
            myBooksWindow.Show();
            this.Close();
        }
        private void StudyRoomsButton_Click(object sender, RoutedEventArgs e)
        {
            StudyRoomsWindow studyRoomsWindow = new StudyRoomsWindow(currentUsername);
            studyRoomsWindow.Show();
            this.Close();
        }
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void BooksDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Book selectedBook = BooksDataGrid.SelectedItem as Book;

            if (selectedBook != null)
            {
                BookDetailsWindow detailsPopup = new BookDetailsWindow(selectedBook);
                detailsPopup.Owner = this;
                detailsPopup.ShowDialog();
            }
        }
        private void AIAsistentButton_Click(object sender, RoutedEventArgs e)
        {
            AssistantChatWindow chatWindow = new AssistantChatWindow(currentUsername);
            chatWindow.Owner = this;
            chatWindow.ShowDialog();
        }
    }
}