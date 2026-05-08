using Proiect.Data;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Proiect
{
    public partial class ManageBooksWindow : Window
    {
        private string _currentUsername;

        // 1. Am adăugat lista care ține toate cărțile în memorie
        private List<Book> _allBooks;

        public ManageBooksWindow(string currentUsername)
        {
            InitializeComponent();
            _currentUsername = currentUsername;
            LoadBooks();
        }

        private void LoadBooks()
        {
            using (var db = new LibraryContext())
            {
                // 2. Acum salvăm cărțile în lista noastră prima dată
                _allBooks = db.Books.ToList();

                // Apoi le afișăm pe ecran
                AllBooksDataGrid.ItemsSource = _allBooks;
            }
        }

        private void EditBookButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedBook = AllBooksDataGrid.SelectedItem as Book;

            if (selectedBook == null)
            {
                MessageBox.Show("Te rog să selectezi o carte din tabel pentru a o edita.", "Atenție", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Deschidem pop-up-ul și îi dăm cartea
            EditBookWindow editWindow = new EditBookWindow(selectedBook);
            editWindow.Owner = this;
            editWindow.ShowDialog();

            // După ce se închide pop-up-ul, reîncărcăm tabelul
            LoadBooks();
        }

        private void DeleteBookButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedBook = AllBooksDataGrid.SelectedItem as Book;

            if (selectedBook == null)
            {
                MessageBox.Show("Te rog să selectezi o carte din tabel pentru a o șterge.", "Atenție", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"Sigur vrei să ștergi cartea '{selectedBook.Title}'?", "Confirmare", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                using (var db = new LibraryContext())
                {
                    var bookInDb = db.Books.Find(selectedBook.Id);
                    if (bookInDb != null)
                    {
                        db.Books.Remove(bookInDb);
                        db.SaveChanges();
                        MessageBox.Show("Cartea a fost ștearsă cu succes!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadBooks();
                    }
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            AngajatWindow angajatWindow = new AngajatWindow(_currentUsername);
            angajatWindow.Show();
            this.Close();
        }

        private void AllBooksDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedBook = AllBooksDataGrid.SelectedItem as Book;
            if (selectedBook != null)
            {
                EmployeeBookActionWindow actionWindow = new EmployeeBookActionWindow(selectedBook);
                actionWindow.Owner = this;
                actionWindow.ShowDialog();

                LoadBooks();
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_allBooks == null) return;

            string searchText = SearchTextBox.Text.ToLower();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                AllBooksDataGrid.ItemsSource = _allBooks;
            }
            else
            {
                var filteredList = _allBooks.Where(b =>
                    (b.Title != null && b.Title.ToLower().Contains(searchText)) ||
                    (b.Author != null && b.Author.ToLower().Contains(searchText))
                ).ToList();

                AllBooksDataGrid.ItemsSource = filteredList;
            }
        }
    }
}