using System.Collections.Generic;
using System.Windows;
using Proiect.Services; 
namespace Proiect
{
    public partial class AddBookWindow : Window
    {
        private readonly LibraryService _libraryService = new LibraryService();

        public AddBookWindow()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string title = TitleTextBox.Text.Trim();
            string author = AuthorTextBox.Text.Trim();
            string subject = SubjectTextBox.Text.Trim();

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(author) || string.IsNullOrEmpty(subject))
            {
                MessageBox.Show("Te rog să completezi toate câmpurile!", "Atenție", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Book newBook = new Book
            {
                Title = title,
                Author = author,
                Subject = subject,
                Status = "Disponibil",
                ReservedBy = "" 
            };

            _libraryService.SaveBooks(new List<Book> { newBook });

            MessageBox.Show("Cartea a fost adăugată cu succes!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}