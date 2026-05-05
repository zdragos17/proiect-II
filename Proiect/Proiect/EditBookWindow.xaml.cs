using Proiect.Data;
using System.Windows;
using System.Windows.Controls;

namespace Proiect
{
    public partial class EditBookWindow : Window
    {
        private Book _bookToEdit;

        public EditBookWindow(Book bookToEdit)
        {
            InitializeComponent();
            _bookToEdit = bookToEdit;

            TitleTextBox.Text = _bookToEdit.Title;
            AuthorTextBox.Text = _bookToEdit.Author;
            SubjectTextBox.Text = _bookToEdit.Subject;
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

            // Ne conectăm direct la baza de date pentru a actualiza cartea
            using (var db = new LibraryContext())
            {
                var bookInDb = db.Books.Find(_bookToEdit.Id);

                if (bookInDb != null)
                {
                    bookInDb.Title = title;
                    bookInDb.Author = author;
                    bookInDb.Subject = subject;

                    db.SaveChanges();
                    MessageBox.Show("Cartea a fost actualizată cu succes!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
