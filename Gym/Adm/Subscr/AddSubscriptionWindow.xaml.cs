using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace Gym
{
    public partial class AddSubscriptionWindow : Window
    {
        private GymmEntities context;

        public AddSubscriptionWindow(GymmEntities dbContext)
        {
            InitializeComponent();
            context = dbContext;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

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