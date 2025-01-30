using System;
using System.Linq;
using System.Windows;
using System.Security.Cryptography;
using System.Text;
using System.Timers;

namespace Gym
{

    /// <summary>
    /// Класс окна авторизации пользователя, где осуществляется проверка логина и пароля для входа в систему.
    /// В зависимости от роли пользователя, открывается соответствующее окно.
    /// </summary>
    public partial class LoginWindow : Window
    {
        private GymmEntities context = new GymmEntities();
        private int failedAttempts = 0;
        private int failedPasswordAttempts = 0;
        private const int maxAttempts = 3;
        private const int maxPasswordAttempts = 5;
        private Timer lockoutTimer;

        /// <summary>
        /// Инициализирует окно авторизации.
        /// </summary>
        public LoginWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Обрабатывает нажатие кнопки входа.
        /// Проверяет логин и пароль, блокирует вход при превышении попыток.
        /// </summary>
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (failedAttempts >= maxAttempts || failedPasswordAttempts >= maxPasswordAttempts)
            {
                MessageBox.Show("Попытки входа исчерпаны. Подождите 1 минуту.", "Блокировка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string login = LoginBox.Text.Trim();
            string password = PasswordBox.Password.Trim();
            string hashedPassword = HashPassword(password);

            var user = context.UserAccounts
                .AsEnumerable()
                .FirstOrDefault(u => u.Login == login);

            if (user != null)
            {
                if (user.Password == hashedPassword)
                {
                    var role = context.Roles.FirstOrDefault(r => r.ID_Roles == user.Role_ID);
                    if (role != null)
                    {
                        MessageBox.Show("Авторизация успешна!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        OpenUserWindow(user.ID_UserAccounts, role.RoleName);
                    }
                    else
                    {
                        MessageBox.Show("Не удалось найти роль пользователя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    failedPasswordAttempts = 0;
                }
                else
                {
                    failedPasswordAttempts++;
                    MessageBox.Show("Неверный пароль!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    if (failedPasswordAttempts >= maxPasswordAttempts)
                    {
                        MessageBox.Show("Попробуйте позже.", "Блокировка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        LockLoginFields();
                    }
                }
            }
            else
            {
                failedAttempts++;
                MessageBox.Show("Неверный логин или пароль!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                if (failedAttempts >= maxAttempts)
                {
                    LockLoginFields();
                }
            }
        }

        /// <summary>
        /// Блокирует поля ввода после превышения попыток входа.
        /// </summary>
        private void LockLoginFields()
        {
            LoginBox.IsEnabled = false;
            PasswordBox.IsEnabled = false;
            LoginButton.IsEnabled = false;

            lockoutTimer = new Timer(60000); // 1 минута
            lockoutTimer.Elapsed += UnlockLoginFields;
            lockoutTimer.AutoReset = false;
            lockoutTimer.Start();
        }

        /// <summary>
        /// Разблокирует поля ввода после завершения таймера блокировки.
        /// </summary>
        private void UnlockLoginFields(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                LoginBox.IsEnabled = true;
                PasswordBox.IsEnabled = true;
                LoginButton.IsEnabled = true;
                failedAttempts = 0;
                failedPasswordAttempts = 0;
            });
        }

        /// <summary>
        /// Хеширует пароль с использованием SHA-256.
        /// </summary>
        private static string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        /// <summary>
        /// Открывает соответствующее окно в зависимости от роли пользователя.
        /// </summary>
        private void OpenUserWindow(int userId, string userRole)
        {
            if (userRole == "Тренер")
            {
                new TrainerssWindow(userId).Show();
            }
            else if (userRole == "Руководитель")
            {
                new ManagersWindow().Show();
            }
            else
            {
                new MainWindow(userRole).Show();
            }
            this.Close();
        }

        /// <summary>
        /// Активирует кнопку входа только при наличии текста в полях логина и пароля.
        /// </summary>
        private void InputFieldsChanged(object sender, RoutedEventArgs e)
        {
            LoginButton.IsEnabled = !string.IsNullOrWhiteSpace(LoginBox.Text) && !string.IsNullOrWhiteSpace(PasswordBox.Password);
        }
    }
}