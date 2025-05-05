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
    /// Логика взаимодействия для WaterPage.xaml
    /// </summary>
    public partial class WaterPage : Page
    {
        private List<Water> _allWaters = new List<Water>();

        public WaterPage()
        {
            InitializeComponent();
            LoadWaters();
            this.IsVisibleChanged += Page_IsVisibleChanged;
        }

        private void LoadWaters()
        {
            using (var context = new DistilleryRassvetBase())
            {
                _allWaters = context.Water.ToList();
                ListViewWater.ItemsSource = _allWaters;
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                ListViewWater.ItemsSource = _allWaters;
            }
            else
            {
                string searchText = SearchTextBox.Text.ToLower();
                ListViewWater.ItemsSource = _allWaters
                    .Where(w => w.WaterDescription != null &&
                               w.WaterDescription.ToLower().Contains(searchText))
                    .ToList();
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            WaterDialog waterDialog = new WaterDialog(null);
            NavigationService.Navigate(waterDialog);
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedWater = ListViewWater.SelectedItem as Water;
            if (selectedWater != null)
            {
                WaterDialog waterDialog = new WaterDialog(selectedWater);
                NavigationService.Navigate(waterDialog);
            }
            else
            {
                MessageBox.Show("Please select a water type to edit.",
                              "Warning",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var watersToDelete = ListViewWater.SelectedItems.Cast<Water>().ToList();

            if (watersToDelete.Count == 0)
            {
                MessageBox.Show("Please select at least one water type to delete.",
                              "Warning",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                return;
            }

            var confirmation = MessageBox.Show($"Are you sure you want to delete {watersToDelete.Count} water type(s)?",
                                            "Confirm Deletion",
                                            MessageBoxButton.YesNo,
                                            MessageBoxImage.Question);

            if (confirmation != MessageBoxResult.Yes)
                return;

            try
            {
                using (var context = new DistilleryRassvetBase())
                {
                    var waterIds = watersToDelete.Select(w => w.IDWater).ToList();

                    var existingWaters = context.Water
                        .Where(w => waterIds.Contains(w.IDWater))
                        .ToList();

                    if (existingWaters.Count == 0)
                    {
                        MessageBox.Show("Selected water types not found in database.",
                                      "Error",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Error);
                        return;
                    }

                    // Проверка на связанные записи
                    foreach (var water in existingWaters)
                    {
                        if (water.GrowingConditions != null && water.GrowingConditions.Any())
                        {
                            MessageBox.Show($"Cannot delete water type '{water.WaterDescription}' because it's used in growing conditions.",
                                          "Error",
                                          MessageBoxButton.OK,
                                          MessageBoxImage.Error);
                            return;
                        }
                    }

                    context.Water.RemoveRange(existingWaters);
                    int deletedCount = context.SaveChanges();

                    if (deletedCount > 0)
                    {
                        MessageBox.Show($"Successfully deleted {deletedCount} water type(s).",
                                      "Success",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Information);
                        LoadWaters();
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete water types.",
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
                    MessageBox.Show("Cannot delete water type as it's linked to other data in the system.",
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
                LoadWaters();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new DescriptionDB());
        }
    }
}
