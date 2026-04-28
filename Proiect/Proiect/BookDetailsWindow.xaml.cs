using System.Windows;
using System.Windows.Media;

namespace Proiect
{
    public partial class BookDetailsWindow : Window
    {
        // Constructorul primește obiectul Book selectat din tabel
        public BookDetailsWindow(Book selectedBook)
        {
            InitializeComponent();

            // Populăm textele. Aici nu se mai taie nimic!
            TitleTextBlock.Text = selectedBook.Title;
            AuthorTextBlock.Text = selectedBook.Author;
            SubjectTextBlock.Text = selectedBook.Subject;
            StatusTextBlock.Text = selectedBook.Status;

            // Punem puțină culoare pe status
            if (selectedBook.Status.ToLower().Contains("disponibil"))
            {
                StatusTextBlock.Foreground = new SolidColorBrush(Colors.MediumSeaGreen);
            }
            else
            {
                StatusTextBlock.Foreground = new SolidColorBrush(Colors.Crimson);
            }
        }

        // Închide pop-up-ul
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}