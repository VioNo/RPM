using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
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
            DateStartedPicker.DisplayDateEnd = DateTime.Today;

            if (employee != null)
            {
                Employee = employee;
                FullNameTextBox.Text = Employee.FullName;
                PhoneTextBox.Text = Employee.Phone;
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
            employee.Phone = FormatPhoneNumber(PhoneTextBox.Text);
            employee.DataPassport = PassportTextBox.Text.Trim();
            employee.Email = EmailTextBox.Text.Trim();
            employee.DateStartedWork = DateStartedPicker.SelectedDate.Value;
        }

        private string FormatPhoneNumber(string phone)
        {
            var sb = new StringBuilder();
            // Оставляем только цифры
            foreach (char c in phone.Where(char.IsDigit))
            {
                sb.Append(c);
            }

            // Форматируем номер в формате +7XXXXXXXXXX
            if (sb.Length >= 11 && sb[0] == '8')
            {
                sb.Remove(0, 1).Insert(0, "7");
            }
            else if (sb.Length >= 10 && sb[0] != '7')
            {
                sb.Insert(0, "7");
            }

            return sb.Length >= 11 ? $"+7{sb.ToString().Substring(1, 10)}" : sb.ToString();
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

            // Проверка телефона
            string phoneDigits = new string(PhoneTextBox.Text.Where(char.IsDigit).ToArray());
            if (phoneDigits.Length < 10)
            {
                MessageBox.Show("Номер телефона должен содержать минимум 10 цифр");
                return false;
            }

            // Форматирование телефона
            string formattedPhone = FormatPhoneNumber(PhoneTextBox.Text);
            if (!Regex.IsMatch(formattedPhone, @"^\+7\d{10}$"))
            {
                MessageBox.Show("Введите корректный номер телефона в формате +7XXXXXXXXXX");
                return false;
            }
            PhoneTextBox.Text = formattedPhone; // Обновляем поле с отформатированным номером

            if (string.IsNullOrWhiteSpace(PassportTextBox.Text))
            {
                MessageBox.Show("Введите паспортные данные");
                return false;
            }

            // Проверка email
            if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                MessageBox.Show("Введите email");
                return false;
            }

            if (!Regex.IsMatch(EmailTextBox.Text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Введите корректный email в формате example@domain.com");
                return false;
            }

            if (DateStartedPicker.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату приема");
                return false;
            }

            if (DateStartedPicker.SelectedDate > DateTime.Today)
            {
                MessageBox.Show("Дата приема не может быть в будущем");
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