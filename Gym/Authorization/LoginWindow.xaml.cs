using System.Linq;
using System.Windows;
using System.Security.Cryptography;
using System.Text;


namespace Gym
{
    public partial class LoginWindow : Window
    {
        private GymmEntities context = new GymmEntities();

        public LoginWindow()
        {
            InitializeComponent();
        }

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