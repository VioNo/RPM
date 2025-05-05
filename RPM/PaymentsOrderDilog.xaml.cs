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
    /// Логика взаимодействия для PaymentsOrderDilog.xaml
    /// </summary>
    public partial class PaymentsOrderDilog : Page
    {
        public PaymentsOrder PaymentOrder;

        public PaymentsOrderDilog(PaymentsOrder paymentOrder)
        {
            InitializeComponent();
            PaymentOrder = paymentOrder;

            if (paymentOrder != null)
            {
                // Режим редактирования
                OrderIdTextBox.Text = paymentOrder.IDOrders.ToString();
                SumTextBox.Text = paymentOrder.Sum.ToString("N2");
                PaymentDatePicker.SelectedDate = paymentOrder.DatePayments;
            }
            else
            {
                // Режим добавления
                PaymentDatePicker.SelectedDate = DateTime.Now;
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
                        int orderId = int.Parse(OrderIdTextBox.Text);

                        if (PaymentOrder != null && PaymentOrder.IDOrders > 0)
                        {
                            // Режим редактирования
                            var existingPayment = db.PaymentsOrder.Find(PaymentOrder.IDOrders);
                            if (existingPayment != null)
                            {
                                existingPayment.Sum = decimal.Parse(SumTextBox.Text);
                                existingPayment.DatePayments = PaymentDatePicker.SelectedDate.Value;
                                db.SaveChanges();
                                MessageBox.Show("Данные платежа успешно обновлены", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                        else
                        {
                            // Проверяем существование заказа
                            var orderExists = db.Orders.Any(o => o.IDOrders == orderId);
                            if (!orderExists)
                            {
                                MessageBox.Show("Заказ с указанным ID не найден", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            // Проверяем, нет ли уже платежа для этого заказа
                            var paymentExists = db.PaymentsOrder.Any(p => p.IDOrders == orderId);
                            if (paymentExists)
                            {
                                MessageBox.Show("Для этого заказа уже существует платеж", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            // Режим создания
                            var paymentOrder = new PaymentsOrder
                            {
                                IDOrders = orderId,
                                Sum = decimal.Parse(SumTextBox.Text),
                                DatePayments = PaymentDatePicker.SelectedDate.Value
                            };

                            db.PaymentsOrder.Add(paymentOrder);
                            db.SaveChanges();
                            MessageBox.Show("Новый платеж успешно сохранен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        }

                        NavigationService.Navigate(new PaymentsOrderPage());
                    }
                    catch (DbEntityValidationException ex)
                    {
                        var errorMessages = ex.EntityValidationErrors
                            .SelectMany(x => x.ValidationErrors)
                            .Select(x => $"{x.PropertyName}: {x.ErrorMessage}");

                        MessageBox.Show($"Ошибки валидации:\n{string.Join("\n", errorMessages)}",
                                      "Ошибка",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Error);
                    }
                    catch (DbUpdateException dbEx)
                    {
                        MessageBox.Show($"Ошибка базы данных: {(dbEx.InnerException?.Message ?? dbEx.Message)}",
                                      "Ошибка",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Error);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Неожиданная ошибка: {ex.Message}",
                                      "Ошибка",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Error);
                    }
                }
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(OrderIdTextBox.Text) || !int.TryParse(OrderIdTextBox.Text, out int orderId) || orderId <= 0)
            {
                MessageBox.Show("Пожалуйста, введите корректный ID заказа",
                              "Предупреждение",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                return false;
            }

            if (!decimal.TryParse(SumTextBox.Text, out decimal sum) || sum <= 0)
            {
                MessageBox.Show("Пожалуйста, введите корректную положительную сумму",
                              "Предупреждение",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                return false;
            }

            if (!PaymentDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Пожалуйста, выберите дату платежа",
                              "Предупреждение",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
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
