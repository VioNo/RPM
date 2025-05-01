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
    /// Логика взаимодействия для MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            Auth(LoginTextBox.Text, PasswordBox.Password);
        }
        public bool Auth(string login, string password)
        {
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль!");
                return false;
            }

            string _hashedPassword = GetHash(password);
            using (var db = new DistilleryRassvetBase())
            {
                var client = db.Clients.AsNoTracking().FirstOrDefault(u => u.Login == login && u.Password == _hashedPassword);
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

        private void SignUpButton_Click(object sender, RoutedEventArgs e)
        {
            Registr(LoginTextBox.Text, PasswordBox.Password, EmailTextBox.Text, PhoneTextBox.Text);
        }
        public bool Registr(string login, string password, string email, string phone )
        {
            var regex = new Regex(@"^\+7\d{10}$");
            StringBuilder errors = new StringBuilder();
            bool hasEnglishLetter = false;
            bool hasNumber = false;

            if (string.IsNullOrEmpty(login)) errors.AppendLine("Укажите логин");
            if (IsUserExists("Login", login)) errors.AppendLine("Пользователь с таким логином уже существует");
            if (phone.Length != 12) errors.AppendLine("Телефон должен состоять из 12 символов");
            foreach (char c in password)
            {
                if (c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z') hasEnglishLetter = true;
                if (c >= '0' && c <= '9') hasNumber = true;
            }
            if (IsUserExists("Password", GetHash(password))) errors.AppendLine("Пользователь с таким паролем уже существует");
            if (password.Length < 6) errors.AppendLine("Пароль должен быть больше 6 символов");
            if (!regex.IsMatch(phone)) errors.AppendLine("Укажите номер телефона в формате +7XXXXXXXXXX");
            if (!hasEnglishLetter) errors.AppendLine("Пароль должен содержать английские буквы");
            if (!hasNumber) errors.AppendLine("Пароль должен содержать хотя бы одну цифру");
            if (!IsValidEmail(email)) errors.AppendLine("Укажите корректный email");
            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return false;
            }
            else
            {
                using (var db = new DistilleryRassvetBase())
                {
                    var userObject = new Clients
                    {
                        Login = login,
                        Password = GetHash(password),
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
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}");
                    }
                    MessageBox.Show("Вы успешно зарегистрировались");
                    return true;
                }

            }
        }

        private string GetHash(string password)
        {
            using (var sha1 = SHA1.Create())
            {
                return string.Concat(sha1.ComputeHash(Encoding.UTF8.GetBytes(password)).Select(x => x.ToString("X2")));
            }
        }

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
       
        private bool IsUserExists(string columnName, string value)
        {
            using (var db = new DistilleryRassvetBase())
            {
                switch (columnName)
                {
                    case "Login":
                        return db.Clients.AsNoTracking().Any(u => u.Login == value);
                    case "Password":
                        return db.Clients.AsNoTracking().Any(u => u.Password == value);
                    default:
                        throw new ArgumentException("Invalid column name");
                }
            }
        }
        private void DBButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new DescriptionDB());
        }
    }
}
