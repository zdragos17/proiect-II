using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;

namespace Proiect
{
    public partial class MyBooksWindow : Window
    {
        private readonly string borrowedBooksFilePath = "borrowedBooks.json";
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
            List<BorrowedBook> borrowedBooks = new List<BorrowedBook>();

            if (File.Exists(borrowedBooksFilePath))
            {
                string json = File.ReadAllText(borrowedBooksFilePath);

                if (!string.IsNullOrWhiteSpace(json))
                {
                    borrowedBooks = JsonSerializer.Deserialize<List<BorrowedBook>>(json) ?? new List<BorrowedBook>();
                }
            }

            var myBooks = borrowedBooks
                .Where(b => b.Username == currentUsername)
                .ToList();

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
    }
}