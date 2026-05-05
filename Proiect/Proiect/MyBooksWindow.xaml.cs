using Proiect.Data;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Proiect
{
    public partial class MyBooksWindow : Window
    {
        private readonly Services.LibraryService _libraryService = new Services.LibraryService();
        private string currentUsername;

        public MyBooksWindow(string username)
        {
            InitializeComponent();
            currentUsername = username;
            TitleTextBlock.Text = $"Cărțile mele - {currentUsername}";
            LoadMyBooks();
        }

        private void LoadMyBooks()
        {
            List<BorrowedBook> borrowedBooks = _libraryService.GetBorrowedBooks();
            var myBooks = borrowedBooks.Where(b => b.Username == currentUsername).ToList();

            MyBooksDataGrid.ItemsSource = null;
            MyBooksDataGrid.ItemsSource = myBooks;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            StudentWindow studentWindow = new StudentWindow(currentUsername);
            studentWindow.Show();
            this.Close();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadMyBooks();
        }
        private void ReturnBookButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = MyBooksDataGrid.SelectedItem as BorrowedBook;

            if (selectedItem == null)
            {
                MessageBox.Show("Te rog să selectezi o carte din listă pentru a o returna sau anula.", "Atenție", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Ești sigur că vrei să returnezi / anulezi rezervarea pentru această carte?",
                "Confirmare",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new LibraryContext())
                    {
                        
                        var borrowedRecord = db.BorrowedBooks.FirstOrDefault(b =>
                            b.Title == selectedItem.Title &&
                            b.Username == selectedItem.Username);

                        if (borrowedRecord != null)
                        {
                            db.BorrowedBooks.Remove(borrowedRecord);
                        }

                        var originalBook = db.Books.FirstOrDefault(b =>
                            b.Title == selectedItem.Title &&
                            b.ReservedBy == selectedItem.Username);

                        if (originalBook != null)
                        {
                            originalBook.Status = "Disponibil";
                            originalBook.ReservedBy = ""; 
                        }

                        db.SaveChanges();

                        MessageBox.Show("Cartea a fost returnată cu succes și este din nou disponibilă!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);

                        LoadMyBooks();
                    }
                }
                catch (System.Exception ex)
                {
                    string innerError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                    MessageBox.Show($"A apărut o problemă la actualizarea bazei de date: {innerError}", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}