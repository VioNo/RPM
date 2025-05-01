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
    /// Логика взаимодействия для ClimatePage.xaml
    /// </summary>
    public partial class ClimatePage : Page
    {
        private List<Climate> _allClimates = new List<Climate>();

        public ClimatePage()
        {
            InitializeComponent();
            LoadClimates();
            this.IsVisibleChanged += Page_IsVisibleChanged;
        }

        private void LoadClimates()
        {
            using (var context = new DistilleryRassvetBase())
            {
                _allClimates = context.Climate.ToList();
                ListViewClimate.ItemsSource = _allClimates;
            }
        }

        private void ApplyFilters()
        {
            var filteredClimates = _allClimates.AsEnumerable();

            // Apply search filter (Climate Description)
            if (!string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                string searchText = SearchTextBox.Text.ToLower();
                filteredClimates = filteredClimates.Where(c =>
                    c.ClimateDescription != null && c.ClimateDescription.ToLower().Contains(searchText));
            }

            ListViewClimate.ItemsSource = filteredClimates.ToList();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void SortByDescription_Click(object sender, RoutedEventArgs e)
        {
            var climates = ListViewClimate.ItemsSource as IEnumerable<Climate> ?? _allClimates;

            ListViewClimate.ItemsSource = climates
                .OrderBy(c => c.ClimateDescription)
                .ToList();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            ClimateDialog climateDialog = new ClimateDialog(null);
            NavigationService.Navigate(climateDialog);
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedClimate = ListViewClimate.SelectedItem as Climate;
            if (selectedClimate != null)
            {
                ClimateDialog climateDialog = new ClimateDialog(selectedClimate);
                NavigationService.Navigate(climateDialog);
            }
            else
            {
                MessageBox.Show("Please select a climate to edit.",
                              "Warning",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var climatesToDelete = ListViewClimate.SelectedItems.Cast<Climate>().ToList();

            if (climatesToDelete.Count == 0)
            {
                MessageBox.Show("Please select at least one climate to delete.",
                              "Warning",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                return;
            }

            var confirmation = MessageBox.Show($"Are you sure you want to delete {climatesToDelete.Count} climate(s)?",
                                            "Delete Confirmation",
                                            MessageBoxButton.YesNo,
                                            MessageBoxImage.Question);

            if (confirmation != MessageBoxResult.Yes)
                return;

            try
            {
                using (var context = new DistilleryRassvetBase())
                {
                    var climateIds = climatesToDelete.Select(c => c.IDClimate).ToList();

                    var existingClimates = context.Climate
                        .Where(c => climateIds.Contains(c.IDClimate))
                        .ToList();

                    if (existingClimates.Count == 0)
                    {
                        MessageBox.Show("Selected climates not found in the database.",
                                      "Error",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Error);
                        return;
                    }

                    context.Climate.RemoveRange(existingClimates);
                    int deletedCount = context.SaveChanges();

                    if (deletedCount > 0)
                    {
                        MessageBox.Show($"Successfully deleted {deletedCount} climate(s).",
                                      "Success",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Information);
                        LoadClimates();
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete climates.",
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
                    MessageBox.Show("Cannot delete climate as it is related to other data in the system.",
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
                LoadClimates();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new DescriptionDB());
        }
    }
}
