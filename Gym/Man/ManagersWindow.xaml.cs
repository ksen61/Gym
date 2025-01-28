using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Runtime.Remoting.Contexts;
using System.Collections.ObjectModel;

namespace Gym
{
    /// <summary>
    /// Окно для управления сотрудниками, формирования отчетов и взаимодействия с данными тренажерного зала.
    /// Включает функции добавления, редактирования, удаления сотрудников, а также генерации отчетов в различных форматах.
    /// </summary>
    public partial class ManagersWindow : Window
    {
        private GymmEntities _context;
        private ObservableCollection<UserAccounts> _employees = new ObservableCollection<UserAccounts>();

        /// <summary>
        /// Конструктор окна менеджера, инициализирует контекст базы данных и загружает список сотрудников.
        /// </summary>
        public ManagersWindow()
        {
            InitializeComponent();

            _context = new GymmEntities();

            _employees = new ObservableCollection<UserAccounts>();
            EmployeesDataGrid.ItemsSource = _employees;
            LoadEmployees();

        }

        /// <summary>
        /// Загружает список сотрудников из базы данных и отображает их в таблице.
        /// </summary>
        private void LoadEmployees()
        {
            try
            {
                var employees = _context.UserAccounts
                    .Include("Roles")
                    .Include("Trainers")
                    .ToList();

                if (employees.Count == 0)
                {
                    MessageBox.Show("Нет сотрудников для отображения.");
                }

                _employees.Clear();

                foreach (var employee in employees)
                {
                    _employees.Add(employee);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}");
            }
        }

        /// <summary>
        /// Открывает окно для добавления нового сотрудника.
        /// </summary>
        private void AddEmployeeButton_Click(object sender, RoutedEventArgs e)
        {
            AddEmployeeWindow addEmployeeWindow = new AddEmployeeWindow();
            addEmployeeWindow.ShowDialog();
            LoadEmployees();
        }

        /// <summary>
        /// Открывает окно для редактирования выбранного сотрудника.
        /// </summary>
        private void EditEmployeeButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedEmployee = EmployeesDataGrid.SelectedItem as UserAccounts;

            if (selectedEmployee != null)
            {
                EditEmployeeWindow editEmployeeWindow = new EditEmployeeWindow(selectedEmployee.ID_UserAccounts, _employees);
                editEmployeeWindow.ShowDialog();
                LoadEmployees();
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите сотрудника для редактирования.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Удаляет выбранного сотрудника после подтверждения.
        /// </summary>
        private void DeleteEmployeeButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedEmployee = EmployeesDataGrid.SelectedItem as UserAccounts;

            if (selectedEmployee != null)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить этого сотрудника?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _context.UserAccounts.Remove(selectedEmployee);
                    _context.SaveChanges();

                    LoadEmployees();
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите сотрудника для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Закрывает текущее окно и открывает окно авторизации.
        /// </summary>
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

        /// <summary>
        /// Обрабатывает изменение вкладки и обновляет статус в статусной строке.
        /// </summary>
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainTabControl.SelectedItem is TabItem selectedTab)
            {
                StatusBarText.Text = $"Вы находитесь во вкладке: {selectedTab.Header}";
            }
        }

