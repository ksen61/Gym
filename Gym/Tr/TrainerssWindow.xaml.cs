using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Gym
{
    /// <summary>
    /// Класс для отображения и управления расписанием тренеров и посещаемостью их занятий.
    /// </summary>
    public partial class TrainerssWindow : Window
    {
        private GymmEntities context = new GymmEntities();

        /// <summary>
        /// Конструктор окна тренера, который загружает расписание для конкретного тренера по его ID.
        /// </summary>
        public TrainerssWindow(int userId)
        {
            InitializeComponent();

            var trainerId = context.UserAccounts
                .Where(u => u.ID_UserAccounts == userId)
                .Select(u => u.Trainer_ID) 
                .FirstOrDefault();

            if (trainerId != 0)
            {
                var schedule = context.GroupClasses
                    .Where(gc => gc.Trainer_ID == trainerId) 
                    .ToList();

                if (schedule.Any())
                {
                    ScheduleDataGrid.ItemsSource = schedule;
                }
                else
                {
                    MessageBox.Show("Нет занятий для данного тренера.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("Тренер не найден!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            LoadAttendance();
        }

        /// <summary>
        /// Загружает данные о посещаемости для каждого занятия тренера.
        /// </summary>
        private void LoadAttendance()
        {
            var attendanceData = context.GroupClass_Clients
                .Select(gcc => new Attendance
                {
                    ClientName = gcc.Clients.Surname + " " + gcc.Clients.Firstname,
                    AttendanceStatus_Bit = gcc.AttendanceStatus_Bit,
                    ClassName = gcc.GroupClasses.ClassName, 
                    Date = gcc.GroupClasses.Date,  
                    Time = gcc.GroupClasses.Time,
                    Client_ID = gcc.Client_ID, 
                    Class_ID = gcc.Class_ID    
                })
                .ToList();

            if (attendanceData.Any())
            {
                AttendanceDataGrid.ItemsSource = attendanceData;
            }
            else
            {
                MessageBox.Show("Нет данных о посещаемости.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Закрывает текущее окно и возвращает пользователя на экран входа.
        /// </summary>
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

        /// <summary>
        /// Обработчик смены вкладки в интерфейсе, обновляющий текст с информацией о текущей вкладке.
        /// </summary>
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TabControl.SelectedItem is TabItem selectedTab)
            {
                StatusTextBlock.Text = $"Вы выбрали вкладку: {selectedTab.Header}";
            }
        }

        /// <summary>
        /// Сохраняет изменения в посещаемости для каждого клиента, отредактировавшего свой статус.
        /// </summary>
        private void SaveAttendanceButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var modifiedItems = AttendanceDataGrid.ItemsSource.Cast<Attendance>()
                    .Where(item => item.AttendanceStatus_Bit.HasValue)  
                    .ToList();

                foreach (var item in modifiedItems)
                {
                    var attendanceRecord = context.GroupClass_Clients
                        .FirstOrDefault(gcc => gcc.Client_ID == item.Client_ID && gcc.Class_ID == item.Class_ID);

                    if (attendanceRecord != null)
                    {
                        attendanceRecord.AttendanceStatus_Bit = item.AttendanceStatus_Bit;

                        context.Entry(attendanceRecord).State = System.Data.Entity.EntityState.Modified;
                    }
                }

                context.SaveChanges();

                MessageBox.Show("Изменения успешно сохранены.", "Сохранение", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении изменений: {ex.Message}");
                MessageBox.Show("Произошла ошибка при сохранении данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Обработчик выбора занятия в расписании для загрузки соответствующей информации о посещаемости.
        /// </summary>
        private void ScheduleDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ScheduleDataGrid.SelectedItem != null)
            {
                var selectedClass = (GroupClasses)ScheduleDataGrid.SelectedItem;
                int classId = selectedClass.ID_Class;
                LoadAttendance();
            }
        }

        /// <summary>
        /// Обработчик редактирования ячейки таблицы посещаемости для обновления статуса посещаемости.
        /// </summary>
        private void AttendanceDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            var editedItem = e.Row.Item as dynamic;
            if (editedItem != null)
            {
                int clientId = editedItem.Client_ID;
                bool isPresent = editedItem.AttendanceStatus_Bit;

                var attendanceRecord = context.GroupClass_Clients
                    .FirstOrDefault(gcc => gcc.Client_ID == clientId && gcc.Class_ID == 1); 

                if (attendanceRecord != null)
                {
                    attendanceRecord.AttendanceStatus_Bit = isPresent;
                    context.SaveChanges();
                }
            }
        }
    }
}