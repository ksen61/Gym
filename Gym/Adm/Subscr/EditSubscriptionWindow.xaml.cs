using System;
using System.Linq;
using System.Windows;

namespace Gym
{
    public partial class EditSubscriptionWindow : Window
    {
        private GymmEntities context;
        private int subscriptionId;

        public EditSubscriptionWindow(GymmEntities context, int subscriptionId)
        {
            InitializeComponent();

            try
            {
                this.context = context;
                this.subscriptionId = subscriptionId;

                var subscription = context.Subscriptions.FirstOrDefault(s => s.ID_Subscriptions == subscriptionId);
                if (subscription != null)
                {
                    TypeTextBox.Text = subscription.Type;
                    PriceTextBox.Text = subscription.Price.ToString("F2");
                    AvailableServicesTextBox.Text = subscription.AvailableServices;
                    MaxParticipantsTextBox.Text = subscription.MaxParticipants.ToString();
                    RegisteredClientsTextBox.Text = subscription.RegisteredClients.ToString();
                    DurationDaysTextBox.Text = subscription.DurationDays.ToString(); 
                }
                else
                {
                    MessageBox.Show("Абонемент не найден", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var subscription = context.Subscriptions.FirstOrDefault(s => s.ID_Subscriptions == subscriptionId);
                if (subscription != null)
                {
                    if (string.IsNullOrWhiteSpace(TypeTextBox.Text))
                    {
                        MessageBox.Show("Введите тип абонемента", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (!System.Text.RegularExpressions.Regex.IsMatch(TypeTextBox.Text, @"^[A-Za-zА-Яа-я]+$"))
                    {
                        MessageBox.Show("Тип абонемента может содержать только буквы", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (!decimal.TryParse(PriceTextBox.Text, out decimal price) || price <= 0)
                    {
                        MessageBox.Show("Введите корректную цену", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(AvailableServicesTextBox.Text))
                    {
                        MessageBox.Show("Введите список доступных услуг", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (!System.Text.RegularExpressions.Regex.IsMatch(AvailableServicesTextBox.Text, @"^[A-Za-zА-Яа-я,\s]+$"))
                    {
                        MessageBox.Show("Список услуг может содержать только буквы, запятые и пробелы", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (!int.TryParse(MaxParticipantsTextBox.Text, out int maxParticipants) || maxParticipants <= 0)
                    {
                        MessageBox.Show("Введите корректное количество участников", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (!int.TryParse(RegisteredClientsTextBox.Text, out int registeredClients) || registeredClients < 0 || registeredClients > maxParticipants)
                    {
                        MessageBox.Show("Введите корректное количество зарегистрированных клиентов", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (!int.TryParse(DurationDaysTextBox.Text, out int durationDays) || durationDays <= 0)
                    {
                        MessageBox.Show("Введите корректную длительность абонемента", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    subscription.Type = TypeTextBox.Text;
                    subscription.Price = price;
                    subscription.AvailableServices = AvailableServicesTextBox.Text;
                    subscription.MaxParticipants = maxParticipants;
                    subscription.RegisteredClients = registeredClients;
                    subscription.DurationDays = durationDays; 

                    context.SaveChanges();
                    MessageBox.Show("Данные успешно сохранены", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Абонемент не найден", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}