using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;

namespace Proiect
{
    public partial class StudentLoansWindow : Window
    {
        private readonly string borrowedBooksFilePath = "borrowedBooks.json";
        private readonly string booksFilePath = "books.json";

        private string studentUsername;
        private List<BorrowedBook> borrowedBooks = new List<BorrowedBook>();
        private List<Book> books = new List<Book>();

        public StudentLoansWindow(string username)
        {
            InitializeComponent();
            studentUsername = username;
            TitleTextBlock.Text = $"Fișa studentului - {studentUsername}";

            LoadBooks();
            LoadBorrowedBooks();
            RemoveExpiredReservations();
            RefreshGrid();
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

        private void SaveBooks()
        {
            string json = JsonSerializer.Serialize(books, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(booksFilePath, json);
        }

        private void LoadBorrowedBooks()
        {
            if (!File.Exists(borrowedBooksFilePath))
            {
                borrowedBooks = new List<BorrowedBook>();
                return;
            }

            string json = File.ReadAllText(borrowedBooksFilePath);

            if (string.IsNullOrWhiteSpace(json))
            {
                borrowedBooks = new List<BorrowedBook>();
                return;
            }

            borrowedBooks = JsonSerializer.Deserialize<List<BorrowedBook>>(json) ?? new List<BorrowedBook>();
        }

        private void SaveBorrowedBooks()
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

            foreach (BorrowedBook borrowedBook in borrowedBooks)
            {
                if (borrowedBook.Status == "Rezervata" && borrowedBook.ReservationDate.HasValue)
                {
                    TimeSpan elapsed = DateTime.Now - borrowedBook.ReservationDate.Value;

                    if (elapsed > TimeSpan.FromHours(24))
                    {
                        borrowedBook.Status = "Expirata";
                        borrowedChanged = true;

                        Book matchingBook = books.FirstOrDefault(b =>
                            b.Title == borrowedBook.Title &&
                            b.ReservedBy == borrowedBook.Username);

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

            if (borrowedChanged)
            {
                SaveBorrowedBooks();
            }

            if (booksChanged)
            {
                SaveBooks();
            }
        }

        private void RefreshGrid()
        {
            LoadBooks();
            LoadBorrowedBooks();
            RemoveExpiredReservations();
            LoadBorrowedBooks();

            var studentBooks = borrowedBooks
                .Where(b => b.Username == studentUsername)
                .ToList();

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

            Book matchingBook = books.FirstOrDefault(b =>
                b.Title == selectedBook.Title &&
                b.ReservedBy == selectedBook.Username);

            if (matchingBook != null)
            {
                matchingBook.Status = "Imprumutata";
            }

            SaveBorrowedBooks();
            SaveBooks();
            RefreshGrid();

            MessageBox.Show("Statusul a fost schimbat în Imprumutata.");
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}