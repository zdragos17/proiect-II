using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Proiect
{
    public partial class AngajatWindow : Window
    {
        private readonly Services.LibraryService _libraryService = new Services.LibraryService();
        private string currentUsername;
        private List<User> users = new List<User>();

        public AngajatWindow(string username)
        {
            InitializeComponent();
            currentUsername = username;
            WelcomeTextBlock.Text = $"Bun venit, {currentUsername}!";

            users = _libraryService.GetAllUsers();
            ShowOnlyStudents();
        }

        private void ShowOnlyStudents()
        {
            var students = users.Where(u => u.Role != null && u.Role.ToLower() == "student")
                                .Select(u => new StudentUser
                                {
                                    Username = u.Username,
                                    LastName = u.LastName,       // Am adăugat maparea numelui
                                    FirstName = u.FirstName,     // Am adăugat maparea prenumelui
                                    Faculty = u.Faculty,         // Am adăugat maparea facultății
                                    MatriculationNumber = u.MatriculationNumber // Am adăugat maparea numărului matricol
                                })
                                .ToList();

            StudentsDataGrid.ItemsSource = null;
            StudentsDataGrid.ItemsSource = students;
        }

        private void SearchStudentButton_Click(object sender, RoutedEventArgs e)
        {
            string searchText = SearchStudentTextBox.Text.Trim().ToLower();
            var filteredStudents = users.Where(u => u.Role != null &&
                                                    u.Role.ToLower() == "student" &&
                                                    u.Username.ToLower().Contains(searchText))
                                        .Select(u => new StudentUser
                                        {
                                            Username = u.Username,
                                            LastName = u.LastName,
                                            FirstName = u.FirstName,
                                            Faculty = u.Faculty,
                                            MatriculationNumber = u.MatriculationNumber
                                        })
                                        .ToList();

            StudentsDataGrid.ItemsSource = null;
            StudentsDataGrid.ItemsSource = filteredStudents;
        }

        private void ShowAllStudentsButton_Click(object sender, RoutedEventArgs e)
        {
            SearchStudentTextBox.Clear();
            ShowOnlyStudents();
        }

        private void OpenStudentFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenSelectedStudentFile();
        }

        private void StudentsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenSelectedStudentFile();
        }

        private void OpenSelectedStudentFile()
        {
            StudentUser selectedStudent = StudentsDataGrid.SelectedItem as StudentUser;
            if (selectedStudent == null)
            {
                MessageBox.Show("Selectează un student.");
                return;
            }

            StudentLoansWindow studentLoansWindow = new StudentLoansWindow(selectedStudent.Username, currentUsername);
            studentLoansWindow.Show();
            this.Close();
        }

        private void SearchStudentTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string searchText = SearchStudentTextBox.Text.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                ShowOnlyStudents();
                return;
            }

            var filteredStudents = users
                .Where(u => u.Role != null &&
                            u.Role.ToLower() == "student" &&
                            u.Username.ToLower().Contains(searchText))
                .Select(u => new StudentUser
                {
                    Username = u.Username,
                    LastName = u.LastName,
                    FirstName = u.FirstName,
                    Faculty = u.Faculty,
                    MatriculationNumber = u.MatriculationNumber
                })
                .ToList();

            StudentsDataGrid.ItemsSource = null;
            StudentsDataGrid.ItemsSource = filteredStudents;
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private async void DownloadApiBooks_Click(object sender, RoutedEventArgs e)
        {
            await _libraryService.SeedDatabaseWithApiBooks();
        }
        private void AddBookManual_Click(object sender, RoutedEventArgs e)
        {
            AddBookWindow addWindow = new AddBookWindow();
            addWindow.Owner = this;
            addWindow.ShowDialog();
        }
    }
}