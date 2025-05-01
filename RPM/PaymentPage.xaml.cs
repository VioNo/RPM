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
    /// Логика взаимодействия для PaymentPage.xaml
    /// </summary>
    public partial class PaymentPage : Page
    {
        private List<Payment> _allPayments = new List<Payment>();

        public PaymentPage()
        {
            InitializeComponent();
            LoadPayments();
            this.IsVisibleChanged += Page_IsVisibleChanged;
        }

        private void LoadPayments()
        {
            using (var context = new DistilleryRassvetBase())
            {
                _allPayments = context.Payment
                    .Include("Orders")
                    .ToList();
                ListViewPayments.ItemsSource = _allPayments;
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
                    p.PaymentMethod != null && p.PaymentMethod.ToLower().Contains(searchText));
            }

            ListViewPayments.ItemsSource = filteredPayments.ToList();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void SortAscending_Click(object sender, RoutedEventArgs e)
        {
            var payments = ListViewPayments.ItemsSource as IEnumerable<Payment> ?? _allPayments;
            ListViewPayments.ItemsSource = payments
                .OrderBy(p => p.PaymentMethod)
                .ToList();
        }

        private void SortDescending_Click(object sender, RoutedEventArgs e)
        {
            var payments = ListViewPayments.ItemsSource as IEnumerable<Payment> ?? _allPayments;
            ListViewPayments.ItemsSource = payments
                .OrderByDescending(p => p.PaymentMethod)
                .ToList();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            PaymentDialog dialog = new PaymentDialog(null);
            NavigationService.Navigate(dialog);
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedPayment = ListViewPayments.SelectedItem as Payment;
            if (selectedPayment != null)
            {
                PaymentDialog dialog = new PaymentDialog(selectedPayment);
                NavigationService.Navigate(dialog);
            }
            else
            {
                MessageBox.Show("Please select a payment method to edit.",
                              "Warning",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var paymentsToDelete = ListViewPayments.SelectedItems.Cast<Payment>().ToList();

            if (paymentsToDelete.Count == 0)
            {
                MessageBox.Show("Please select at least one payment method to delete.",
                              "Warning",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                return;
            }

            var confirmation = MessageBox.Show($"Are you sure you want to delete {paymentsToDelete.Count} payment method(s)?\n" +
                                            "This will affect all orders using these payment methods.",
                                            "Confirm Deletion",
                                            MessageBoxButton.YesNo,
                                            MessageBoxImage.Question);

            if (confirmation != MessageBoxResult.Yes)
                return;

            try
            {
                using (var context = new DistilleryRassvetBase())
                {
                    var paymentIds = paymentsToDelete.Select(p => p.IDPayment).ToList();

                    // Check if any orders are using these payment methods
                    var hasOrders = context.Orders.Any(o => paymentIds.Contains(o.IDPayment));

                    if (hasOrders)
                    {
                        MessageBox.Show("Cannot delete payment methods that are assigned to orders.",
                                      "Error",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Error);
                        return;
                    }

                    var existingPayments = context.Payment
                        .Where(p => paymentIds.Contains(p.IDPayment))
                        .ToList();

                    if (existingPayments.Count == 0)
                    {
                        MessageBox.Show("Selected payment methods not found in database.",
                                      "Error",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Error);
                        return;
                    }

                    context.Payment.RemoveRange(existingPayments);
                    int deletedCount = context.SaveChanges();

                    if (deletedCount > 0)
                    {
                        MessageBox.Show($"Successfully deleted {deletedCount} payment method(s).",
                                      "Success",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Information);
                        LoadPayments();
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete payment methods.",
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
                    MessageBox.Show("Cannot delete payment methods that are assigned to orders.",
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
