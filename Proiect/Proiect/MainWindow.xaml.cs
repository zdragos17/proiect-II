using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Proiect
{
    public partial class MainWindow : Window
    {
        private readonly Services.LibraryService _libraryService = new Services.LibraryService();

        private bool IsValidEmployeeCode(string code)
        {

            return System.Text.RegularExpressions.Regex.IsMatch(
                code,
                @"^UTCN-ANG-\d{3}$"
            );
        }


        public MainWindow()
        {
            InitializeComponent();

            StartPanel.Visibility = Visibility.Visible;
            LoginPanel.Visibility = Visibility.Collapsed;
            RegisterPanel.Visibility = Visibility.Collapsed;
        }

        private void ShowLoginForm_Click(object sender, RoutedEventArgs e)
        {
            StartPanel.Visibility = Visibility.Collapsed;
            LoginPanel.Visibility = Visibility.Visible;
            RegisterPanel.Visibility = Visibility.Collapsed;
        }

        private void ShowRegisterForm_Click(object sender, RoutedEventArgs e)
        {
            StartPanel.Visibility = Visibility.Collapsed;
            LoginPanel.Visibility = Visibility.Collapsed;
            RegisterPanel.Visibility = Visibility.Visible;

            RegisterRoleComboBox_SelectionChanged(null, null);
        }

        private void BackToStart_Click(object sender, RoutedEventArgs e)
        {
            StartPanel.Visibility = Visibility.Visible;
            LoginPanel.Visibility = Visibility.Collapsed;
            RegisterPanel.Visibility = Visibility.Collapsed;
        }

        private void RegisterRoleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FacultyLabel == null ||
                FacultyComboBox == null ||
                MatriculationNumberLabel == null ||
                MatriculationNumberTextBox == null ||
                EmployeeCodeLabel == null ||
                EmployeeCodePasswordBox == null)
                return;

            ComboBoxItem selectedRoleItem = RegisterRoleComboBox.SelectedItem as ComboBoxItem;
            string role = selectedRoleItem?.Content.ToString().Trim().ToLower();

            if (role == "student")
            {
                FacultyLabel.Visibility = Visibility.Visible;
                FacultyComboBox.Visibility = Visibility.Visible;
                MatriculationNumberLabel.Visibility = Visibility.Visible;
                MatriculationNumberTextBox.Visibility = Visibility.Visible;

                EmployeeCodeLabel.Visibility = Visibility.Collapsed;
                EmployeeCodePasswordBox.Visibility = Visibility.Collapsed;
                EmployeeCodePasswordBox.Clear();
            }
            else if (role == "angajat")
            {
                FacultyLabel.Visibility = Visibility.Collapsed;
                FacultyComboBox.Visibility = Visibility.Collapsed;
                MatriculationNumberLabel.Visibility = Visibility.Collapsed;
                MatriculationNumberTextBox.Visibility = Visibility.Collapsed;

                EmployeeCodeLabel.Visibility = Visibility.Visible;
                EmployeeCodePasswordBox.Visibility = Visibility.Visible;
            }
        }

        private void RegisterNameFields_TextChanged(object sender, TextChangedEventArgs e)
        {
            string firstName = NormalizeText(FirstNameTextBox.Text.Trim());
            string lastName = NormalizeText(LastNameTextBox.Text.Trim());

            string baseUsername = $"{firstName}{lastName}".ToLower();

            if (string.IsNullOrWhiteSpace(baseUsername))
            {
                RegisterUsernameTextBox.Text = "Se va genera automat";
            }
            else
            {
                RegisterUsernameTextBox.Text = baseUsername;
            }
        }

        private string NormalizeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            string normalized = text.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();

            foreach (char c in normalized)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(c);

                if (uc != UnicodeCategory.NonSpacingMark && char.IsLetterOrDigit(c))
                {
                    sb.Append(c);
                }
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        private string GenerateUniqueUsername(string firstName, string lastName, List<User> users)
        {
            string baseUsername = $"{NormalizeText(firstName)}{NormalizeText(lastName)}".ToLower();

            string username = baseUsername;
            int counter = 2;

            while (users.Any(u => u.Username.ToLower() == username))
            {
                username = $"{baseUsername}{counter}";
                counter++;
            }

            return username;
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string firstName = FirstNameTextBox.Text.Trim();
            string lastName = LastNameTextBox.Text.Trim();
            string password = RegisterPasswordBox.Password.Trim();
            string confirmPassword = ConfirmPasswordBox.Password.Trim();

            ComboBoxItem selectedRoleItem = (ComboBoxItem)RegisterRoleComboBox.SelectedItem;
            string role = selectedRoleItem.Content.ToString().Trim().ToLower();

            string faculty = "";
            string matriculationNumber = "";
            string employeeCode = "";

            if (string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                MessageBox.Show("Completează toate câmpurile obligatorii.");
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Parolele nu coincid.");
                return;
            }

            if (role == "student")
            {
                if (FacultyComboBox.SelectedIndex <= 0)
                {
                    MessageBox.Show("Selectează facultatea.");
                    return;
                }

                ComboBoxItem selectedFacultyItem = (ComboBoxItem)FacultyComboBox.SelectedItem;
                faculty = selectedFacultyItem.Content.ToString().Trim();

                matriculationNumber = MatriculationNumberTextBox.Text.Trim();

                if (string.IsNullOrWhiteSpace(matriculationNumber))
                {
                    MessageBox.Show("Introdu numărul matricol.");
                    return;
                }
            }

            if (role == "angajat")
            {
                employeeCode = EmployeeCodePasswordBox.Password.Trim();

                if (string.IsNullOrWhiteSpace(employeeCode))
                {
                    MessageBox.Show("Introdu codul de angajat.");
                    return;
                }

                if (!IsValidEmployeeCode(employeeCode))
                {
                    MessageBox.Show("Cod angajat invalid. Format corect: UTCN-ANG-001");
                    return;
                }
            }

            List<User> users = _libraryService.GetAllUsers();

            if (role == "student" &&
                users.Any(u => u.MatriculationNumber == matriculationNumber))
            {
                MessageBox.Show("Există deja un cont pentru acest număr matricol.");
                return;
            }

            if (role == "angajat" &&
                users.Any(u => u.EmployeeCode == employeeCode))
            {
                MessageBox.Show("Există deja un cont creat cu acest cod de angajat.");
                return;
            }

            string username = GenerateUniqueUsername(firstName, lastName, users);
            RegisterUsernameTextBox.Text = username;

            User newUser = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Faculty = faculty,
                MatriculationNumber = matriculationNumber,
                EmployeeCode = employeeCode,
                Username = username,
                Password = password,
                Role = role
            };

            users.Add(newUser);
            _libraryService.SaveUsers(users);

            MessageBox.Show($"Cont creat cu succes! Username-ul tău este: {username}");

            OpenMenuByRole(newUser);
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = LoginUsernameTextBox.Text.Trim().ToLower();
            string password = LoginPasswordBox.Password.Trim();

            ComboBoxItem selectedRoleItem = (ComboBoxItem)LoginRoleComboBox.SelectedItem;
            string selectedRole = selectedRoleItem.Content.ToString().Trim().ToLower();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Completează username și parola.");
                return;
            }

            List<User> users = _libraryService.GetAllUsers();

            User foundUser = users.FirstOrDefault(u =>
                u.Username.ToLower() == username &&
                u.Password == password &&
                u.Role.ToLower() == selectedRole);

            if (foundUser == null)
            {
                MessageBox.Show("Date greșite sau rol selectat incorect.");
                return;
            }

            OpenMenuByRole(foundUser);
        }

        private void OpenMenuByRole(User user)
        {
            string role = user.Role?.Trim().ToLower();

            if (role == "student")
            {
                StudentWindow studentWindow = new StudentWindow(user.Username);
                studentWindow.Show();
                this.Close();
            }
            else if (role == "angajat")
            {
                AngajatWindow angajatWindow = new AngajatWindow(user.Username);
                angajatWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show($"Rol necunoscut: {user.Role}");
            }
        }
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
           
            Application.Current.Shutdown();
        }
    }
}