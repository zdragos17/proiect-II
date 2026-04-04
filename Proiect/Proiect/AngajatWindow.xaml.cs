using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;

namespace Proiect
{
    public partial class AngajatWindow : Window
    {
        private readonly string usersFilePath = "users.json";
        private string currentUsername;
        private List<User> users = new List<User>();

        public AngajatWindow(string username)
        {
            InitializeComponent();
            currentUsername = username;
            WelcomeTextBlock.Text = $"Bun venit, {currentUsername}!";

            LoadUsers();
            ShowOnlyStudents();
        }

        private void LoadUsers()
        {
            if (!File.Exists(usersFilePath))
            {
                users = new List<User>();
                return;
            }

            string json = File.ReadAllText(usersFilePath);

            if (string.IsNullOrWhiteSpace(json))
            {
                users = new List<User>();
                return;
            }

            users = JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
        }

        private void ShowOnlyStudents()
        {
            var students = users
                .Where(u => u.Role != null && u.Role.ToLower() == "student")
                .Select(u => new StudentUser { Username = u.Username })
                .ToList();

            StudentsDataGrid.ItemsSource = null;
            StudentsDataGrid.ItemsSource = students;
        }

        private void SearchStudentButton_Click(object sender, RoutedEventArgs e)
        {
            string searchText = SearchStudentTextBox.Text.Trim().ToLower();

            var filteredStudents = users
                .Where(u => u.Role != null &&
                            u.Role.ToLower() == "student" &&
                            u.Username.ToLower().Contains(searchText))
                .Select(u => new StudentUser { Username = u.Username })
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

            StudentLoansWindow studentLoansWindow = new StudentLoansWindow(selectedStudent.Username);
            studentLoansWindow.Show();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
    }
}