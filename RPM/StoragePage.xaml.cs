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
        private List<Storage> _allStorages;
        private bool _sortAscending = true;
        private bool _isInitialized = false;

        public StoragePage()
        {
            try
            {
                InitializeComponent();
                _allStorages = new List<Storage>();
                _isInitialized = true;
                this.Loaded += Page_Loaded;
                this.IsVisibleChanged += Page_IsVisibleChanged;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации страницы: {ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isInitialized && DataGridStorage != null)
            {
                LoadStorage();
            }
        }

        private void LoadStorage()
        {
            try
            {
                using (var context = new DistilleryRassvetBase())
                {
                    _allStorages = context.Storage.ToList() ?? new List<Storage>();
                    SafeApplyFiltersAndSearch();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
                _allStorages = new List<Storage>();
                SafeApplyFiltersAndSearch();
            }
        }

        private void SafeApplyFiltersAndSearch()
        {
            if (!_isInitialized || DataGridStorage == null) return;
            ApplyFiltersAndSearch();
        }

        private void ApplyFiltersAndSearch()
        {
            if (!_isInitialized || DataGridStorage == null || _allStorages == null)
                return;

            try
            {
                var filteredStorages = _allStorages.AsQueryable();

                // Apply search filter
                if (SearchTextBox != null && !string.IsNullOrWhiteSpace(SearchTextBox.Text))
                {
                    string searchText = SearchTextBox.Text.ToLower();
                    filteredStorages = filteredStorages.Where(s =>
                        s.Address != null && s.Address.ToLower().Contains(searchText));
                }

                // Apply fullness filter
                if (FilterComboBox?.SelectedItem is ComboBoxItem selectedItem)
                {
                    switch (selectedItem.Content.ToString())
                    {
                        case "Пустые (0%)":
                            filteredStorages = filteredStorages.Where(s => s.Fullness == 0);
                            break;
                        case "Менее 50%":
                            filteredStorages = filteredStorages.Where(s => s.Fullness > 0 && s.Fullness < 50);
                            break;
                        case "50-80%":
                            filteredStorages = filteredStorages.Where(s => s.Fullness >= 50 && s.Fullness <= 80);
                            break;
                        case "Более 80%":
                            filteredStorages = filteredStorages.Where(s => s.Fullness > 80 && s.Fullness < 100);
                            break;
                        case "Полные (100%)":
                            filteredStorages = filteredStorages.Where(s => s.Fullness == 100);
                            break;
                    }
                }

                // Apply sorting
                filteredStorages = _sortAscending
                    ? filteredStorages.OrderBy(s => s.IDStorage)
                    : filteredStorages.OrderByDescending(s => s.IDStorage);

                DataGridStorage.ItemsSource = filteredStorages.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при фильтрации данных: {ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SafeApplyFiltersAndSearch();
        }

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SafeApplyFiltersAndSearch();
        }

        private void SortButton_Click(object sender, RoutedEventArgs e)
        {
            _sortAscending = !_sortAscending;
            SafeApplyFiltersAndSearch();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new StorageDialog(null));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия формы: {ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataGridStorage?.SelectedItem is Storage selectedStorage)
                {
                    NavigationService?.Navigate(new StorageDialog(selectedStorage));
                }
                else
                {
                    MessageBox.Show("Пожалуйста, выберите склад для редактирования.",
                                  "Предупреждение",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия формы: {ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataGridStorage?.SelectedItems == null)
                {
                    MessageBox.Show("Пожалуйста, выберите хотя бы один склад для удаления.",
                                  "Предупреждение",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Warning);
                    return;
                }

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
            if (this.Visibility == Visibility.Visible && _isInitialized)
            {
                LoadStorage();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new DescriptionDB());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка навигации: {ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }
    }
}