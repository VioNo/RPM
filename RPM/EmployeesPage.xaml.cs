using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace RPM
{
    public partial class EmployeesPage : Page
    {
        private List<Employees> _allEmployees = new List<Employees>();
        private List<JobTitles> _jobTitles = new List<JobTitles>();

        public EmployeesPage()
        {
            InitializeComponent();
            LoadData();
            this.IsVisibleChanged += Page_IsVisibleChanged;
        }

        private void LoadData()
        {
            using (var context = new DistilleryRassvetBase())
            {
                _allEmployees = context.Employees.Include("JobTitles").ToList();
                _jobTitles = context.JobTitles.ToList();

                // Добавляем элемент "Все должности" в начало списка
                var jobTitlesWithAll = new List<JobTitles> { new JobTitles { JobTitle = "Все должности" } };
                jobTitlesWithAll.AddRange(_jobTitles);

                JobTitleComboBox.ItemsSource = jobTitlesWithAll;
                JobTitleComboBox.SelectedIndex = 0;

                ListViewEmployees.ItemsSource = _allEmployees;
            }
        }


        private void ApplyFilters()
        {
            var filteredEmployees = _allEmployees.AsEnumerable();

            // Применение фильтра по полному имени
            if (!string.IsNullOrWhiteSpace(FullNameSearchTextBox.Text))
            {
                string searchText = FullNameSearchTextBox.Text.ToLower();
                filteredEmployees = filteredEmployees.Where(emp =>
                    emp.FullName != null && emp.FullName.ToLower().Contains(searchText));
            }

            // Применение фильтра по должности
            if (JobTitleComboBox.SelectedItem is JobTitles selectedJob &&
                selectedJob.JobTitle != "Все должности")
            {
                filteredEmployees = filteredEmployees.Where(emp =>
                    emp.JobTitles != null && emp.JobTitles.IDJobTitle == selectedJob.IDJobTitle);
            }

            ListViewEmployees.ItemsSource = filteredEmployees.ToList();
        }

        private void FullNameSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void JobTitleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void HireDateFilterPicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void SortByFirstLetter_Click(object sender, RoutedEventArgs e)
        {
            var employees = ListViewEmployees.ItemsSource as IEnumerable<Employees> ?? _allEmployees;

            ListViewEmployees.ItemsSource = employees
                .OrderBy(emp => !string.IsNullOrEmpty(emp.FullName) ? emp.FullName[0].ToString() : "")
                .ToList();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            EmployeesDialog employeesDialog = new EmployeesDialog(null);
            NavigationService.Navigate(employeesDialog);
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedEmployee = ListViewEmployees.SelectedItem as Employees;
            if (selectedEmployee != null)
            {
                EmployeesDialog employeesDialog = new EmployeesDialog(selectedEmployee);
                NavigationService.Navigate(employeesDialog);
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите сотрудника для редактирования.",
                              "Предупреждение",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var employeesToDelete = ListViewEmployees.SelectedItems.Cast<Employees>().ToList();

            if (employeesToDelete.Count == 0)
            {
                MessageBox.Show("Пожалуйста, выберите хотя бы одного сотрудника для удаления.",
                              "Предупреждение",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                return;
            }

            var confirmation = MessageBox.Show($"Вы точно хотите удалить {employeesToDelete.Count} сотрудника(ов)?",
                                          "Подтверждение удаления",
                                          MessageBoxButton.YesNo,
                                          MessageBoxImage.Question);

            if (confirmation != MessageBoxResult.Yes) return;

            try
            {
                using (var context = new DistilleryRassvetBase())
                {
                    var employeeIds = employeesToDelete.Select(emp => emp.IDEmployee).ToList();
                    var existingEmployees = context.Employees
                        .Where(emp => employeeIds.Contains(emp.IDEmployee))
                        .ToList();

                    context.Employees.RemoveRange(existingEmployees);
                    int deletedCount = context.SaveChanges();

                    MessageBox.Show($"Успешно удалено {deletedCount} сотрудника(ов).",
                                  "Успех",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);
                    LoadData();
                }
            }
            catch (DbUpdateException dbEx)
            {
                string errorMessage = dbEx.InnerException?.Message ?? dbEx.Message;
                MessageBox.Show($"Ошибка базы данных: {errorMessage}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            if (this.Visibility == Visibility.Visible)
            {
                LoadData();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new DescriptionDB());
        }
    }
}
