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
    /// Логика взаимодействия для GrowingConditionsPage.xaml
    /// </summary>
    public partial class GrowingConditionsPage : Page
    {
        private List<GrowingConditions> _allGrowingConditions = new List<GrowingConditions>();

        public GrowingConditionsPage()
        {
            InitializeComponent();
            LoadGrowingConditions();
            this.IsVisibleChanged += Page_IsVisibleChanged;
        }

        private void LoadGrowingConditions()
        {
            using (var context = new DistilleryRassvetBase())
            {
                _allGrowingConditions = context.GrowingConditions
                    .Include("GrapeVarieties")
                    .Include("Soil")
                    .Include("Water")
                    .Include("Climate")
                    .ToList();
                ListViewGrowingConditions.ItemsSource = _allGrowingConditions;
            }
        }

        private void ApplyFilters()
        {
            var filteredConditions = _allGrowingConditions.AsEnumerable();

            // Apply search filter (Description)
            if (!string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                string searchText = SearchTextBox.Text.ToLower();
                filteredConditions = filteredConditions.Where(g =>
                    g.Description != null && g.Description.ToLower().Contains(searchText));
            }

            ListViewGrowingConditions.ItemsSource = filteredConditions.ToList();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void SortByGrapeVariety_Click(object sender, RoutedEventArgs e)
        {
            var conditions = ListViewGrowingConditions.ItemsSource as IEnumerable<GrowingConditions> ?? _allGrowingConditions;
            ListViewGrowingConditions.ItemsSource = conditions
                .OrderBy(g => g.IDGrapeVarieties)
                .ToList();
        }

        private void SortByClimate_Click(object sender, RoutedEventArgs e)
        {
            var conditions = ListViewGrowingConditions.ItemsSource as IEnumerable<GrowingConditions> ?? _allGrowingConditions;
            ListViewGrowingConditions.ItemsSource = conditions
                .OrderBy(g => g.IDClimate)
                .ToList();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            GrowingConditionsDialog dialog = new GrowingConditionsDialog(null);
            NavigationService.Navigate(dialog);
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedCondition = ListViewGrowingConditions.SelectedItem as GrowingConditions;
            if (selectedCondition != null)
            {
                GrowingConditionsDialog dialog = new GrowingConditionsDialog(selectedCondition);
                NavigationService.Navigate(dialog);
            }
            else
            {
                MessageBox.Show("Please select a growing condition to edit.",
                              "Warning",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var conditionsToDelete = ListViewGrowingConditions.SelectedItems.Cast<GrowingConditions>().ToList();

            if (conditionsToDelete.Count == 0)
            {
                MessageBox.Show("Please select at least one growing condition to delete.",
                              "Warning",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                return;
            }

            var confirmation = MessageBox.Show($"Are you sure you want to delete {conditionsToDelete.Count} growing condition(s)?",
                                            "Confirm Deletion",
                                            MessageBoxButton.YesNo,
                                            MessageBoxImage.Question);

            if (confirmation != MessageBoxResult.Yes)
                return;

            try
            {
                using (var context = new DistilleryRassvetBase())
                {
                    var conditionIds = conditionsToDelete.Select(g => g.IDGrowingConditions).ToList();

                    var existingConditions = context.GrowingConditions
                        .Where(g => conditionIds.Contains(g.IDGrowingConditions))
                        .ToList();

                    if (existingConditions.Count == 0)
                    {
                        MessageBox.Show("Selected growing conditions not found in database.",
                                      "Error",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Error);
                        return;
                    }

                    context.GrowingConditions.RemoveRange(existingConditions);
                    int deletedCount = context.SaveChanges();

                    if (deletedCount > 0)
                    {
                        MessageBox.Show($"Successfully deleted {deletedCount} growing condition(s).",
                                      "Success",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Information);
                        LoadGrowingConditions();
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete growing conditions.",
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
                    MessageBox.Show("Cannot delete growing condition as it's linked to fermentation processes.",
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
                LoadGrowingConditions();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new DescriptionDB());
        }
    }
}

