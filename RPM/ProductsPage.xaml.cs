using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace RPM
{
    public partial class ProductsPage : Page
    {
        private List<Products> _allProducts = new List<Products>();
        private string _currentSort = "id_asc";
        private string _currentSearch = "";
        private string _currentPartyFilter = "";
        private string _currentStorageFilter = "";
        private bool _isInitialized = false;

        public ProductsPage()
        {
            try
            {
                InitializeComponent();
                _isInitialized = true;
                LoadProducts();
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

        private void LoadProducts()
        {
            try
            {
                using (var context = new DistilleryRassvetBase())
                {
                    _allProducts = context.Products.ToList() ?? new List<Products>();
                    ApplyFiltersAndSort();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
                _allProducts = new List<Products>();
                ApplyFiltersAndSort();
            }
        }

        private void ApplyFiltersAndSort()
        {
            if (!_isInitialized || ListViewProducts == null || _allProducts == null)
                return;

            try
            {
                IEnumerable<Products> result = _allProducts.AsEnumerable();

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(_currentSearch))
                {
                    result = result.Where(p => p.Name != null &&
                                           p.Name.ToLower().Contains(_currentSearch.ToLower()));
                }

                // Apply party ID filter
                if (!string.IsNullOrWhiteSpace(_currentPartyFilter))
                {
                    if (int.TryParse(_currentPartyFilter, out int partyId))
                    {
                        result = result.Where(p => p.IDParty == partyId);
                    }
                }

                // Apply storage ID filter
                if (!string.IsNullOrWhiteSpace(_currentStorageFilter))
                {
                    if (int.TryParse(_currentStorageFilter, out int storageId))
                    {
                        result = result.Where(p => p.IDStorage == storageId);
                    }
                }

                // Apply sorting
                switch (_currentSort)
                {
                    case "id_asc":
                        result = result.OrderBy(p => p?.IDProduct ?? 0);
                        break;
                    case "id_desc":
                        result = result.OrderByDescending(p => p?.IDProduct ?? 0);
                        break;
                    case "name_asc":
                        result = result.OrderBy(p => p?.Name ?? "");
                        break;
                    case "name_desc":
                        result = result.OrderByDescending(p => p?.Name ?? "");
                        break;
                    case "cost_asc":
                        result = result.OrderBy(p => p?.primecost ?? 0);
                        break;
                    case "cost_desc":
                        result = result.OrderByDescending(p => p?.primecost ?? 0);
                        break;
                    default:
                        result = result.OrderBy(p => p?.IDProduct ?? 0);
                        break;
                }

                ListViewProducts.ItemsSource = result.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при фильтрации и сортировке: {ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _currentSearch = SearchTextBox?.Text ?? "";
            ApplyFiltersAndSort();
        }

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SortComboBox?.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag != null)
            {
                _currentSort = selectedItem.Tag.ToString();
                ApplyFiltersAndSort();
            }
        }

        private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender == PartyFilterTextBox)
            {
                _currentPartyFilter = PartyFilterTextBox?.Text ?? "";
            }
            else if (sender == StorageFilterTextBox)
            {
                _currentStorageFilter = StorageFilterTextBox?.Text ?? "";
            }
            ApplyFiltersAndSort();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new ProductDialog(null));
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
                if (ListViewProducts?.SelectedItem is Products selectedProduct)
                {
                    NavigationService?.Navigate(new ProductDialog(selectedProduct));
                }
                else
                {
                    MessageBox.Show("Пожалуйста, выберите один элемент для редактирования.",
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
                if (ListViewProducts?.SelectedItems == null)
                {
                    MessageBox.Show("Пожалуйста, выберите хотя бы один продукт для удаления.",
                                  "Предупреждение",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Warning);
                    return;
                }

                var productsToDelete = ListViewProducts.SelectedItems.Cast<Products>().ToList();
                if (productsToDelete.Count == 0)
                {
                    MessageBox.Show("Пожалуйста, выберите хотя бы один продукт для удаления.",
                                  "Предупреждение",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Warning);
                    return;
                }

                var confirmation = MessageBox.Show($"Вы точно хотите удалить продукты в кол-ве {productsToDelete.Count}?",
                                                "Подтверждение удаления",
                                                MessageBoxButton.YesNo,
                                                MessageBoxImage.Question);

                if (confirmation != MessageBoxResult.Yes)
                    return;

                using (var context = new DistilleryRassvetBase())
                {
                    var productIds = productsToDelete.Select(p => p.IDProduct).ToList();

                    var existingProducts = context.Products
                        .Where(p => productIds.Contains(p.IDProduct))
                        .ToList();

                    if (existingProducts.Count == 0)
                    {
                        MessageBox.Show("Выбранные продукты не найдены в базе данных.",
                                      "Ошибка",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Error);
                        return;
                    }

                    context.Products.RemoveRange(existingProducts);
                    int deletedCount = context.SaveChanges();

                    if (deletedCount > 0)
                    {
                        MessageBox.Show($"Успешно удалено продукты в кол-ве {deletedCount}.",
                                      "Успех",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Information);
                        LoadProducts();
                    }
                    else
                    {
                        MessageBox.Show("Не удалось удалить продукты.",
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
                    MessageBox.Show("Невозможно удалить продукт, так как он используется в других таблицах.",
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
            if (this.Visibility == Visibility.Visible && _isInitialized)
            {
                LoadProducts();
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