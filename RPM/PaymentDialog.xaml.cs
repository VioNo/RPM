using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
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
    /// Логика взаимодействия для PaymentDialog.xaml
    /// </summary>
    public partial class PaymentDialog : Page
    {
        public Payment Payment;

        public PaymentDialog(Payment payment)
        {
            InitializeComponent();

            if (payment != null)
            {
                Payment = payment;
                MethodTextBox.Text = payment.PaymentMethod;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                using (var db = new DistilleryRassvetBase())
                {
                    try
                    {
                        if (Payment != null && Payment.IDPayment > 0)
                        {
                            var existingPayment = db.Payment.Find(Payment.IDPayment);

                            if (existingPayment != null)
                            {
                                existingPayment.PaymentMethod = MethodTextBox.Text.Trim();
                                db.SaveChanges();
                                MessageBox.Show("Payment method updated successfully");
                            }
                            else
                            {
                                MessageBox.Show("Payment method not found in database");
                                return;
                            }
                        }
                        else
                        {
                            var payment = new Payment
                            {
                                PaymentMethod = MethodTextBox.Text.Trim()
                            };

                            db.Payment.Add(payment);
                            db.SaveChanges();
                            MessageBox.Show("New payment method saved successfully");
                        }

                        NavigationService.Navigate(new PaymentPage());
                    }
                    catch (DbEntityValidationException ex)
                    {
                        var errorMessages = ex.EntityValidationErrors
                            .SelectMany(x => x.ValidationErrors)
                            .Select(x => $"{x.PropertyName}: {x.ErrorMessage}");

                        MessageBox.Show($"Validation errors:\n{string.Join("\n", errorMessages)}");
                    }
                    catch (DbUpdateException dbEx)
                    {
                        MessageBox.Show($"Database error: {(dbEx.InnerException?.Message ?? dbEx.Message)}");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Unexpected error: {ex.Message}");
                    }
                }
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(MethodTextBox.Text))
            {
                MessageBox.Show("Please enter a payment method.");
                return false;
            }

            if (MethodTextBox.Text.Length > 50)
            {
                MessageBox.Show("Payment method cannot exceed 50 characters.");
                return false;
            }

            return true;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
