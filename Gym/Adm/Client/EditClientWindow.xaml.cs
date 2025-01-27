using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Gym
{
    public partial class EditClientWindow : Window
    {
        private GymmEntities context;
        private int clientId;

        public EditClientWindow(GymmEntities context, int clientId)
        {
            InitializeComponent();
            this.context = context;
            this.clientId = clientId;

            var client = context.Clients.FirstOrDefault(c => c.ID_Client == clientId);
            if (client != null)
            {
                SurnameTextBox.Text = client.Surname;
                FirstnameTextBox.Text = client.Firstname;
                MiddlenameTextBox.Text = client.Middlename;
                PhoneTextBox.Text = client.Phone;
                EmailTextBox.Text = client.Email;

                BirthDatePicker.SelectedDate = client.BirthDate;
                PurchaseDatePicker.SelectedDate = client.PurchaseDate;

                var subscriptions = context.Subscriptions.ToList();
                SubscriptionComboBox.ItemsSource = subscriptions;
                SubscriptionComboBox.DisplayMemberPath = "Type";
                SubscriptionComboBox.SelectedValuePath = "ID_Subscription";

                if (client.Subscriptions_ID != 0)
                {
                    var selectedSubscription = subscriptions.FirstOrDefault(s => s.ID_Subscriptions == client.Subscriptions_ID);
                    SubscriptionComboBox.SelectedItem = selectedSubscription;

                    if (client.PurchaseDate.HasValue && selectedSubscription != null)
                    {
                        EndDatePicker.SelectedDate = client.PurchaseDate.Value.AddDays(selectedSubscription.DurationDays);
                    }
                }

                var statuses = new List<string> { "Активен", "Неактивен", "Просрочен" };
                SubscriptionStatusComboBox.ItemsSource = statuses;
                SubscriptionStatusComboBox.SelectedItem = client.SubscriptionStatus; 
            }
            else
            {
                MessageBox.Show("Клиент не найден", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }

            PurchaseDatePicker.SelectedDateChanged += PurchaseDatePicker_SelectedDateChanged;
        }

        private void PurchaseDatePicker_SelectedDateChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (PurchaseDatePicker.SelectedDate.HasValue)
            {
                DateTime purchaseDate = PurchaseDatePicker.SelectedDate.Value;

                var selectedSubscription = SubscriptionComboBox.SelectedItem as Subscriptions;
                if (selectedSubscription != null)
                {
                    EndDatePicker.SelectedDate = purchaseDate.AddDays(selectedSubscription.DurationDays);
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateFields())
            {
                return;
            }

            try
            {
                var client = context.Clients.FirstOrDefault(c => c.ID_Client == clientId);
                if (client != null)
                {
                    client.Surname = SurnameTextBox.Text;
                    client.Firstname = FirstnameTextBox.Text;
                    client.Middlename = MiddlenameTextBox.Text;
                    client.Phone = PhoneTextBox.Text;
                    client.Email = EmailTextBox.Text;

                    if (BirthDatePicker.SelectedDate.HasValue)
                    {
                        client.BirthDate = BirthDatePicker.SelectedDate.Value;
                    }
                    else
                    {
                        MessageBox.Show("Дата рождения не выбрана", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (PurchaseDatePicker.SelectedDate.HasValue)
                    {
                        client.PurchaseDate = PurchaseDatePicker.SelectedDate.Value;
                    }
                    else
                    {
                        MessageBox.Show("Дата покупки не выбрана", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (PurchaseDatePicker.SelectedDate.HasValue && SubscriptionComboBox.SelectedItem != null)
                    {
                        var selectedSubscription = SubscriptionComboBox.SelectedItem as Subscriptions;
                        if (selectedSubscription != null)
                        {
                            client.EndDate = PurchaseDatePicker.SelectedDate.Value.AddDays(selectedSubscription.DurationDays);
                        }
                    }

                    var selectedStatus = SubscriptionStatusComboBox.SelectedItem as string;
                    if (!string.IsNullOrEmpty(selectedStatus))
                    {
                        client.SubscriptionStatus = selectedStatus;
                    }
                    else
                    {
                        MessageBox.Show("Статус абонемента не выбран", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    context.SaveChanges();
                    MessageBox.Show("Данные успешно сохранены", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Клиент не найден", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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