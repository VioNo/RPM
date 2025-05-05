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
    /// Логика взаимодействия для PaymentsOrderPage.xaml
    /// </summary>
    public partial class PaymentsOrderPage : Page
    {
        private List<PaymentsOrder> _allPayments = new List<PaymentsOrder>();

        public PaymentsOrderPage()
        {
            InitializeComponent();
            LoadPayments();
            this.IsVisibleChanged += Page_IsVisibleChanged;
        }

        private void LoadPayments()
        {
            using (var context = new DistilleryRassvetBase())
            {
                _allPayments = context.PaymentsOrder
                    .Include("Orders")
                    .Include("Orders.Payment")
                    .ToList();
                ListViewPaymentsOrder.ItemsSource = _allPayments;
            }
        }

        private void ApplyFilters()
        {
            var filteredPayments = _allPayments.AsEnumerable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                string searchText = SearchTextBox.Text.ToLower();
                filteredPayments = filteredPayments.Where(p =>
                    p.IDOrders.ToString().Contains(searchText) ||
                    (p.Orders?.Payment?.PaymentMethod != null &&
                     p.Orders.Payment.PaymentMethod.ToLower().Contains(searchText)));
            }

            // Apply date filters
            if (StartDateFilter.SelectedDate.HasValue)
            {
                filteredPayments = filteredPayments.Where(p =>
                    p.DatePayments >= StartDateFilter.SelectedDate.Value);
            }

            ListViewPaymentsOrder.ItemsSource = filteredPayments.ToList();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void DateFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void SortBySum_Click(object sender, RoutedEventArgs e)
        {
            var payments = ListViewPaymentsOrder.ItemsSource as IEnumerable<PaymentsOrder> ?? _allPayments;
            ListViewPaymentsOrder.ItemsSource = payments
                .OrderBy(p => p.Sum)
                .ToList();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            PaymentsOrderDilog paymentsOrderDialog = new PaymentsOrderDilog(null);
            NavigationService.Navigate(paymentsOrderDialog);
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedPayment = ListViewPaymentsOrder.SelectedItem as PaymentsOrder;
            if (selectedPayment != null)
            {
                PaymentsOrderDilog paymentsOrderDialog = new PaymentsOrderDilog(selectedPayment);
                NavigationService.Navigate(paymentsOrderDialog);
            }
            else
            {
                MessageBox.Show("Please select a payment to edit.",
                              "Warning",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var paymentsToDelete = ListViewPaymentsOrder.SelectedItems.Cast<PaymentsOrder>().ToList();

            if (paymentsToDelete.Count == 0)
            {
                MessageBox.Show("Please select at least one payment to delete.",
                              "Warning",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                return;
            }

            var confirmation = MessageBox.Show($"Are you sure you want to delete {paymentsToDelete.Count} payment(s)?",
                                            "Confirm Deletion",
                                            MessageBoxButton.YesNo,
                                            MessageBoxImage.Question);

            if (confirmation != MessageBoxResult.Yes)
                return;

            try
            {
                using (var context = new DistilleryRassvetBase())
                {
                    var paymentIds = paymentsToDelete.Select(p => p.IDOrders).ToList();

                    var existingPayments = context.PaymentsOrder
                        .Where(p => paymentIds.Contains(p.IDOrders))
                        .ToList();

                    if (existingPayments.Count == 0)
                    {
                        MessageBox.Show("Selected payments not found in database.",
                                      "Error",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Error);
                        return;
                    }

                    context.PaymentsOrder.RemoveRange(existingPayments);
                    int deletedCount = context.SaveChanges();

                    if (deletedCount > 0)
                    {
                        MessageBox.Show($"Successfully deleted {deletedCount} payment(s).",
                                      "Success",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Information);
                        LoadPayments();
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete payments.",
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
                    MessageBox.Show("Cannot delete payment as it's linked to other data in the system.",
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
                LoadPayments();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new DescriptionDB());
        }
    }
}
