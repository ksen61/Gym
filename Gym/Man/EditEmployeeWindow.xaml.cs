using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Data.Entity.Validation;
using System.Collections.ObjectModel;

namespace Gym
{
    public partial class EditEmployeeWindow : Window
    {
        private GymmEntities _context;
        private UserAccounts _selectedUser;
        private ObservableCollection<UserAccounts> _employees; 

        public EditEmployeeWindow(int userId, ObservableCollection<UserAccounts> employees)
        {
            InitializeComponent();
            _context = new GymmEntities();
            _employees = employees; 

            RoleComboBox.ItemsSource = _context.Roles.ToList();
            RoleComboBox.DisplayMemberPath = "RoleName";
            RoleComboBox.SelectedValuePath = "ID_Roles";

            TrainerComboBox.ItemsSource = _context.Trainers.ToList();
            TrainerComboBox.DisplayMemberPath = "Surname";
            TrainerComboBox.SelectedValuePath = "ID_Trainer";

            _selectedUser = _context.UserAccounts.FirstOrDefault(u => u.ID_UserAccounts == userId);
            if (_selectedUser != null)
            {
                LoginTextBox.Text = _selectedUser.Login;
                PasswordBox.Password = _selectedUser.Password;
                RoleComboBox.SelectedValue = _selectedUser.Role_ID;
                TrainerComboBox.SelectedValue = _selectedUser.Trainer_ID;
            }
        }


        private void EditEmployeeButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(LoginTextBox.Text) || string.IsNullOrEmpty(PasswordBox.Password) || RoleComboBox.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string hashedPassword = HashPassword(PasswordBox.Password);

            _selectedUser.Login = LoginTextBox.Text;
            _selectedUser.Password = hashedPassword;
            _selectedUser.Role_ID = (int)RoleComboBox.SelectedValue;  // Роль
            _selectedUser.Trainer_ID = TrainerComboBox.SelectedItem != null ? (int?)TrainerComboBox.SelectedValue : null;  // Тренер

            try
            {
                _context.SaveChanges();

                var employeeToUpdate = _employees.FirstOrDefault(employee => employee.ID_UserAccounts == _selectedUser.ID_UserAccounts);
                if (employeeToUpdate != null)
                {
                    employeeToUpdate.Login = _selectedUser.Login;
                    employeeToUpdate.Password = _selectedUser.Password;
                    employeeToUpdate.Role_ID = _selectedUser.Role_ID;
                    employeeToUpdate.Trainer_ID = _selectedUser.Trainer_ID;
                }

                MessageBox.Show("Данные сотрудника успешно обновлены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

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
    }
}