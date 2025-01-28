using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

/// <summary>
/// Окно добавления нового абонемента в систему. Позволяет пользователю ввести данные абонемента,
/// после чего сохраняет информацию в базе данных. Также включает в себя валидацию введённых данных и обработку ошибок.
/// </summary>
namespace Gym
{
    public partial class AddSubscriptionWindow : Window
    {
        private GymmEntities context;

        /// <summary>
        /// Конструктор окна для добавления абонемента. Инициализирует окно и устанавливает контекст базы данных.
        /// </summary>
        public AddSubscriptionWindow(GymmEntities dbContext)
        {
            InitializeComponent();
            context = dbContext;
        }

        /// <summary>
        /// Обработчик события для кнопки отмены. Закрывает окно добавления абонемента без сохранения изменений.
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Обработчик события для кнопки добавления абонемента. Проверяет введенные данные, создает новый абонемент
        /// и сохраняет его в базе данных. Если данные некорректны, выводит сообщения об ошибке.
        /// </summary>
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(TypeTextBox.Text) ||
                    string.IsNullOrWhiteSpace(PriceTextBox.Text) ||
                    string.IsNullOrWhiteSpace(AvailableServicesTextBox.Text) ||
                    string.IsNullOrWhiteSpace(MaxParticipantsTextBox.Text) ||
                    string.IsNullOrWhiteSpace(RegisteredClientsTextBox.Text) ||
                    string.IsNullOrWhiteSpace(DurationDaysTextBox.Text))  
                {
                    MessageBox.Show("Все поля должны быть заполнены.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }


                string type = TypeTextBox.Text;
                if (!Regex.IsMatch(type, @"^[a-zA-Zа-яА-ЯёЁ ]+$")) 
                {
                    MessageBox.Show("Поле 'Тип абонемента' может содержать только буквы.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                decimal price;
                int maxParticipants, registeredClients, durationDays;

                if (!decimal.TryParse(PriceTextBox.Text, out price) || price <= 0)
                {
                    MessageBox.Show("Неверный формат цены. Цена должна быть положительным числом.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!int.TryParse(MaxParticipantsTextBox.Text, out maxParticipants) || maxParticipants <= 0)
                {
                    MessageBox.Show("Неверный формат максимального числа участников. Значение должно быть положительным числом.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!int.TryParse(RegisteredClientsTextBox.Text, out registeredClients) || registeredClients < 0)
                {
                    MessageBox.Show("Неверный формат числа зарегистрированных клиентов. Значение должно быть неотрицательным числом.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!int.TryParse(DurationDaysTextBox.Text, out durationDays) || durationDays <= 0)
                {
                    MessageBox.Show("Неверный формат длительности. Значение должно быть положительным числом.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (registeredClients > maxParticipants)
                {
                    MessageBox.Show("Количество зарегистрированных клиентов не может превышать максимальное количество участников.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string availableServices = AvailableServicesTextBox.Text;
                string pattern = @"^[a-zA-Zа-яА-ЯёЁ, ]*$"; 
                if (!Regex.IsMatch(availableServices, pattern))
                {
                    MessageBox.Show("Поле 'Дополнительные услуги' может содержать только буквы и запятые.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var newSubscription = new Subscriptions
                {
                    Type = TypeTextBox.Text,
                    Price = price,
                    AvailableServices = availableServices,
                    MaxParticipants = maxParticipants,
                    RegisteredClients = registeredClients,
                    DurationDays = durationDays 
                };

                context.Subscriptions.Add(newSubscription);
                context.SaveChanges();

                MessageBox.Show("Абонемент успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении абонемента: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}