        /// <summary>
        /// Показывает/скрывает элементы управления для выбора дат в зависимости от выбранного отчета.
        /// </summary>
        private void ReportComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ReportComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                if (selectedItem.Content.ToString() == "Доходы за выбранный период")
                {
                    DateSelectionGrid.Visibility = Visibility.Visible;
                }
                else
                {
                    DateSelectionGrid.Visibility = Visibility.Collapsed;
                }
            }
        }

        /// <summary>
        /// Загружает отчет на основе выбранного типа отчета.
        /// </summary>
        private void LoadOt(string reportName)
        {
            if (reportName == "Доходы за выбранный период")
            {
               

                ShowIncomeReport(); 
            }
            else if (reportName == "Популярные услуги")
            {
                ShowPopularServicesReport(); 
            }
            else if (reportName == "График работы тренеров")
            {
                ShowTrainerScheduleReport(); 
            }
            else
            {
                MessageBox.Show("Выбран неверный тип отчёта!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Обрабатывает нажатие кнопки для отображения выбранного отчета.
        /// </summary>
        private void ShowReportButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedReport = (ReportComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();

            if (string.IsNullOrEmpty(selectedReport))
            {
                MessageBox.Show("Пожалуйста, выберите отчёт из списка!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (selectedReport == "Доходы за выбранный период" &&
                (StartDatePicker.SelectedDate == null || EndDatePicker.SelectedDate == null))
            {
                MessageBox.Show("Пожалуйста, выберите даты начала и конца периода для отчета по доходам.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return; 
            }

            LoadOt(selectedReport); 
        }

        /// <summary>
        /// Формирует отчет о доходах за выбранный период.
        /// </summary>
        private void ShowIncomeReport()
        {
            DateTime startDate = StartDatePicker.SelectedDate.Value;
            DateTime endDate = EndDatePicker.SelectedDate.Value;

            var income = _context.Payment
                .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate)
                .GroupBy(p => p.PaymentDate)
                .Select(g => new
                {
                    Date = g.Key,
                    TotalIncome = g.Sum(p => p.Amount)
                }).ToList();

            ReportDataGrid.ItemsSource = income;
        }

        /// <summary>
        /// Формирует отчет о популярных услугах.
        /// </summary>
        private void ShowPopularServicesReport()
        {
            var popularServices = _context.Subscriptions
                .Select(s => new
                {
                    s.Type,
                    s.Price,
                    AvailableServices = s.AvailableServices,
                    PurchaseCount = s.Clients.Count() 
                })
                .OrderByDescending(s => s.PurchaseCount) 
                .ToList();

            ReportDataGrid.ItemsSource = popularServices;
        }

        /// <summary>
        /// Формирует отчет о графике работы тренеров.
        /// </summary>
        private void ShowTrainerScheduleReport()
        {
            var trainerSchedule = _context.GroupClasses
                .Select(gc => new
                {
                    gc.ClassName,
                    gc.Date,
                    gc.Time,
                    TrainerName = gc.Trainers.Firstname + " " + gc.Trainers.Middlename
                }).ToList();

            ReportDataGrid.ItemsSource = trainerSchedule;
        }

        /// <summary>
        /// Генерирует PDF-отчет на основе выбранного типа отчета и сохраняет его в файл.
        /// </summary>
        private void GeneratePdfButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedReport = (ReportComboBox.SelectedItem as ComboBoxItem).Content.ToString();

            if (selectedReport == "Доходы за выбранный период")
            {
                if (StartDatePicker.SelectedDate == null || EndDatePicker.SelectedDate == null)
                {
                    MessageBox.Show("Пожалуйста, выберите даты начала и конца периода для отчета по доходам.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return; 
                }
            }
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PDF Files (*.pdf)|*.pdf"; 
            saveFileDialog.DefaultExt = "pdf"; 
            saveFileDialog.FileName = "report.pdf"; 

            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName; 

                Document document = new Document();
                PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create));
                document.Open();

                string fontPath = @"C:\Windows\Fonts\arial.ttf"; 
                BaseFont baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                Font font = new Font(baseFont, 12);

                document.Add(new Paragraph("Отчет", font));

                if (selectedReport == "Доходы за выбранный период")
                {
                    DateTime startDate = StartDatePicker.SelectedDate.Value;
                    DateTime endDate = EndDatePicker.SelectedDate.Value;

                    var income = _context.Payment
                        .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate)
                        .GroupBy(p => p.PaymentDate)
                        .Select(g => new
                        {
                            Date = g.Key,
                            TotalIncome = g.Sum(p => p.Amount)
                        }).ToList();

                    document.Add(new Paragraph($"Доходы за период с {startDate.ToShortDateString()} по {endDate.ToShortDateString()}", font));
                    document.Add(new Paragraph(" ")); 

                    foreach (var record in income)
                    {
                        document.Add(new Paragraph($"{record.Date.ToShortDateString()} - {record.TotalIncome} руб.", font));
                    }
                }
                else if (selectedReport == "Популярные услуги")
                {
                    var popularServices = _context.Subscriptions
                        .Select(s => new
                        {
                            s.Type,
                            s.Price,
                            AvailableServices = s.AvailableServices
                        }).ToList();

                    document.Add(new Paragraph("Популярные услуги", font));
                    document.Add(new Paragraph(" "));

                    foreach (var service in popularServices.OrderByDescending(s => s.Price)) 
                    {
                        document.Add(new Paragraph($"{service.Type} - {service.AvailableServices} - {service.Price} руб.", font));
                    }
                }
                else if (selectedReport == "График работы тренеров")
                {
                    var trainerSchedule = _context.GroupClasses
                        .Select(gc => new
                        {
                            gc.ClassName,
                            gc.Date,
                            gc.Time,
                            TrainerName = gc.Trainers.Firstname + " " + gc.Trainers.Middlename
                        }).ToList();

                    document.Add(new Paragraph("График работы тренеров", font));
                    document.Add(new Paragraph(" ")); 

                    foreach (var record in trainerSchedule)
                    {
                        document.Add(new Paragraph($"{record.TrainerName}: {record.ClassName} - {record.Date.ToShortDateString()} {record.Time}", font));
                    }
                }

                document.Close();

                MessageBox.Show($"Отчет сохранен как {filePath}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}