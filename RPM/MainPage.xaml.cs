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
                    return true;
                    //UserInfoPage userInfoPage = new UserInfoPage(client);
                    //NavigationService.Navigate(userInfoPage);
                    
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
         
            var regex = new Regex(@"^\+7\d{10}$");
            StringBuilder errors = new StringBuilder();
            bool hasEnglishLetter = false;
            bool hasNumber = false;

            if (string.IsNullOrEmpty(LoginTextBox.Text)) errors.AppendLine("Укажите логин");
            if (IsUserExists("Login", LoginTextBox.Text)) errors.AppendLine("Пользователь с таким логином уже существует");
            if (IsUserExists("Email", EmailTextBox.Text)) errors.AppendLine("Пользователь с таким email уже существует");
            if (IsUserExists("Phone", PhoneTextBox.Text)) errors.AppendLine("Пользователь с таким телефоном уже существует");
            if (PhoneTextBox.Text.Length != 12) errors.AppendLine("Телефон должен состоять из 12 символов");
            foreach (char c in PasswordBox.Password)
            {
                if (c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z') hasEnglishLetter = true;
                if (c >= '0' && c <= '9') hasNumber = true;
            }
            if (PasswordBox.Password.Length < 6) errors.AppendLine("Пароль должен быть больше 6 символов");
            if (!regex.IsMatch(PhoneTextBox.Text)) errors.AppendLine("Укажите номер телефона в формате +7XXXXXXXXXX");
            if (!hasEnglishLetter) errors.AppendLine("Пароль должен содержать английские буквы");
            if (!hasNumber) errors.AppendLine("Пароль должен содержать хотя бы одну цифру");
            if (!IsValidEmail(EmailTextBox.Text)) errors.AppendLine("Укажите корректный email");
            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }
            else
            {
                using (var db = new DistilleryRassvetBase())
                {
                    var userObject = new Clients
                    {
                        Login = LoginTextBox.Text,
                        Password = GetHash(PasswordBox.Password),
                        Phone = PhoneTextBox.Text,
                        Email = EmailTextBox.Text,
                        Name = "",
                        Surname = "",
                        Address = "",
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
                    case "Email":
                        return db.Clients.AsNoTracking().Any(u => u.Email == value);
                    case "Phone":
                        return db.Clients.AsNoTracking().Any(u => u.Phone == value);
                    default:
                        throw new ArgumentException("Invalid column name");
                }
            }
        }
        private void DBButton_Click(object sender, RoutedEventArgs e)
        {
            DescriptionDB descriptionDB = new DescriptionDB();
            NavigationService.Navigate(descriptionDB);
        }
    }
}
