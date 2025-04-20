using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace RPM
{
    public partial class GrapeVarietiesPage : Page
    {
        private List<GrapeVarieties> _allVarieties;
        private string _currentSort = "id_asc";
        private string _currentSearch = "";
        private string _currentFilter = "";

        public GrapeVarietiesPage()
        {
            InitializeComponent();
            LoadGrapeVarieties();
            this.IsVisibleChanged += Page_IsVisibleChanged;
        }

        private void LoadGrapeVarieties()
        {
            using (var context = new DistilleryRassvetBase())
            {
                _allVarieties = context.GrapeVarieties.ToList();
                ApplyFiltersAndSort();
            }
        }

        private void ApplyFiltersAndSort()
        {
            IEnumerable<GrapeVarieties> result = _allVarieties;

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(_currentSearch))
            {
                result = result.Where(v => v.NameGrapeVarieties != null &&
                                         v.NameGrapeVarieties.ToLower().Contains(_currentSearch.ToLower()));
            }

            // Apply description filter
            if (!string.IsNullOrWhiteSpace(_currentFilter))
            {
                result = result.Where(v => v.Description != null &&
                                         v.Description.ToLower().Contains(_currentFilter.ToLower()));
            }

            // Apply sorting
            switch (_currentSort)
            {
                case "id_asc":
                    result = result.OrderBy(v => v.IDGrapeVarieties);
                    break;
                case "id_desc":
                    result = result.OrderByDescending(v => v.IDGrapeVarieties);
                    break;
                case "name_asc":
                    result = result.OrderBy(v => v.NameGrapeVarieties);
                    break;
                case "name_desc":
                    result = result.OrderByDescending(v => v.NameGrapeVarieties);
                    break;
                default:
                    result = result.OrderBy(v => v.IDGrapeVarieties);
                    break;
            }

            ListViewGrapeVarieties.ItemsSource = result.ToList();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _currentSearch = SearchTextBox.Text;
            ApplyFiltersAndSort();
        }

        private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _currentFilter = FilterTextBox.Text;
            ApplyFiltersAndSort();
        }

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SortComboBox.SelectedItem != null)
            {
                var selectedItem = (ComboBoxItem)SortComboBox.SelectedItem;
                _currentSort = selectedItem.Tag.ToString();
                ApplyFiltersAndSort();
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            GrapeVarietiesDialog dialog = new GrapeVarietiesDialog(null);
            NavigationService.Navigate(dialog);
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedVariety = ListViewGrapeVarieties.SelectedItem as GrapeVarieties;
            if (selectedVariety != null)
            {
                GrapeVarietiesDialog dialog = new GrapeVarietiesDialog(selectedVariety);
                NavigationService.Navigate(dialog);
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите сорт для редактирования.",
                              "Предупреждение",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var varietiesToDelete = ListViewGrapeVarieties.SelectedItems.Cast<GrapeVarieties>().ToList();

            if (varietiesToDelete.Count == 0)
            {
                MessageBox.Show("Пожалуйста, выберите хотя бы один сорт для удаления.",
                              "Предупреждение",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                return;
            }

            var confirmation = MessageBox.Show($"Вы точно хотите удалить {varietiesToDelete.Count} сорт(ов)?",
                                          "Подтверждение удаления",
                                          MessageBoxButton.YesNo,
                                          MessageBoxImage.Question);

            if (confirmation != MessageBoxResult.Yes) return;

            try
            {
                using (var context = new DistilleryRassvetBase())
                {
                    var varietyIds = varietiesToDelete.Select(v => v.IDGrapeVarieties).ToList();
                    var existingVarieties = context.GrapeVarieties
                        .Where(v => varietyIds.Contains(v.IDGrapeVarieties))
                        .ToList();

                    context.GrapeVarieties.RemoveRange(existingVarieties);
                    int deletedCount = context.SaveChanges();

                    MessageBox.Show($"Успешно удалено {deletedCount} сорт(ов).",
                                  "Успех",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);
                    LoadGrapeVarieties();
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
                LoadGrapeVarieties();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new DescriptionDB());
        }
    }
}