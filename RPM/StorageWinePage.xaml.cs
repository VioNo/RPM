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
    /// Логика взаимодействия для StorageWinePage.xaml
    /// </summary>
    public partial class StorageWinePage : Page
    {
        private List<StorageWine> _allStorageWines = new List<StorageWine>();

        public StorageWinePage()
        {
            InitializeComponent();
            LoadStorageWines();
            this.IsVisibleChanged += Page_IsVisibleChanged;
        }

        private void LoadStorageWines()
        {
            using (var context = new DistilleryRassvetBase())
            {
                _allStorageWines = context.StorageWine
                    .Include("Fermentation")
                    .Include("Party")
                    .ToList();
                ListViewStorageWine.ItemsSource = _allStorageWines;
            }
        }

        private void ApplyFilters()
        {
            var filteredStorageWines = _allStorageWines.AsEnumerable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                string searchText = SearchTextBox.Text.ToLower();
                filteredStorageWines = filteredStorageWines.Where(sw =>
                    (sw.Fermentation?.Description != null && sw.Fermentation.Description.ToLower().Contains(searchText)) ||
                    (sw.Party?.PartyDescription != null && sw.Party.PartyDescription.ToLower().Contains(searchText)));
            }

            // Apply date filter
            if (ExpirationDateFilter.SelectedDate.HasValue)
            {
                filteredStorageWines = filteredStorageWines.Where(sw =>
                    sw.ExpirationDate.Date == ExpirationDateFilter.SelectedDate.Value.Date);
            }

            ListViewStorageWine.ItemsSource = filteredStorageWines.ToList();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void DateFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void SortByCount_Click(object sender, RoutedEventArgs e)
        {
            var storageWines = ListViewStorageWine.ItemsSource as IEnumerable<StorageWine> ?? _allStorageWines;
            ListViewStorageWine.ItemsSource = storageWines
                .OrderBy(sw => sw.Count ?? 0)
                .ToList();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            StorageWineDialog storageWineDialog = new StorageWineDialog(null);
            NavigationService.Navigate(storageWineDialog);
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedStorageWine = ListViewStorageWine.SelectedItem as StorageWine;
            if (selectedStorageWine != null)
            {
                StorageWineDialog storageWineDialog = new StorageWineDialog(selectedStorageWine);
                NavigationService.Navigate(storageWineDialog);
            }
            else
            {
                MessageBox.Show("Please select a wine storage record to edit.",
                              "Warning",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var storageWinesToDelete = ListViewStorageWine.SelectedItems.Cast<StorageWine>().ToList();

            if (storageWinesToDelete.Count == 0)
            {
                MessageBox.Show("Please select at least one wine storage record to delete.",
                              "Warning",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                return;
            }

            var confirmation = MessageBox.Show($"Are you sure you want to delete {storageWinesToDelete.Count} wine storage record(s)?",
                                            "Confirm Deletion",
                                            MessageBoxButton.YesNo,
                                            MessageBoxImage.Question);

            if (confirmation != MessageBoxResult.Yes)
                return;

            try
            {
                using (var context = new DistilleryRassvetBase())
                {
                    var storageWineIds = storageWinesToDelete.Select(sw => sw.IDStorageWine).ToList();

                    var existingStorageWines = context.StorageWine
                        .Where(sw => storageWineIds.Contains(sw.IDStorageWine))
                        .ToList();

                    if (existingStorageWines.Count == 0)
                    {
                        MessageBox.Show("Selected wine storage records not found in database.",
                                      "Error",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Error);
                        return;
                    }

                    context.StorageWine.RemoveRange(existingStorageWines);
                    int deletedCount = context.SaveChanges();

                    if (deletedCount > 0)
                    {
                        MessageBox.Show($"Successfully deleted {deletedCount} wine storage record(s).",
                                      "Success",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Information);
                        LoadStorageWines();
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete wine storage records.",
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
                    MessageBox.Show("Cannot delete wine storage record as it's linked to other data in the system.",
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
                LoadStorageWines();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new DescriptionDB());
        }
    }
}
