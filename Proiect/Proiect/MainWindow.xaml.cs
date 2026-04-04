using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace Proiect
{
    public partial class MainWindow : Window
    {
        private readonly string filePath = "users.json";

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

            List<User> users = LoadUsers();

            bool userExists = users.Any(u => u.Username == username);
            if (userExists)
            {
                MessageBox.Show("Acest username există deja.");
                return;
            }

            users.Add(new User
            {
                Username = username,
                Password = password,
                Role = role
            });

            SaveUsers(users);

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

            List<User> users = LoadUsers();

            User foundUser = users.FirstOrDefault(u => u.Username == username && u.Password == password);

            if (foundUser == null)
            {
                MessageBox.Show("Username sau parolă greșită.");
                return;
            }

            string role = foundUser.Role?.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(role))
            {
                MessageBox.Show("Acest utilizator nu are rol setat.");
                return;
            }

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

        private List<User> LoadUsers()
        {
            if (!File.Exists(filePath))
                return new List<User>();

            string json = File.ReadAllText(filePath);

            if (string.IsNullOrWhiteSpace(json))
                return new List<User>();

            return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
        }

        private void SaveUsers(List<User> users)
        {
            string json = JsonSerializer.Serialize(users, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(filePath, json);
        }
    }
}