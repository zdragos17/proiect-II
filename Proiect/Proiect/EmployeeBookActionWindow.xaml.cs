using Proiect.Data;
using System.Windows;
using System.Windows.Controls;

namespace Proiect
{
    public partial class EmployeeBookActionWindow : Window
    {
        private Book _currentBook;

        public EmployeeBookActionWindow(Book book)
        {
            InitializeComponent();
            _currentBook = book;
            LoadBookDetails();
        }

        private void LoadBookDetails()
        {
            TitleTextBlock.Text = _currentBook.Title;
            AuthorTextBlock.Text = _currentBook.Author;
            StatusTextBlock.Text = _currentBook.Status;

            // Logica: Dacă e disponibilă, lăsăm angajatul să completeze numele. 
            // Dacă nu, îi arătăm cine o are și blocăm căsuțele.
            if (_currentBook.Status.ToLower().Contains("disponibil"))
            {
                StudentNameTextBox.Text = "";
                StudentNameTextBox.IsReadOnly = false;

                ActionLabel.Visibility = Visibility.Visible;
                ActionComboBox.Visibility = Visibility.Visible;

                AssignButton.Visibility = Visibility.Visible;
                ReturnButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                StudentNameTextBox.Text = _currentBook.ReservedBy;
                StudentNameTextBox.IsReadOnly = true; // Nu o poate modifica direct, trebuie returnată

                ActionLabel.Visibility = Visibility.Collapsed;
                ActionComboBox.Visibility = Visibility.Collapsed;

                AssignButton.Visibility = Visibility.Collapsed;
                ReturnButton.Visibility = Visibility.Visible; // Apare butonul verde de eliberare
            }
        }

        // Când angajatul dă cartea unui student (cu cont sau adăugat manual din tastatură)
        private void AssignButton_Click(object sender, RoutedEventArgs e)
        {
            string studentName = StudentNameTextBox.Text.Trim();

            if (string.IsNullOrEmpty(studentName))
            {
                MessageBox.Show("Te rog să introduci numele studentului!", "Atenție", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Luăm valoarea din ComboBox (Împrumutată sau Rezervată)
            string actionType = (ActionComboBox.SelectedItem as ComboBoxItem).Content.ToString();

            using (var db = new LibraryContext())
            {
                var bookInDb = db.Books.Find(_currentBook.Id);
                if (bookInDb != null)
                {
                    bookInDb.Status = actionType;
                    bookInDb.ReservedBy = studentName; // Poate fi orice string, BD-ul o acceptă
                    db.SaveChanges();

                    MessageBox.Show($"Cartea a fost marcată ca '{actionType}' pentru {studentName}!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
            }
        }

        // Când angajatul primește cartea înapoi fizic
        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            using (var db = new LibraryContext())
            {
                var bookInDb = db.Books.Find(_currentBook.Id);
                if (bookInDb != null)
                {
                    bookInDb.Status = "Disponibil";
                    bookInDb.ReservedBy = "";
                    db.SaveChanges();

                    MessageBox.Show("Cartea a fost eliberată și a redevenit disponibilă!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}