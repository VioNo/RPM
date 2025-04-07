using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace RPM
{
    public partial class StoragePage : Page
    {
        public StoragePage()
        {
            InitializeComponent();
            LoadStorage();
            this.IsVisibleChanged += Page_IsVisibleChanged;
        }

        private void LoadStorage()
        {
            using (var context = new DistilleryRassvetBase())
            {
                var storage = context.Storage.ToList();
                DataGridStorage.ItemsSource = storage;
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            StorageDialog dialog = new StorageDialog(null);
            NavigationService.Navigate(dialog);
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedStorage = DataGridStorage.SelectedItem as Storage;
            if (selectedStorage != null)
            {
                StorageDialog dialog = new StorageDialog(selectedStorage);
                NavigationService.Navigate(dialog);
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите склад для редактирования.",
                              "Предупреждение",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var storageToDelete = DataGridStorage.SelectedItems.Cast<Storage>().ToList();

            if (storageToDelete.Count == 0)
            {
                MessageBox.Show("Пожалуйста, выберите хотя бы один склад для удаления.",
                              "Предупреждение",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                return;
            }

            var confirmation = MessageBox.Show($"Вы точно хотите удалить {storageToDelete.Count} склад(ов)?",
                                          "Подтверждение удаления",
                                          MessageBoxButton.YesNo,
                                          MessageBoxImage.Question);

            if (confirmation != MessageBoxResult.Yes) return;

            try
            {
                using (var context = new DistilleryRassvetBase())
                {
                    var storageIds = storageToDelete.Select(s => s.IDStorage).ToList();
                    var existingStorage = context.Storage
                        .Where(s => storageIds.Contains(s.IDStorage))
                        .ToList();

                    context.Storage.RemoveRange(existingStorage);
                    int deletedCount = context.SaveChanges();

                    MessageBox.Show($"Успешно удалено {deletedCount} склад(ов).",
                                  "Успех",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);
                    LoadStorage();
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

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.Visibility == Visibility.Visible)
            {
                LoadStorage();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new DescriptionDB());
        }
    }
}