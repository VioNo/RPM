using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace RPM
{
    /// <summary>
    /// Логика взаимодействия для ClientsPage.xaml
    /// </summary>
    public partial class ClientsPage : Page
    {
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
                var clients = context.Clients.ToList();
                ListViewClients.ItemsSource = clients;
            }
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
            // Получаем выбранных клиентов
            var clientsToDelete = ListViewClients.SelectedItems.Cast<Clients>().ToList();

            // Проверяем, что есть что удалять
            if (clientsToDelete.Count == 0)
            {
                MessageBox.Show("Пожалуйста, выберите хотя бы одного клиента для удаления.",
                              "Предупреждение",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                return;
            }

            // Запрашиваем подтверждение
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
                    // Получаем только ID выбранных клиентов
                    var clientIds = clientsToDelete.Select(c => c.IDClient).ToList();

                    // Находим клиентов в базе данных
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

                    // Удаляем клиентов
                    context.Clients.RemoveRange(existingClients);
                    int deletedCount = context.SaveChanges();

                    if (deletedCount > 0)
                    {
                        MessageBox.Show($"Успешно удалено {deletedCount} клиента(ов).",
                                      "Успех",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Information);
                        LoadClients(); // Обновляем список
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

                // Проверяем, является ли ошибка нарушением внешнего ключа
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