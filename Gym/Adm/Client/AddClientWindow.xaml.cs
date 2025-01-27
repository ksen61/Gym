using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Gym
{
    public partial class AddClientWindow : Window
    {
        private GymmEntities context;

        public AddClientWindow(GymmEntities dbContext)
        {
            InitializeComponent();
            context = dbContext;

            SubscriptionComboBox.ItemsSource = context.Subscriptions.ToList();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateFields())
            {
                return;
            }

            try
            {
                var selectedSubscription = (Subscriptions)SubscriptionComboBox.SelectedItem;

                DateTime endDate = DateTime.Now.AddDays(selectedSubscription.DurationDays);

                var selectedStatus = ((ComboBoxItem)SubscriptionStatusComboBox.SelectedItem).Content.ToString();

                var newClient = new Clients
                {
                    Surname = SurnameTextBox.Text.Trim(),
                    Firstname = FirstnameTextBox.Text.Trim(),
                    Middlename = MiddlenameTextBox.Text.Trim(),
                    BirthDate = BirthDatePicker.SelectedDate.Value,
                    Phone = PhoneTextBox.Text.Trim(),
                    Email = EmailTextBox.Text.Trim(),
                    PurchaseDate = DateTime.Now,
                    EndDate = endDate, 
                    Subscriptions_ID = selectedSubscription.ID_Subscriptions,
                    SubscriptionStatus = selectedStatus 
                };

                context.Clients.Add(newClient);
                context.SaveChanges();

                MessageBox.Show("Клиент успешно добавлен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                Close(); 
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении клиента: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private bool ValidateFields()
        {
            if (string.IsNullOrWhiteSpace(SurnameTextBox.Text) || !SurnameTextBox.Text.All(char.IsLetter))
            {
                MessageBox.Show("Пожалуйста, введите корректную фамилию (только буквы).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(FirstnameTextBox.Text) || !FirstnameTextBox.Text.All(char.IsLetter))
            {
                MessageBox.Show("Пожалуйста, введите корректное имя (только буквы).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!string.IsNullOrWhiteSpace(MiddlenameTextBox.Text) && !MiddlenameTextBox.Text.All(char.IsLetter))
            {
                MessageBox.Show("Если отчество указано, оно должно содержать только буквы.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!BirthDatePicker.SelectedDate.HasValue || BirthDatePicker.SelectedDate.Value >= DateTime.Now)
            {
                MessageBox.Show("Пожалуйста, выберите корректную дату рождения (не в будущем).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            var phonePattern = @"^8\(\d{3}\)\d{3}-\d{2}-\d{2}$";
            if (!string.IsNullOrWhiteSpace(PhoneTextBox.Text) && !System.Text.RegularExpressions.Regex.IsMatch(PhoneTextBox.Text, phonePattern))
            {
                MessageBox.Show("Пожалуйста, введите телефон в формате 8(XXX)XXX-XX-XX.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(EmailTextBox.Text) || !IsValidEmail(EmailTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите корректный Email.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (SubscriptionComboBox.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите абонемент.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

    }
}