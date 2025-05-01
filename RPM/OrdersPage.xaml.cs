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
    /// Логика взаимодействия для OrdersPage.xaml
    /// </summary>
    public partial class OrdersPage : Page
    {
        private List<Orders> _allOrders = new List<Orders>();

        public OrdersPage()
        {
            InitializeComponent();
            LoadOrders();
            this.IsVisibleChanged += Page_IsVisibleChanged;
        }

        private void LoadOrders()
        {
            using (var context = new DistilleryRassvetBase())
            {
                _allOrders = context.Orders
                    .Include("Clients")
                    .Include("Products")
                    .Include("Employees")
                    .Include("Payment")
                    .ToList();
                ListViewOrders.ItemsSource = _allOrders;
            }
        }

        private void ApplyFilters()
        {
            var filteredOrders = _allOrders.AsEnumerable();

            // Apply date filters
            if (DateFromFilter.SelectedDate.HasValue)
            {
                filteredOrders = filteredOrders.Where(o => o.DateOrder >= DateFromFilter.SelectedDate.Value);
            }

            if (DateToFilter.SelectedDate.HasValue)
            {
                filteredOrders = filteredOrders.Where(o => o.DateOrder <= DateToFilter.SelectedDate.Value);
            }

            // Apply client name filter
            if (!string.IsNullOrWhiteSpace(ClientSearchTextBox.Text))
            {
                string searchText = ClientSearchTextBox.Text.ToLower();
                filteredOrders = filteredOrders.Where(o =>
                    (o.Clients.Name != null && o.Clients.Name.ToLower().Contains(searchText)) ||
                    (o.Clients.Surname != null && o.Clients.Surname.ToLower().Contains(searchText)));
            }

            ListViewOrders.ItemsSource = filteredOrders.ToList();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void DateFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void SortByDate_Click(object sender, RoutedEventArgs e)
        {
            var orders = ListViewOrders.ItemsSource as IEnumerable<Orders> ?? _allOrders;
            ListViewOrders.ItemsSource = orders
                .OrderBy(o => o.DateOrder)
                .ToList();
        }

        private void SortBySum_Click(object sender, RoutedEventArgs e)
        {
            var orders = ListViewOrders.ItemsSource as IEnumerable<Orders> ?? _allOrders;
            ListViewOrders.ItemsSource = orders
                .OrderByDescending(o => o.Sum)
                .ToList();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            OrdersDialog dialog = new OrdersDialog(null);
            NavigationService.Navigate(dialog);
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedOrder = ListViewOrders.SelectedItem as Orders;
            if (selectedOrder != null)
            {
                OrdersDialog dialog = new OrdersDialog(selectedOrder);
                NavigationService.Navigate(dialog);
            }
            else
            {
                MessageBox.Show("Please select an order to edit.",
                              "Warning",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var ordersToDelete = ListViewOrders.SelectedItems.Cast<Orders>().ToList();

            if (ordersToDelete.Count == 0)
            {
                MessageBox.Show("Please select at least one order to delete.",
                              "Warning",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                return;
            }

            var confirmation = MessageBox.Show($"Are you sure you want to delete {ordersToDelete.Count} order(s)?",
                                            "Confirm Deletion",
                                            MessageBoxButton.YesNo,
                                            MessageBoxImage.Question);

            if (confirmation != MessageBoxResult.Yes)
                return;

            try
            {
                using (var context = new DistilleryRassvetBase())
                {
                    var orderIds = ordersToDelete.Select(o => o.IDOrders).ToList();

                    var existingOrders = context.Orders
                        .Where(o => orderIds.Contains(o.IDOrders))
                        .ToList();

                    if (existingOrders.Count == 0)
                    {
                        MessageBox.Show("Selected orders not found in database.",
                                      "Error",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Error);
                        return;
                    }

                    context.Orders.RemoveRange(existingOrders);
                    int deletedCount = context.SaveChanges();

                    if (deletedCount > 0)
                    {
                        MessageBox.Show($"Successfully deleted {deletedCount} order(s).",
                                      "Success",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Information);
                        LoadOrders();
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete orders.",
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
                    MessageBox.Show("Cannot delete orders that are linked to payments.",
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
                LoadOrders();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new DescriptionDB());
        }
    }
}
