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
    /// Логика взаимодействия для ShipmentsPage.xaml
    /// </summary>
    public partial class ShipmentsPage : Page
    {
        private List<Shipments> _allShipments = new List<Shipments>();

        public ShipmentsPage()
        {
            InitializeComponent();
            LoadShipments();
            this.IsVisibleChanged += Page_IsVisibleChanged;
        }

        private void LoadShipments()
        {
            using (var context = new DistilleryRassvetBase())
            {
                _allShipments = context.Shipments
                    .Include("Party")
                    .Include("Storage")
                    .ToList();
                ListViewShipments.ItemsSource = _allShipments;
            }
        }

        private void ApplyFilters()
        {
            var filteredShipments = _allShipments.AsEnumerable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                string searchText = SearchTextBox.Text.ToLower();
                filteredShipments = filteredShipments.Where(s =>
                    (s.Party?.PartyDescription != null && s.Party.PartyDescription.ToLower().Contains(searchText)) ||
                    (s.Storage?.Address != null && s.Storage.Address.ToLower().Contains(searchText)));
            }

            // Apply date filters
            if (StartDateFilter.SelectedDate.HasValue)
            {
                filteredShipments = filteredShipments.Where(s =>
                    s.DateShipment >= StartDateFilter.SelectedDate.Value);
            }

            if (EndDateFilter.SelectedDate.HasValue)
            {
                filteredShipments = filteredShipments.Where(s =>
                    s.DateShipment <= EndDateFilter.SelectedDate.Value);
            }

            ListViewShipments.ItemsSource = filteredShipments.ToList();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void DateFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void SortByAmount_Click(object sender, RoutedEventArgs e)
        {
            var shipments = ListViewShipments.ItemsSource as IEnumerable<Shipments> ?? _allShipments;
            ListViewShipments.ItemsSource = shipments
                .OrderBy(s => s.Amount)
                .ToList();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            ShipmentsDialog shipmentsDialog = new ShipmentsDialog(null);
            NavigationService.Navigate(shipmentsDialog);
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedShipment = ListViewShipments.SelectedItem as Shipments;
            if (selectedShipment != null)
            {
                ShipmentsDialog shipmentsDialog = new ShipmentsDialog(selectedShipment);
                NavigationService.Navigate(shipmentsDialog);
            }
            else
            {
                MessageBox.Show("Please select a shipment to edit.",
                              "Warning",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var shipmentsToDelete = ListViewShipments.SelectedItems.Cast<Shipments>().ToList();

            if (shipmentsToDelete.Count == 0)
            {
                MessageBox.Show("Please select at least one shipment to delete.",
                              "Warning",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                return;
            }

            var confirmation = MessageBox.Show($"Are you sure you want to delete {shipmentsToDelete.Count} shipment(s)?",
                                            "Confirm Deletion",
                                            MessageBoxButton.YesNo,
                                            MessageBoxImage.Question);

            if (confirmation != MessageBoxResult.Yes)
                return;

            try
            {
                using (var context = new DistilleryRassvetBase())
                {
                    var shipmentIds = shipmentsToDelete.Select(s => s.IDShipment).ToList();

                    var existingShipments = context.Shipments
                        .Where(s => shipmentIds.Contains(s.IDShipment))
                        .ToList();

                    if (existingShipments.Count == 0)
                    {
                        MessageBox.Show("Selected shipments not found in database.",
                                      "Error",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Error);
                        return;
                    }

                    context.Shipments.RemoveRange(existingShipments);
                    int deletedCount = context.SaveChanges();

                    if (deletedCount > 0)
                    {
                        MessageBox.Show($"Successfully deleted {deletedCount} shipment(s).",
                                      "Success",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Information);
                        LoadShipments();
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete shipments.",
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
                    MessageBox.Show("Cannot delete shipment as it's linked to other data in the system.",
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
                LoadShipments();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new DescriptionDB());
        }
    }
}
