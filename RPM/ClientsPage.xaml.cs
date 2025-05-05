using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace RPM
{
    public partial class ClientsPage : Page
    {
        private List<Clients> _allClients = new List<Clients>();

        public ClientsPage()
        {
            InitializeComponent();
            LoadClients();
            this.IsVisibleChanged += Page_IsVisibleChanged;
        }

        private void LoadClients()
        {
            using (var context = new DistilleryRassvetBase())
            {
                _allClients = context.Clients.ToList();
                ListViewClients.ItemsSource = _allClients;
            }
        }

        private void ApplyFilters()
        {
            var filteredClients = _allClients.AsEnumerable();

            // Apply search filter (Name or Surname)
            if (!string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                string searchText = SearchTextBox.Text.ToLower();
                filteredClients = filteredClients.Where(c =>
                    (c.Name != null && c.Name.ToLower().Contains(searchText)) ||
                    (c.Surname != null && c.Surname.ToLower().Contains(searchText)));
            }

            // Apply email filter
            if (!string.IsNullOrWhiteSpace(EmailFilterTextBox.Text))
            {
                string emailFilter = EmailFilterTextBox.Text.ToLower();
                filteredClients = filteredClients.Where(c =>
                    c.Email != null && c.Email.ToLower().Contains(emailFilter));
            }

            ListViewClients.ItemsSource = filteredClients.ToList();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void EmailFilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void SortByPhone_Click(object sender, RoutedEventArgs e)
        {
            var clients = ListViewClients.ItemsSource as IEnumerable<Clients> ?? _allClients;

            ListViewClients.ItemsSource = clients
                .OrderBy(c => GetFirstDigitInPhone(c.Phone))
                .ToList();
        }

        private int GetFirstDigitInPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return int.MaxValue; // Put empty phones at the end

            var match = Regex.Match(phone, @"\d");
            return match.Success ? int.Parse(match.Value) : int.MaxValue;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            ClientsDialog clientsDialog = new ClientsDialog(null);
            NavigationService.Navigate(clientsDialog);
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedClient = ListViewClients.SelectedItem as Clients;
            if (selectedClient != null)
            {
                ClientsDialog clientsDialog = new ClientsDialog(selectedClient);
                NavigationService.Navigate(clientsDialog);
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите клиента для редактирования.",
                              "Предупреждение",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var clientsToDelete = ListViewClients.SelectedItems.Cast<Clients>().ToList();

            if (clientsToDelete.Count == 0)
            {
                MessageBox.Show("Пожалуйста, выберите хотя бы одного клиента для удаления.",
                              "Предупреждение",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                return;
            }

            var confirmation = MessageBox.Show($"Вы точно хотите удалить {clientsToDelete.Count} клиента(ов)?",
                                            "Подтверждение удаления",
                                            MessageBoxButton.YesNo,
                                            MessageBoxImage.Question);

            if (confirmation != MessageBoxResult.Yes)
                return;

            try
            {
                using (var context = new DistilleryRassvetBase())
                {
                    var clientIds = clientsToDelete.Select(c => c.IDClient).ToList();

                    var existingClients = context.Clients
                        .Where(c => clientIds.Contains(c.IDClient))
                        .ToList();

                    if (existingClients.Count == 0)
                    {
                        MessageBox.Show("Выбранные клиенты не найдены в базе данных.",
                                      "Ошибка",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Error);
                        return;
                    }

                    context.Clients.RemoveRange(existingClients);
                    int deletedCount = context.SaveChanges();

                    if (deletedCount > 0)
                    {
                        MessageBox.Show($"Успешно удалено {deletedCount} клиента(ов).",
                                      "Успех",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Information);
                        LoadClients();
                    }
                    else
                    {
                        MessageBox.Show("Не удалось удалить клиентов.",
                                      "Ошибка",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Error);
                    }
                }
            }
            catch (DbUpdateException dbEx)
            {
                string errorMessage = dbEx.InnerException?.Message ?? dbEx.Message;

                if (errorMessage.Contains("FK_") || errorMessage.Contains("foreign key"))
                {
                    MessageBox.Show("Невозможно удалить клиента, так как он связан с другими данными в системе.",
                                  "Ошибка",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show($"Ошибка базы данных: {errorMessage}",
                                  "Ошибка",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.Visibility == Visibility.Visible)
            {
                LoadClients();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new DescriptionDB());
        }
    }
}