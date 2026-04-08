using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Proiect
{
    public partial class MainWindow : Window
    {
        private readonly Services.LibraryService _libraryService = new Services.LibraryService();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password.Trim();
            ComboBoxItem selectedRoleItem = (ComboBoxItem)RoleComboBox.SelectedItem;
            string role = selectedRoleItem.Content.ToString().Trim().ToLower();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Completează username și parola.");
                return;
            }

            List<User> users = _libraryService.GetAllUsers();

            if (users.Any(u => u.Username == username))
            {
                MessageBox.Show("Acest username există deja.");
                return;
            }

            users.Add(new User { Username = username, Password = password, Role = role });
            _libraryService.SaveUsers(users);

            MessageBox.Show("Înregistrare reușită!");
            UsernameTextBox.Clear();
            PasswordBox.Clear();
            RoleComboBox.SelectedIndex = 0;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password.Trim();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Completează username și parola.");
                return;
            }

            List<User> users = _libraryService.GetAllUsers();
            User foundUser = users.FirstOrDefault(u => u.Username == username && u.Password == password);

            if (foundUser == null)
            {
                MessageBox.Show("Username sau parolă greșită.");
                return;
            }

            string role = foundUser.Role?.Trim().ToLower();

            if (role == "student")
            {
                StudentWindow studentWindow = new StudentWindow(foundUser.Username);
                studentWindow.Show();
                studentWindow.WindowState = WindowState.Normal;
                studentWindow.Activate();
                this.Hide();
            }
            else if (role == "angajat")
            {
                AngajatWindow angajatWindow = new AngajatWindow(foundUser.Username);
                angajatWindow.Show();
                angajatWindow.WindowState = WindowState.Normal;
                angajatWindow.Activate();
                this.Hide();
            }
            else
            {
                MessageBox.Show($"Rol necunoscut: {foundUser.Role}");
            }
        }
    }
}