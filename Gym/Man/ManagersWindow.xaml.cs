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
    public partial class ManagersWindow : Window
    {
        private GymmEntities _context;
        private ObservableCollection<UserAccounts> _employees = new ObservableCollection<UserAccounts>();


        public ManagersWindow()
        {
            InitializeComponent();

            _context = new GymmEntities();

            _employees = new ObservableCollection<UserAccounts>();
            EmployeesDataGrid.ItemsSource = _employees;
            LoadEmployees();

        }

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

        private void AddEmployeeButton_Click(object sender, RoutedEventArgs e)
        {
            AddEmployeeWindow addEmployeeWindow = new AddEmployeeWindow();
            addEmployeeWindow.ShowDialog();
            LoadEmployees();
        }



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



        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainTabControl.SelectedItem is TabItem selectedTab)
            {
                StatusBarText.Text = $"Вы находитесь во вкладке: {selectedTab.Header}";
            }
        }


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