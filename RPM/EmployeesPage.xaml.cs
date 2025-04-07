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
        public EmployeesPage()
        {
            InitializeComponent();
            LoadEmployees();
            this.IsVisibleChanged += Page_IsVisibleChanged;
        }

        private void LoadEmployees()
        {
            using (var context = new DistilleryRassvetBase())
            {
                var employees = context.Employees.Include("JobTitles").ToList();
                ListViewEmployees.ItemsSource = employees;
            }
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
                    LoadEmployees();
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
                LoadEmployees();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new DescriptionDB());
        }
    }
}