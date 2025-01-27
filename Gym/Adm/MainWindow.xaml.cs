using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Gym
{
    public partial class MainWindow : Window
    {
        private GymmEntities context;
        private int clientId;
        private string surnameSearchText = string.Empty;

        public MainWindow(string userRole)
        {
            InitializeComponent();
            context = new GymmEntities();
            this.clientId = clientId;
            CurrentDateTextBlock.Text = DateTime.Now.ToString("dddd, dd MMMM yyyy");

            LoadScheduledClasses();

            if (userRole != "Администратор")
            {
                if (MainTabControl.Items.Contains(SubscriptionsTab))
                    MainTabControl.Items.Remove(SubscriptionsTab);

                if (MainTabControl.Items.Contains(ScheduleTab))
                    MainTabControl.Items.Remove(ScheduleTab);
            }
        }
        private void LoadScheduledClasses()
        {
            using (var context = new GymmEntities())
            {
                var todayClasses = context.GroupClasses
                    .Where(gc => DbFunctions.TruncateTime(gc.Date) == DbFunctions.TruncateTime(DateTime.Now))
                    .Select(gc => new
                    {
                        ClassTime = gc.Time, 
                        ClassName = gc.ClassName,
                        TrainerName = gc.Trainers.Surname 
                    })
                    .ToList();

                var formattedClasses = todayClasses.Select(gc => new
                {
                    ClassTime = gc.ClassTime.ToString(@"hh\:mm"), 
                    ClassName = gc.ClassName,
                    TrainerName = gc.TrainerName
                }).ToList();

                ScheduledClassesDataGrid.ItemsSource = formattedClasses;
            }
        }

        private async void UpdateSubscriptionExpiryNotification()
        {
            var clients = context.Clients.ToList(); 

            var expiringClients = clients.Where(c => c.EndDate <= DateTime.Now.AddDays(7) && c.EndDate >= DateTime.Now).ToList();

            if (expiringClients.Any())
            {
                string notification = "Клиенты с истекающими абонементами: \n" + string.Join("\n", expiringClients.Select(c => c.Surname));

                SubscriptionExpiryNotification.Text = notification;

                await Task.Delay(15000); 
                SubscriptionExpiryNotification.Text = ""; 
            }
            else
            {
                SubscriptionExpiryNotification.Text = ""; 
            }
        }


        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();

            this.Close();
        }


        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                if (ClientsTab.IsSelected)
                {
                    LoadClients();
                    UpdateSubscriptionExpiryNotification();
                    StatusTextBlock.Text = "Вы находитесь в разделе: Учет клиентов";
                }
                else if (SubscriptionsTab != null && SubscriptionsTab.IsSelected)
                {
                    LoadSubscriptions();
                    StatusTextBlock.Text = "Вы находитесь в разделе: Управление абонементами"; 
                }
                else if (ScheduleTab != null && ScheduleTab.IsSelected)
                {
                    LoadSchedule();
                    StatusTextBlock.Text = "Вы находитесь в разделе: Расписание";
                }
                else if (HomeTab != null && HomeTab.IsSelected)
                {
                    LoadScheduledClasses();
                    StatusTextBlock.Text = "Вы находитесь в разделе: Главная";
                }

            }
        }

        private void LoadClients()
        {
            try
            {
                string selectedStatus = ((ComboBoxItem)StatusFilterComboBox.SelectedItem)?.Content.ToString();

                var clientsQuery = context.Clients.AsQueryable();

                if (selectedStatus != "Все" && !string.IsNullOrEmpty(selectedStatus))
                {
                    clientsQuery = clientsQuery.Where(c => c.SubscriptionStatus == selectedStatus);
                }

                if (!string.IsNullOrEmpty(surnameSearchText))
                {
                    clientsQuery = clientsQuery.Where(c => c.Surname.ToLower().Contains(surnameSearchText));
                }

                var clients = clientsQuery
                    .Select(c => new
                    {
                        ID = c.ID_Client,
                        Фамилия = c.Surname,
                        Имя = c.Firstname,
                        Отчество = c.Middlename,
                        Дата_рождения = c.BirthDate,
                        Телефон = c.Phone,
                        Email = c.Email,
                        Дата_покупки = c.PurchaseDate, 
                        Дата_окончания = c.EndDate, 
                        Абонемент = c.Subscriptions.Type, 
                        Статус_абонемента = c.SubscriptionStatus
                    })
                    .ToList();

                ClientsDataGrid.ItemsSource = clients;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки клиентов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddClientButton_Click(object sender, RoutedEventArgs e)
        {
            var addClientWindow = new AddClientWindow(context);
            addClientWindow.ShowDialog();
            LoadClients(); 
        }


        private void EditClientButton_Click(object sender, RoutedEventArgs e)
        {
            if (ClientsDataGrid.SelectedItem is null)
            {
                MessageBox.Show("Пожалуйста, выберите клиента для редактирования.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedClient = (dynamic)ClientsDataGrid.SelectedItem;
            int clientId = selectedClient.ID;

            var editClientWindow = new EditClientWindow(context, clientId);
            editClientWindow.ShowDialog();
            LoadClients(); 
        }

        private void DeleteClientButton_Click(object sender, RoutedEventArgs e)
        {
            if (ClientsDataGrid.SelectedItem is null)
            {
                MessageBox.Show("Пожалуйста, выберите клиента для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedClient = (dynamic)ClientsDataGrid.SelectedItem;
            int clientId = selectedClient.ID;

            var client = context.Clients.FirstOrDefault(c => c.ID_Client == clientId);

            if (client != null)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить этого клиента?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    context.Clients.Remove(client);
                    context.SaveChanges();
                    MessageBox.Show("Клиент успешно удален", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadClients(); 
                }
            }
            else
            {
                MessageBox.Show("Клиент не найден", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void StatusFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadClients();
        }

        private void SurnameSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            surnameSearchText = SurnameSearchTextBox.Text.Trim().ToLower();
            LoadClients();
        }


        private void LoadSubscriptions()
        {
            try
            {
                if (DurationDayFilterComboBox == null || PriceFilterComboBox == null)
                {
                    return; 
                }

                if (DurationDayFilterComboBox.SelectedItem == null)
                {
                    DurationDayFilterComboBox.SelectedIndex = 4; 
                }

                if (PriceFilterComboBox.SelectedItem == null)
                {
                    PriceFilterComboBox.SelectedIndex = 4; 
                }

                var selectedDuration = DurationDayFilterComboBox.SelectedItem as ComboBoxItem;

                var selectedPrice = PriceFilterComboBox.SelectedItem as ComboBoxItem;

                if (context == null)
                {
                    context = new GymmEntities();
                }

                var subscriptionsQuery = context.Subscriptions.AsQueryable();


                if (selectedDuration != null)
                {
                    switch (selectedDuration.Content.ToString())
                    {
                        case "Меньше 30 дней":
                            subscriptionsQuery = subscriptionsQuery.Where(s => s.DurationDays < 30);
                            break;
                        case "30-90 дней":
                            subscriptionsQuery = subscriptionsQuery.Where(s => s.DurationDays >= 30 && s.DurationDays <= 90);
                            break;
                        case "91-180 дней":
                            subscriptionsQuery = subscriptionsQuery.Where(s => s.DurationDays > 90 && s.DurationDays <= 180);
                            break;
                        case "Больше 180 дней":
                            subscriptionsQuery = subscriptionsQuery.Where(s => s.DurationDays > 180);
                            break;
                        case "Все":
                            break;
                        default:
                            break;
                    }
                }

                if (selectedPrice != null)
                {
                    switch (selectedPrice.Content.ToString())
                    {
                        case "Меньше 500":
                            subscriptionsQuery = subscriptionsQuery.Where(s => s.Price < 500);
                            break;
                        case "500-1000":
                            subscriptionsQuery = subscriptionsQuery.Where(s => s.Price >= 500 && s.Price <= 1000);
                            break;
                        case "1000-5000":
                            subscriptionsQuery = subscriptionsQuery.Where(s => s.Price > 1000 && s.Price <= 5000);
                            break;
                        case "Больше 5000":
                            subscriptionsQuery = subscriptionsQuery.Where(s => s.Price > 5000);
                            break;
                        case "Все":
                            break;
                        default:
                            break;
                    }
                }

                if (SubscriptionsDataGrid != null)
                {
                    SubscriptionsDataGrid.ItemsSource = subscriptionsQuery.ToList();
                }
                else
                {
                    Console.WriteLine("SubscriptionsDataGrid is null.");
                }

            }
            catch (Exception ex)
            {
            }
        }

        private void PriceFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadSubscriptions(); 
        }

        private void DurationDayFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadSubscriptions(); 
        }

        private void AddSubscriptionsButton_Click(object sender, RoutedEventArgs e)
        {
            var addSubscriptionsWindow = new AddSubscriptionWindow(context);
            addSubscriptionsWindow.ShowDialog();
            LoadSubscriptions();
        }

        private void EditSubscriptionsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SubscriptionsDataGrid.SelectedItem is null)
                {
                    MessageBox.Show("Пожалуйста, выберите абонемент для редактирования.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var selectedSubscription = (dynamic)SubscriptionsDataGrid.SelectedItem;
                int subscriptionId = selectedSubscription.ID_Subscriptions;

                var editSubscriptionWindow = new EditSubscriptionWindow(context, subscriptionId);
                editSubscriptionWindow.ShowDialog(); 
                LoadSubscriptions(); 
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteSubscriptionsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SubscriptionsDataGrid.SelectedItem is null)
                {
                    MessageBox.Show("Пожалуйста, выберите абонемент для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (SubscriptionsDataGrid.SelectedItem is Subscriptions selectedSubscription)
                {
                    int subscriptionId = selectedSubscription.ID_Subscriptions;

                    var subscription = context.Subscriptions.FirstOrDefault(s => s.ID_Subscriptions == subscriptionId);

                    if (subscription != null)
                    {
                        var result = MessageBox.Show("Вы уверены, что хотите удалить этот абонемент?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                        if (result == MessageBoxResult.Yes)
                        {
                            context.Subscriptions.Remove(subscription);
                            context.SaveChanges();
                            MessageBox.Show("Абонемент успешно удален", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadSubscriptions(); 
                        }
                    }
                    else
                    {
                        MessageBox.Show("Абонемент не найден", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Неверный формат данных. Попробуйте еще раз.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void LoadSchedule()
        {
            try
            {
                var schedule = context.GroupClasses
                    .Join(context.Trainers,
                          groupClass => groupClass.Trainer_ID,
                          trainer => trainer.ID_Trainer,
                          (groupClass, trainer) => new
                          {
                              ID = groupClass.ID_Class,
                              Название = groupClass.ClassName,
                              Дата = groupClass.Date,
                              Время = groupClass.Time,
                              Максимальное_кол_во_участников = groupClass.MaxParticipants,
                              Зарегистрировано_участников = groupClass.RegisteredClients,
                              Тренер = trainer.Surname + " " + trainer.Firstname
                          })
                    .ToList();

                ScheduleDataGrid.ItemsSource = schedule;
                ScheduleDataGrid.Items.Refresh();  
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки расписания: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void AddClassButton_Click(object sender, RoutedEventArgs e)
        {
            var addClassWindow = new AddGroupClassWindow(context);
            addClassWindow.ShowDialog();
            LoadSchedule();
        }

        private void EditClassButton_Click(object sender, RoutedEventArgs e)
        {
            if (ScheduleDataGrid.SelectedItem is null)
            {
                MessageBox.Show("Пожалуйста, выберите занятие для редактирования.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedSchedule = (dynamic)ScheduleDataGrid.SelectedItem;
            int scheduleId = selectedSchedule.ID;

            var groupClass = context.GroupClasses.FirstOrDefault(g => g.ID_Class == scheduleId);

            if (groupClass != null)
            {
                var editScheduleWindow = new EditGroupClassWindow(context, groupClass);
                editScheduleWindow.ShowDialog();
                LoadSchedule();
            }
            else
            {
                MessageBox.Show("Занятие не найдено.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void DeleteClassButton_Click(object sender, RoutedEventArgs e)
        {
            if (ScheduleDataGrid.SelectedItem is null)
            {
                MessageBox.Show("Пожалуйста, выберите занятие для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedSchedule = (dynamic)ScheduleDataGrid.SelectedItem;
            int scheduleId = selectedSchedule.ID;

            var groupClass = context.GroupClasses.FirstOrDefault(g => g.ID_Class == scheduleId);

            if (groupClass != null)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить это занятие?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        context.GroupClasses.Remove(groupClass);
                        context.SaveChanges();

                        LoadSchedule();

                        MessageBox.Show("Занятие успешно удалено.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении занятия: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Занятие не найдено.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchTextBox.Text.ToLower();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var filteredSchedule = context.GroupClasses
                    .Join(context.Trainers,
                          groupClass => groupClass.Trainer_ID,
                          trainer => trainer.ID_Trainer,
                          (groupClass, trainer) => new
                          {
                              ID = groupClass.ID_Class,
                              Название = groupClass.ClassName,
                              Дата = groupClass.Date,
                              Время = groupClass.Time,
                              Максимальное_кол_во_участников = groupClass.MaxParticipants,
                              Зарегистрировано_участников = groupClass.RegisteredClients,
                              Тренер = trainer.Surname + " " + trainer.Firstname
                          })
                    .Where(g => g.Название.ToLower().Contains(searchText)) 
                    .ToList();

                ScheduleDataGrid.ItemsSource = filteredSchedule;
            }
            else
            {
                LoadSchedule();
            }
        }



    }
}