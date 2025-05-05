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
    /// Логика взаимодействия для SoilPage.xaml
    /// </summary>
    public partial class SoilPage : Page
    {
        private List<Soil> _allSoils = new List<Soil>();

        public SoilPage()
        {
            InitializeComponent();
            LoadSoils();
            this.IsVisibleChanged += Page_IsVisibleChanged;
        }

        private void LoadSoils()
        {
            using (var context = new DistilleryRassvetBase())
            {
                _allSoils = context.Soil.ToList();
                ListViewSoil.ItemsSource = _allSoils;
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                ListViewSoil.ItemsSource = _allSoils;
            }
            else
            {
                string searchText = SearchTextBox.Text.ToLower();
                ListViewSoil.ItemsSource = _allSoils
                    .Where(s => s.SoilDescription != null &&
                               s.SoilDescription.ToLower().Contains(searchText))
                    .ToList();
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            SoilDialog soilDialog = new SoilDialog(null);
            NavigationService.Navigate(soilDialog);
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedSoil = ListViewSoil.SelectedItem as Soil;
            if (selectedSoil != null)
            {
                SoilDialog soilDialog = new SoilDialog(selectedSoil);
                NavigationService.Navigate(soilDialog);
            }
            else
            {
                MessageBox.Show("Please select a soil to edit.",
                              "Warning",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var soilsToDelete = ListViewSoil.SelectedItems.Cast<Soil>().ToList();

            if (soilsToDelete.Count == 0)
            {
                MessageBox.Show("Please select at least one soil to delete.",
                              "Warning",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                return;
            }

            var confirmation = MessageBox.Show($"Are you sure you want to delete {soilsToDelete.Count} soil(s)?",
                                            "Confirm Deletion",
                                            MessageBoxButton.YesNo,
                                            MessageBoxImage.Question);

            if (confirmation != MessageBoxResult.Yes)
                return;

            try
            {
                using (var context = new DistilleryRassvetBase())
                {
                    var soilIds = soilsToDelete.Select(s => s.IDSoil).ToList();

                    var existingSoils = context.Soil
                        .Where(s => soilIds.Contains(s.IDSoil))
                        .ToList();

                    if (existingSoils.Count == 0)
                    {
                        MessageBox.Show("Selected soils not found in database.",
                                      "Error",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Error);
                        return;
                    }

                    // Проверка на связанные записи
                    foreach (var soil in existingSoils)
                    {
                        if (soil.GrowingConditions != null && soil.GrowingConditions.Any())
                        {
                            MessageBox.Show($"Cannot delete soil '{soil.SoilDescription}' because it's used in growing conditions.",
                                          "Error",
                                          MessageBoxButton.OK,
                                          MessageBoxImage.Error);
                            return;
                        }
                    }

                    context.Soil.RemoveRange(existingSoils);
                    int deletedCount = context.SaveChanges();

                    if (deletedCount > 0)
                    {
                        MessageBox.Show($"Successfully deleted {deletedCount} soil(s).",
                                      "Success",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Information);
                        LoadSoils();
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete soils.",
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
                    MessageBox.Show("Cannot delete soil as it's linked to other data in the system.",
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
                LoadSoils();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new DescriptionDB());
        }
    }
}
