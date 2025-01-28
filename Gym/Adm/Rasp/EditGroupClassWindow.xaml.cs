using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

/// <summary>
/// Окно редактирвоания данныъ занятия в системе. Позволяет пользователю ихменить данные зантия,
/// после чего сохраняет информацию в базе данных. Также включает в себя валидацию введённых данных и обработку ошибок.
/// </summary>
namespace Gym
{
    public partial class EditGroupClassWindow : Window
    {
        private GymmEntities context;
        private GroupClasses groupClass;

        /// <summary>
        /// Конструктор окна для редактирования группового занятия. Инициализирует поля окна значениями
        /// существующего занятия и загружает список тренеров из базы данных.
        /// </summary>
        public EditGroupClassWindow(GymmEntities dbContext, GroupClasses classToEdit)
        {
            InitializeComponent();
            context = dbContext;
            groupClass = classToEdit;

            ClassNameTextBox.Text = groupClass.ClassName;
            ClassDatePicker.SelectedDate = groupClass.Date;
            ClassTimeTextBox.Text = groupClass.Time.ToString(@"hh\:mm"); 
            MaxParticipantsTextBox.Text = groupClass.MaxParticipants.ToString();
            RegisteredClientsTextBox.Text = groupClass.RegisteredClients.ToString();

            TrainerComboBox.ItemsSource = context.Trainers
                .Select(t => new { t.ID_Trainer, FullName = t.Surname + " " + t.Firstname })
                .ToList();
            TrainerComboBox.DisplayMemberPath = "FullName";
            TrainerComboBox.SelectedValuePath = "ID_Trainer";
            TrainerComboBox.SelectedValue = groupClass.Trainer_ID;
        }


        /// <summary>
        /// Обработчик события для кнопки сохранения. Проверяет введенные данные, обновляет информацию о занятии
        /// в базе данных и сохраняет изменения.
        /// </summary>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ClassNameTextBox.Text) ||
                    string.IsNullOrWhiteSpace(MaxParticipantsTextBox.Text) ||
                    string.IsNullOrWhiteSpace(RegisteredClientsTextBox.Text) ||
                    ClassDatePicker.SelectedDate == null ||
                    string.IsNullOrWhiteSpace(ClassTimeTextBox.Text) ||
                    TrainerComboBox.SelectedValue == null)
                {
                    MessageBox.Show("Все поля должны быть заполнены.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!Regex.IsMatch(ClassNameTextBox.Text, @"^[А-Яа-яA-Za-z\s]+$"))
                {
                    MessageBox.Show("Название занятия может содержать только буквы.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }


                if (!TimeSpan.TryParseExact(ClassTimeTextBox.Text, "hh\\:mm", null, out TimeSpan time))
                {
                    MessageBox.Show("Введите корректное время в формате чч:мм.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                int maxParticipants, registeredClients;
                DateTime date = ClassDatePicker.SelectedDate.Value;
                int trainerId = (int)TrainerComboBox.SelectedValue;

                if (!int.TryParse(MaxParticipantsTextBox.Text, out maxParticipants) || maxParticipants <= 0)
                {
                    MessageBox.Show("Максимальное количество участников должно быть положительным числом.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!int.TryParse(RegisteredClientsTextBox.Text, out registeredClients) || registeredClients < 0)
                {
                    MessageBox.Show("Число зарегистрированных клиентов должно быть неотрицательным числом.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (registeredClients > maxParticipants)
                {
                    MessageBox.Show("Число зарегистрированных клиентов не может превышать максимальное количество участников.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                groupClass.ClassName = ClassNameTextBox.Text;
                groupClass.Date = date;
                groupClass.Time = time;
                groupClass.MaxParticipants = maxParticipants;
                groupClass.RegisteredClients = registeredClients;
                groupClass.Trainer_ID = trainerId;

                context.SaveChanges();

                MessageBox.Show("Групповое занятие успешно обновлено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при редактировании занятия: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Обработчик события для кнопки отмены. Закрывает окно редактирования без сохранения изменений.
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}