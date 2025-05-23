using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SqlClient;
using System.Data;
using System.ComponentModel;

namespace RPM
{
    /// <summary>
    /// Главная страница приложения, содержащая функционал авторизации и регистрации пользователей
    /// </summary>
    public partial class MainPage : Page
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса MainPage
        /// </summary>
        public MainPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Обработчик события нажатия кнопки входа в систему
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие</param>
        /// <param name="e">Аргументы события</param>
        private void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            //// 1. Проверка на администратора
            //if (LoginTextBox.Text.ToLower() == "admin" && PasswordBox.Password == "adminpassword") // вы убили преподавателя этим фрагментом кода!!!ы
            //{
            //    NavigationService.Navigate(new DescriptionDB());
            //    return;
            //}

            // 2. Проверка на сотрудника
            if (AuthEmployee(LoginTextBox.Text, PasswordBox.Password))
            {
                NavigationService.Navigate(new DescriptionDB());
                return;
            }

            // 3. Проверка на клиента
            AuthClient(LoginTextBox.Text, PasswordBox.Password);
        }

        /// <summary>
        /// Проверяет авторизационные данные сотрудника
        /// </summary>
        /// <param name="fullName">Полное имя сотрудника</param>
        /// <param name="passportData">Паспортные данные сотрудника</param>
        /// <returns>True, если авторизация успешна, иначе False</returns>
        public bool AuthEmployee(string fullName, string passportData)
        {
            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(passportData))
            {
                return false;
            }

            using (var db = new DistilleryRassvetBase())
            {
                var employee = db.Employees.AsNoTracking()
                    .FirstOrDefault(e => e.FullName == fullName && e.DataPassport == passportData && e.IDJobTitle == 31);

                return employee != null;
            }
        }

        /// <summary>
        /// Проверяет авторизационные данные клиента
        /// </summary>
        /// <param name="login">Логин клиента</param>
        /// <param name="password">Пароль клиента</param>
        /// <returns>True, если авторизация успешна, иначе False</returns>
        public bool AuthClient(string login, string password)
        {
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль!");
                return false;
            }

            string _hashedPassword = GetHash(password);
            using (var db = new DistilleryRassvetBase())
            {
                var client = db.Clients.AsNoTracking()
                    .FirstOrDefault(u => u.Login == login && u.Password == _hashedPassword);
                if (client != null)
                {
                    UserInfoPage userInfoPage = new UserInfoPage(client);
                    NavigationService.Navigate(userInfoPage);
                    return true;
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль!");
                    return false;
                }
            }
        }

        /// <summary>
        /// Обработчик события нажатия кнопки регистрации
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие</param>
        /// <param name="e">Аргументы события</param>
        private void SignUpButton_Click(object sender, RoutedEventArgs e)
        {
            Registr(LoginTextBox.Text, PasswordBox.Password, EmailTextBox.Text, PhoneTextBox.Text);
        }

        /// <summary>
        /// Регистрирует нового клиента в системе
        /// </summary>
        /// <param name="login">Логин нового пользователя</param>
        /// <param name="password">Пароль нового пользователя</param>
        /// <param name="email">Email нового пользователя</param>
        /// <param name="phone">Телефон нового пользователя</param>
        /// <returns>True, если регистрация успешна, иначе False</returns>
        public bool Registr(string login, string password, string email, string phone)
        {
            var regex = new Regex(@"^\+7\d{10}$");
            StringBuilder errors = new StringBuilder();
            bool hasEnglishLetter = false;
            bool hasNumber = false;

            // Валидация логина
            if (string.IsNullOrEmpty(login))
                errors.AppendLine("Укажите логин");

            // Проверка уникальности логина
            if (IsClientLoginExists(login) || IsEmployeeLoginExists(login))
                errors.AppendLine("Пользователь с таким логином уже существует");

            // Валидация пароля
            foreach (char c in password)
            {
                if (c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z') hasEnglishLetter = true;
                if (c >= '0' && c <= '9') hasNumber = true;
            }

            string hashedPassword = GetHash(password);
            if (IsClientPasswordExists(hashedPassword) || IsEmployeePasswordExists(password))
                errors.AppendLine("Пользователь с таким паролем уже существует");

            if (password.Length < 6)
                errors.AppendLine("Пароль должен быть больше 6 символов");
            if (!hasEnglishLetter)
                errors.AppendLine("Пароль должен содержать английские буквы");
            if (!hasNumber)
                errors.AppendLine("Пароль должен содержать хотя бы одну цифру");

            // Валидация телефона
            if (phone.Length != 12)
                errors.AppendLine("Телефон должен состоять из 12 символов");
            if (!regex.IsMatch(phone))
                errors.AppendLine("Укажите номер телефона в формате +7XXXXXXXXXX");

            // Валидация email
            if (!IsValidEmail(email))
                errors.AppendLine("Укажите корректный email");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return false;
            }

            // Регистрация нового клиента
            using (var db = new DistilleryRassvetBase())
            {
                var userObject = new Clients
                {
                    Login = login,
                    Password = hashedPassword,
                    Phone = phone,
                    Email = email,
                    Name = "Name",
                    Surname = "Surname",
                    Address = "Address",
                };

                try
                {
                    db.Clients.Add(userObject);
                    db.SaveChanges();
                    MessageBox.Show("Вы успешно зарегистрировались");
                    ClearInputs();
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}");
                    return false;
                }
            }
        }

        /// <summary>
        /// Генерирует хеш строки с использованием алгоритма SHA1
        /// </summary>
        /// <param name="password">Пароль для хеширования</param>
        /// <returns>Хеш-строка в шестнадцатеричном формате</returns>
        private string GetHash(string password)
        {
            using (var sha1 = SHA1.Create())
            {
                return string.Concat(sha1.ComputeHash(Encoding.UTF8.GetBytes(password)).Select(x => x.ToString("X2")));
            }
        }

        /// <summary>
        /// Проверяет валидность email-адреса
        /// </summary>
        /// <param name="email">Email для проверки</param>
        /// <returns>True, если email валиден, иначе False</returns>
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Проверяет существование клиента с указанным логином
        /// </summary>
        /// <param name="login">Логин для проверки</param>
        /// <returns>True, если клиент с таким логином существует, иначе False</returns>
        private bool IsClientLoginExists(string login)
        {
            using (var db = new DistilleryRassvetBase())
            {
                return db.Clients.AsNoTracking().Any(u => u.Login == login);
            }
        }

        /// <summary>
        /// Проверяет существование клиента с указанным паролем
        /// </summary>
        /// <param name="hashedPassword">Хеш пароля для проверки</param>
        /// <returns>True, если клиент с таким паролем существует, иначе False</returns>
        private bool IsClientPasswordExists(string hashedPassword)
        {
            using (var db = new DistilleryRassvetBase())
            {
                return db.Clients.AsNoTracking().Any(u => u.Password == hashedPassword);
            }
        }

        /// <summary>
        /// Проверяет существование сотрудника с указанным полным именем
        /// </summary>
        /// <param name="fullName">Полное имя для проверки</param>
        /// <returns>True, если сотрудник с таким именем существует, иначе False</returns>
        private bool IsEmployeeLoginExists(string fullName)
        {
            using (var db = new DistilleryRassvetBase())
            {
                return db.Employees.AsNoTracking().Any(e => e.FullName == fullName);
            }
        }

        /// <summary>
        /// Проверяет существование сотрудника с указанными паспортными данными
        /// </summary>
        /// <param name="passportData">Паспортные данные для проверки</param>
        /// <returns>True, если сотрудник с такими данными существует, иначе False</returns>
        private bool IsEmployeePasswordExists(string passportData)
        {
            using (var db = new DistilleryRassvetBase())
            {
                return db.Employees.AsNoTracking().Any(e => e.DataPassport == passportData);
            }
        }

        /// <summary>
        /// Очищает поля ввода на форме
        /// </summary>
        private void ClearInputs()
        {
            LoginTextBox.Text = string.Empty;
            PasswordBox.Password = string.Empty;
            EmailTextBox.Text = string.Empty;
            PhoneTextBox.Text = string.Empty;
        }
    }
}