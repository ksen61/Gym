using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Data.Entity.Validation;

namespace Gym
{
    public partial class AddEmployeeWindow : Window
    {
        private GymmEntities _context;

        public AddEmployeeWindow()
        {
            InitializeComponent();
            _context = new GymmEntities();

            RoleComboBox.ItemsSource = _context.Roles.ToList();
            RoleComboBox.DisplayMemberPath = "RoleName";
            RoleComboBox.SelectedValuePath = "ID_Roles";

            TrainerComboBox.ItemsSource = _context.Trainers.ToList();
            TrainerComboBox.DisplayMemberPath = "Surname";
            TrainerComboBox.SelectedValuePath = "ID_Trainer";
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

                StringBuilder builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private void AddEmployeeButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(LoginTextBox.Text) || string.IsNullOrEmpty(PasswordBox.Password) || RoleComboBox.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string hashedPassword = HashPassword(PasswordBox.Password);

            var newUser = new UserAccounts
            {
                Login = LoginTextBox.Text,
                Password = hashedPassword,  
                Role_ID = (int)RoleComboBox.SelectedValue,
                Trainer_ID = TrainerComboBox.SelectedItem != null ? (int?)TrainerComboBox.SelectedValue : null
            };

            try
            {
                _context.UserAccounts.Add(newUser);
                _context.SaveChanges();

                MessageBox.Show("Сотрудник успешно добавлен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                this.DialogResult = true;
                this.Close();
            }
            catch (DbEntityValidationException ex)
            {
                StringBuilder errorMessages = new StringBuilder();
                foreach (var validationError in ex.EntityValidationErrors)
                {
                    foreach (var error in validationError.ValidationErrors)
                    {
                        errorMessages.AppendLine($"Property: {error.PropertyName}, Error: {error.ErrorMessage}");
                    }
                }

                MessageBox.Show($"Ошибка при сохранении данных:\n{errorMessages.ToString()}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}