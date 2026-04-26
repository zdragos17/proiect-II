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
            
            if (MyBooksDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Te rog să selectezi o carte din listă pentru a o returna sau anula.", "Atenție", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show("Cererea a fost înregistrată! (Necesită integrare cu baza de date)", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}