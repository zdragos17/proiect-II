using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Proiect
{
    public partial class StudentLoansWindow : Window
    {
        private readonly Services.LibraryService _libraryService = new Services.LibraryService();
        private string studentUsername;
        private List<BorrowedBook> borrowedBooks = new List<BorrowedBook>();
        private List<Book> books = new List<Book>();

        public StudentLoansWindow(string username)
        {
            InitializeComponent();
            studentUsername = username;
            TitleTextBlock.Text = $"Fișa studentului - {studentUsername}";

            RefreshGrid();
        }

        private void RemoveExpiredReservations()
        {
            bool booksChanged = false;
            bool borrowedChanged = false;

            foreach (BorrowedBook borrowedBook in borrowedBooks)
            {
                if (borrowedBook.Status == "Rezervata" && borrowedBook.ReservationDate.HasValue)
                {
                    TimeSpan elapsed = DateTime.Now - borrowedBook.ReservationDate.Value;

                    if (elapsed > TimeSpan.FromHours(24))
                    {
                        borrowedBook.Status = "Expirata";
                        borrowedChanged = true;

                        Book matchingBook = books.FirstOrDefault(b => b.Title == borrowedBook.Title && b.ReservedBy == borrowedBook.Username);

                        if (matchingBook != null)
                        {
                            matchingBook.IsReserved = false;
                            matchingBook.ReservedBy = "";
                            matchingBook.ReservationDate = null;
                            matchingBook.Status = "Disponibila";
                            booksChanged = true;
                        }
                    }
                }
            }

            if (borrowedChanged) _libraryService.SaveBorrowedBooks(borrowedBooks);
            if (booksChanged) _libraryService.SaveBooks(books);
        }

        private void RefreshGrid()
        {
            books = _libraryService.GetAllBooks();
            borrowedBooks = _libraryService.GetBorrowedBooks();

            RemoveExpiredReservations();

            borrowedBooks = _libraryService.GetBorrowedBooks();

            var studentBooks = borrowedBooks.Where(b => b.Username == studentUsername).ToList();

            StudentBooksDataGrid.ItemsSource = null;
            StudentBooksDataGrid.ItemsSource = studentBooks;
        }

        private void MarkAsBorrowedButton_Click(object sender, RoutedEventArgs e)
        {
            BorrowedBook selectedBook = StudentBooksDataGrid.SelectedItem as BorrowedBook;
            if (selectedBook == null)
            {
                MessageBox.Show("Selectează o carte.");
                return;
            }

            if (selectedBook.Status == "Imprumutata")
            {
                MessageBox.Show("Cartea este deja împrumutată.");
                return;
            }

            if (selectedBook.Status == "Expirata")
            {
                MessageBox.Show("Rezervarea este expirată.");
                return;
            }

            selectedBook.Status = "Imprumutata";
            selectedBook.BorrowDate = DateTime.Now;

            Book matchingBook = books.FirstOrDefault(b => b.Title == selectedBook.Title && b.ReservedBy == selectedBook.Username);
            if (matchingBook != null)
            {
                matchingBook.Status = "Imprumutata";
            }

            _libraryService.SaveBorrowedBooks(borrowedBooks);
            _libraryService.SaveBooks(books);
            RefreshGrid();

            MessageBox.Show("Statusul a fost schimbat în Imprumutata.");
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}