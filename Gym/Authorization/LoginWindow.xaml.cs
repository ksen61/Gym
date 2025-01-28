using System.Linq;
using System.Windows;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Класс окна авторизации пользователя, где осуществляется проверка логина и пароля для входа в систему.
/// В зависимости от роли пользователя, открывается соответствующее окно.
/// </summary>
namespace Gym
{
    public partial class LoginWindow : Window
    {
        private GymmEntities context = new GymmEntities();

        /// <summary>
        /// Конструктор окна авторизации. Инициализирует компоненты интерфейса.
        /// </summary>
        public LoginWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Обработчик клика по кнопке входа. Проверяет логин и пароль пользователя, осуществляет хеширование пароля
        /// и находит пользователя в базе данных. В зависимости от роли пользователя открывает соответствующее окно.
        /// Если данные некорректны, выводит ошибку.
        /// </summary>
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginBox.Text.Trim();
            string password = PasswordBox.Password.Trim();

            string hashedPassword = HashPassword(password);

            var user = context.UserAccounts
                .AsEnumerable()
                .FirstOrDefault(u => u.Login == login && u.Password == hashedPassword);

            if (user != null)
            {
                var role = context.Roles.FirstOrDefault(r => r.ID_Roles == user.Role_ID);

                if (role != null)
                {
                    MessageBox.Show("Авторизация успешна!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    string userRole = role.RoleName;

                    if (userRole == "Тренер")
                    {
                        TrainerssWindow trainerWindow = new TrainerssWindow(user.ID_UserAccounts);  
                        trainerWindow.Show();
                    }
                    else if (userRole == "Руководитель")
                    {
                        ManagersWindow managerWindow = new ManagersWindow();
                        managerWindow.Show();
                    }
                    else
                    {
                        MainWindow mainWindow = new MainWindow(userRole);
                        mainWindow.Show();
                    }

                    this.Close();
                }
                else
                {
                    MessageBox.Show("Не удалось найти роль пользователя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Хеширует пароль пользователя с использованием алгоритма SHA-256.
        /// </summary>
        public static string HashPassword(string password)
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
    }
}