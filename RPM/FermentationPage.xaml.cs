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
    /// Логика взаимодействия для FermentationPage.xaml
    /// </summary>
    public partial class FermentationPage : Page
    {
        private List<Fermentation> _allFermentations = new List<Fermentation>();

        public FermentationPage()
        {
            InitializeComponent();
            LoadFermentations();
            this.IsVisibleChanged += Page_IsVisibleChanged;
        }

        private void LoadFermentations()
        {
            using (var context = new DistilleryRassvetBase())
            {
                _allFermentations = context.Fermentation
                    .Include("Yield")
                    .Include("GrowingConditions")
                    .ToList();
                ListViewFermentation.ItemsSource = _allFermentations;
            }
        }

        private void ApplyFilters()
        {
            var filteredFermentations = _allFermentations.AsEnumerable();

            // Apply search filter (Description)
            if (!string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                string searchText = SearchTextBox.Text.ToLower();
                filteredFermentations = filteredFermentations.Where(f =>
                    f.Description != null && f.Description.ToLower().Contains(searchText));
            }

            // Apply date filters
            if (StartDateFilter.SelectedDate.HasValue)
            {
                filteredFermentations = filteredFermentations.Where(f =>
                    f.StartDate >= StartDateFilter.SelectedDate.Value);
            }

            if (EndDateFilter.SelectedDate.HasValue)
            {
                filteredFermentations = filteredFermentations.Where(f =>
                    f.EndDate <= EndDateFilter.SelectedDate.Value);
            }

            ListViewFermentation.ItemsSource = filteredFermentations.ToList();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void DateFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void SortByTemperature_Click(object sender, RoutedEventArgs e)
        {
            var fermentations = ListViewFermentation.ItemsSource as IEnumerable<Fermentation> ?? _allFermentations;
            ListViewFermentation.ItemsSource = fermentations
                .OrderBy(f => f.Temperature)
                .ToList();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            FermentationDialog fermentationDialog = new FermentationDialog(null);
            NavigationService.Navigate(fermentationDialog);
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedFermentation = ListViewFermentation.SelectedItem as Fermentation;
            if (selectedFermentation != null)
            {
                FermentationDialog fermentationDialog = new FermentationDialog(selectedFermentation);
                NavigationService.Navigate(fermentationDialog);
            }
            else
            {
                MessageBox.Show("Please select a fermentation process to edit.",
                              "Warning",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var fermentationsToDelete = ListViewFermentation.SelectedItems.Cast<Fermentation>().ToList();

            if (fermentationsToDelete.Count == 0)
            {
                MessageBox.Show("Please select at least one fermentation process to delete.",
                              "Warning",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                return;
            }

            var confirmation = MessageBox.Show($"Are you sure you want to delete {fermentationsToDelete.Count} fermentation process(es)?",
                                            "Confirm Deletion",
                                            MessageBoxButton.YesNo,
                                            MessageBoxImage.Question);

            if (confirmation != MessageBoxResult.Yes)
                return;

            try
            {
                using (var context = new DistilleryRassvetBase())
                {
                    var fermentationIds = fermentationsToDelete.Select(f => f.IDFermentation).ToList();

                    var existingFermentations = context.Fermentation
                        .Where(f => fermentationIds.Contains(f.IDFermentation))
                        .ToList();

                    if (existingFermentations.Count == 0)
                    {
                        MessageBox.Show("Selected fermentation processes not found in database.",
                                      "Error",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Error);
                        return;
                    }

                    context.Fermentation.RemoveRange(existingFermentations);
                    int deletedCount = context.SaveChanges();

                    if (deletedCount > 0)
                    {
                        MessageBox.Show($"Successfully deleted {deletedCount} fermentation process(es).",
                                      "Success",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Information);
                        LoadFermentations();
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete fermentation processes.",
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
                    MessageBox.Show("Cannot delete fermentation process as it's linked to other data in the system.",
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
                LoadFermentations();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new DescriptionDB());
        }
    }
}
