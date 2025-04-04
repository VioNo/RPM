using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
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

namespace RPM
{
    /// <summary>
    /// Логика взаимодействия для ClientsDialog.xaml
    /// </summary>
    
    public partial class ClientsDialog : Page
    {
        public Clients Client;

        public ClientsDialog(Clients client)
        {
            InitializeComponent();

            if (client != null)
            {
                Client = client;
                DataContext = client;

                NameTextBox.Text = Client.Name;
                SurnameTextBox.Text = Client.Surname;
                PhoneTextBox.Text = Client.Phone;
                AddressTextBox.Text = Client.Address;
                EmailTextBox.Text = Client.Email;
                LoginTextBox.Text = Client.Login;
                // Пароль не устанавливаем для безопасности
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
                        if (Client != null && Client.IDClient > 0)
                        {
                            // Редактирование существующего клиента
                            var existingClient = db.Clients.Find(Client.IDClient);

                            if (existingClient != null)
                            {
                                existingClient.Name = NameTextBox.Text.Trim();
                                existingClient.Surname = SurnameTextBox.Text.Trim();
                                existingClient.Phone = PhoneTextBox.Text.Trim();
                                existingClient.Address = AddressTextBox.Text.Trim();
                                existingClient.Email = EmailTextBox.Text.Trim();
                                existingClient.Login = LoginTextBox.Text.Trim();

                                // Обновляем пароль только если он был изменен
                                if (!string.IsNullOrEmpty(PasswordBox.Password))
                                {
                                    existingClient.Password = PasswordBox.Password;
                                }

                                // Проверка валидации
                                var validationErrors = db.GetValidationErrors();
                                if (validationErrors.Any())
                                {
                                    var errorMessages = validationErrors
                                        .SelectMany(validationError => validationError.ValidationErrors)
                                        .Select(validationErrorItem => $"{validationErrorItem.PropertyName}: {validationErrorItem.ErrorMessage}");

                                    MessageBox.Show($"Ошибки валидации:\n{string.Join("\n", errorMessages)}");
                                    return;
                                }

                                db.SaveChanges();
                                MessageBox.Show("Данные клиента успешно обновлены");
                            }
                            else
                            {
                                MessageBox.Show("Клиент не найден в базе данных");
                                return;
                            }
                        }
                        else
                        {
                            // Создание нового клиента
                            var client = new Clients
                            {
                                Name = NameTextBox.Text.Trim(),
                                Surname = SurnameTextBox.Text.Trim(),
                                Phone = PhoneTextBox.Text.Trim(),
                                Address = AddressTextBox.Text.Trim(),
                                Email = EmailTextBox.Text.Trim(),
                                Login = LoginTextBox.Text.Trim(),
                                Password = PasswordBox.Password
                            };

                            // Проверка уникальности логина
                            if (db.Clients.Any(c => c.Login == client.Login))
                            {
                                MessageBox.Show("Пользователь с таким логином уже существует");
                                return;
                            }

                            db.Clients.Add(client);
                            db.SaveChanges();
                            MessageBox.Show("Новый клиент успешно сохранен");
                        }

                        NavigationService.Navigate(new ClientsPage());
                    }
                    catch (DbEntityValidationException ex)
                    {
                        var errorMessages = ex.EntityValidationErrors
                            .SelectMany(x => x.ValidationErrors)
                            .Select(x => $"{x.PropertyName}: {x.ErrorMessage}");

                        MessageBox.Show($"Ошибки валидации:\n{string.Join("\n", errorMessages)}");
                    }
                    catch (DbUpdateException dbEx)
                    {
                        MessageBox.Show($"Ошибка базы данных: {(dbEx.InnerException?.Message ?? dbEx.Message)}");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Неожиданная ошибка: {ex.Message}");
                    }
                }
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите имя клиента.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(SurnameTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите фамилию клиента.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(PhoneTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите телефон клиента.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите email клиента.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(LoginTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите логин клиента.");
                return false;
            }

            // Для нового клиента проверяем пароль
            if (Client == null && string.IsNullOrEmpty(PasswordBox.Password))
            {
                MessageBox.Show("Пожалуйста, введите пароль клиента.");
                return false;
            }

            return true;
        }
    }
}

