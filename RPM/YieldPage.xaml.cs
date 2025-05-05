using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
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
    /// Логика взаимодействия для YieldPage.xaml
    /// </summary>
    public partial class YieldPage : Page
    {
        private List<Yield> _allYields = new List<Yield>();

        public YieldPage()
        {
            InitializeComponent();
            LoadYields();
            this.IsVisibleChanged += Page_IsVisibleChanged;
        }

        private void LoadYields()
        {
            using (var context = new DistilleryRassvetBase())
            {
                _allYields = context.Yield
                    .Include("GrapeVarieties")
                    .ToList();
                ListViewYield.ItemsSource = _allYields;
            }
        }

        private void ApplyFilters()
        {
            var filteredYields = _allYields.AsEnumerable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                string searchText = SearchTextBox.Text.ToLower();
                filteredYields = filteredYields.Where(y =>
                    y.GrapeVarieties != null &&
                    y.GrapeVarieties.NameGrapeVarieties != null &&
                    y.GrapeVarieties.NameGrapeVarieties.ToLower().Contains(searchText));
            }

            // Apply date filters
            if (StartDateFilter.SelectedDate.HasValue)
            {
                filteredYields = filteredYields.Where(y =>
                    y.DateYield >= StartDateFilter.SelectedDate.Value);
            }

            ListViewYield.ItemsSource = filteredYields.ToList();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void DateFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void SortByHarvest_Click(object sender, RoutedEventArgs e)
        {
            var yields = ListViewYield.ItemsSource as IEnumerable<Yield> ?? _allYields;
            ListViewYield.ItemsSource = yields
                .OrderBy(y => y.Harvest)
                .ToList();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            YieldDialog yieldDialog = new YieldDialog(null);
            NavigationService.Navigate(yieldDialog);
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedYield = ListViewYield.SelectedItem as Yield;
            if (selectedYield != null)
            {
                YieldDialog yieldDialog = new YieldDialog(selectedYield);
                NavigationService.Navigate(yieldDialog);
            }
            else
            {
                MessageBox.Show("Please select a yield to edit.",
                              "Warning",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var yieldsToDelete = ListViewYield.SelectedItems.Cast<Yield>().ToList();

            if (yieldsToDelete.Count == 0)
            {
                MessageBox.Show("Please select at least one yield to delete.",
                              "Warning",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                return;
            }

            var confirmation = MessageBox.Show($"Are you sure you want to delete {yieldsToDelete.Count} yield(s)?",
                                            "Confirm Deletion",
                                            MessageBoxButton.YesNo,
                                            MessageBoxImage.Question);

            if (confirmation != MessageBoxResult.Yes)
                return;

            try
            {
                using (var context = new DistilleryRassvetBase())
                {
                    var yieldIds = yieldsToDelete.Select(y => y.IDYield).ToList();

                    var existingYields = context.Yield
                        .Where(y => yieldIds.Contains(y.IDYield))
                        .ToList();

                    if (existingYields.Count == 0)
                    {
                        MessageBox.Show("Selected yields not found in database.",
                                      "Error",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Error);
                        return;
                    }

                    // Проверка на связанные записи
                    foreach (var yield in existingYields)
                    {
                        if (yield.Fermentation != null && yield.Fermentation.Any())
                        {
                            MessageBox.Show($"Cannot delete yield from {yield.DateYield:dd.MM.yyyy} because it's used in fermentation processes.",
                                          "Error",
                                          MessageBoxButton.OK,
                                          MessageBoxImage.Error);
                            return;
                        }
                    }

                    context.Yield.RemoveRange(existingYields);
                    int deletedCount = context.SaveChanges();

                    if (deletedCount > 0)
                    {
                        MessageBox.Show($"Successfully deleted {deletedCount} yield(s).",
                                      "Success",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Information);
                        LoadYields();
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete yields.",
                                      "Error",
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
                    MessageBox.Show("Cannot delete yield as it's linked to other data in the system.",
                                  "Error",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show($"Database error: {errorMessage}",
                                  "Error",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}",
                              "Error",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.Visibility == Visibility.Visible)
            {
                LoadYields();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new DescriptionDB());
        }
    }
}
