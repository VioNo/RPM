using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace RPM
{
    /// <summary>
    /// Логика взаимодействия для EmployeesDialog.xaml
    /// </summary>
    public partial class EmployeesDialog : Page
    {
        public Employees Employee;
        private List<JobTitles> _jobTitles;

        public EmployeesDialog(Employees employee)
        {
            InitializeComponent();
            LoadJobTitles();

            if (employee != null)
            {
                Employee = employee;
                FullNameTextBox.Text = Employee.FullName;
                PhoneTextBox.Text = Employee.Phone.ToString();
                PassportTextBox.Text = Employee.DataPassport;
                EmailTextBox.Text = Employee.Email;
                DateStartedPicker.SelectedDate = Employee.DateStartedWork;

                if (Employee.IDJobTitle > 0)
                    JobTitleComboBox.SelectedValue = Employee.IDJobTitle;
            }
            else
            {
                DateStartedPicker.SelectedDate = DateTime.Today;
            }
        }

        private void LoadJobTitles()
        {
            using (var db = new DistilleryRassvetBase())
            {
                _jobTitles = db.JobTitles.ToList();
                JobTitleComboBox.ItemsSource = _jobTitles;
                JobTitleComboBox.DisplayMemberPath = "JobTitle";
                JobTitleComboBox.SelectedValuePath = "IDJobTitle";
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                using (var db = new DistilleryRassvetBase())
                {
                    try
                    {
                        if (Employee != null && Employee.IDEmployee > 0)
                        {
                            var existingEmployee = db.Employees.Find(Employee.IDEmployee);
                            if (existingEmployee != null)
                            {
                                UpdateEmployee(existingEmployee);
                                db.SaveChanges();
                                MessageBox.Show("Данные сотрудника успешно обновлены");
                            }
                        }
                        else
                        {
                            var newEmployee = new Employees();
                            UpdateEmployee(newEmployee);
                            db.Employees.Add(newEmployee);
                            db.SaveChanges();
                            MessageBox.Show("Новый сотрудник успешно сохранен");
                        }

                        NavigationService.Navigate(new EmployeesPage());
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}");
                    }
                }
            }
        }

        private void UpdateEmployee(Employees employee)
        {
            employee.FullName = FullNameTextBox.Text.Trim();
            employee.IDJobTitle = (int)JobTitleComboBox.SelectedValue;
            employee.Phone = PhoneTextBox.Text;
            employee.DataPassport = PassportTextBox.Text.Trim();
            employee.Email = EmailTextBox.Text.Trim();
            employee.DateStartedWork = DateStartedPicker.SelectedDate.Value;
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(FullNameTextBox.Text))
            {
                MessageBox.Show("Введите ФИО сотрудника");
                return false;
            }

            if (JobTitleComboBox.SelectedValue == null)
            {
                MessageBox.Show("Выберите должность");
                return false;
            }

            string phoneText = new string(PhoneTextBox.Text.Where(char.IsDigit).ToArray());
            if (phoneText.Length < 10)
            {
                MessageBox.Show("Введите корректный номер телефона (минимум 10 цифр)");
                return false;
            }

            if (string.IsNullOrWhiteSpace(PassportTextBox.Text))
            {
                MessageBox.Show("Введите паспортные данные");
                return false;
            }

            if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                MessageBox.Show("Введите email");
                return false;
            }

            if (DateStartedPicker.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату приема");
                return false;
            }

            return true;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new EmployeesPage());
        }
    }
}