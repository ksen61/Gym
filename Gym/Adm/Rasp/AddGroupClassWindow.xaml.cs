﻿using System;
using System.Linq;
using System.Windows;

namespace Gym
{
    public partial class AddGroupClassWindow : Window
    {
        private GymmEntities context;

        public AddGroupClassWindow(GymmEntities dbContext)
        {
            InitializeComponent();
            context = dbContext;

            TrainerComboBox.ItemsSource = context.Trainers
            .Select(t => new { t.ID_Trainer, FullName = t.Surname + " " + t.Firstname })
            .ToList();
            TrainerComboBox.DisplayMemberPath = "FullName"; 
            TrainerComboBox.SelectedValuePath = "ID_Trainer";
        }

        private void TimeTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"^[0-9:]$");
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ClassNameTextBox.Text) ||
                    string.IsNullOrWhiteSpace(MaxParticipantsTextBox.Text) ||
                    string.IsNullOrWhiteSpace(RegisteredClientsTextBox.Text) ||
                    DatePicker.SelectedDate == null ||
                    string.IsNullOrWhiteSpace(TimeTextBox.Text) ||
                    TrainerComboBox.SelectedValue == null)
                {
                    MessageBox.Show("Все поля должны быть заполнены.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!System.Text.RegularExpressions.Regex.IsMatch(ClassNameTextBox.Text, @"^[a-zA-ZА-Яа-я\s]+$"))
                {
                    MessageBox.Show("Название занятия может содержать только буквы.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!TimeSpan.TryParse(TimeTextBox.Text, out TimeSpan time))
                {
                    MessageBox.Show("Введите корректное время в формате чч:мм.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                int maxParticipants, registeredClients;
                DateTime date = DatePicker.SelectedDate.Value;
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

                var newClass = new GroupClasses
                {
                    ClassName = ClassNameTextBox.Text,
                    Date = date,
                    Time = time,
                    MaxParticipants = maxParticipants,
                    RegisteredClients = registeredClients,
                    Trainer_ID = trainerId
                };

                context.GroupClasses.Add(newClass);
                context.SaveChanges();

                MessageBox.Show("Групповое занятие успешно добавлено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении занятия: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


    }
}