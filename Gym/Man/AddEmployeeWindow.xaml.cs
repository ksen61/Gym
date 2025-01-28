using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Data.Entity.Validation;

namespace Gym
{
    /// <summary>
    /// Окно для добавления нового сотрудника в систему.
    /// Позволяет вводить данные пользователя, такие как логин, пароль, роль и тренера.
    /// После сохранения нового сотрудника в базе данных, окно закрывается и возвращается результат.
    /// </summary>
    public partial class AddEmployeeWindow : Window
    {
        private GymmEntities _context;

        /// <summary>
        /// Конструктор окна для добавления нового сотрудника.
        /// Инициализирует контекст базы данных и загружает список ролей и тренеров для выбора.
        /// </summary>
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

        /// <summary>
        /// Хеширует пароль с использованием алгоритма SHA256.
        /// </summary>
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

        /// <summary>
        /// Обрабатывает нажатие кнопки для добавления нового сотрудника.
        /// Проверяет заполнение всех полей, хеширует пароль и добавляет нового сотрудника в базу данных.
        /// В случае успеха отображает сообщение и закрывает окно.
        /// В случае ошибки сохраняет и отображает сообщения об ошибках валидации.
        /// </summary>
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

        /// <summary>
        /// Обрабатывает нажатие кнопки для отмены добавления сотрудника и закрывает окно.
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}