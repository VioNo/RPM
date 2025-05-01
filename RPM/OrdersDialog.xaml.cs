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
    /// Логика взаимодействия для OrdersDialog.xaml
    /// </summary>
    public partial class OrdersDialog : Page
    {
        public Orders Order;
        private List<Clients> _clients;
        private List<Products> _products;
        private List<Employees> _employees;
        private List<Payment> _payments;

        public OrdersDialog(Orders order)
        {
            InitializeComponent();
            LoadComboBoxData();

            if (order != null)
            {
                Order = order;

                ClientComboBox.SelectedValue = order.IDClient;
                ProductComboBox.SelectedValue = order.IDProduct;
                EmployeeComboBox.SelectedValue = order.IDEmployee;
                CountTextBox.Text = order.Count.ToString();
                DatePicker.SelectedDate = order.DateOrder;
                SumTextBox.Text = order.Sum.ToString();
                PaymentComboBox.SelectedValue = order.IDPayment;
            }
            else
            {
                DatePicker.SelectedDate = DateTime.Today;
            }
        }

        private void LoadComboBoxData()
        {
            using (var db = new DistilleryRassvetBase())
            {
                _clients = db.Clients.ToList();
                _products = db.Products.ToList();

                // Для сотрудников используем FullName вместо Name
                _employees = db.Employees
                    .Include("JobTitles") // Если нужно отображать должность
                    .ToList();

                _payments = db.Payment.ToList();

                ClientComboBox.ItemsSource = _clients;

                ProductComboBox.ItemsSource = _products;
                ProductComboBox.DisplayMemberPath = "Name"; 
                ProductComboBox.SelectedValuePath = "IDProduct";

                EmployeeComboBox.ItemsSource = _employees;
                EmployeeComboBox.DisplayMemberPath = "FullName"; 
                EmployeeComboBox.SelectedValuePath = "IDEmployee";

                PaymentComboBox.ItemsSource = _payments;
                PaymentComboBox.DisplayMemberPath = "PaymentMethod"; 
                PaymentComboBox.SelectedValuePath = "IDPayment";
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
                        if (Order != null && Order.IDOrders > 0)
                        {
                            var existingOrder = db.Orders.Find(Order.IDOrders);

                            if (existingOrder != null)
                            {
                                existingOrder.IDClient = (int)ClientComboBox.SelectedValue;
                                existingOrder.IDProduct = (int)ProductComboBox.SelectedValue;
                                existingOrder.IDEmployee = (int)EmployeeComboBox.SelectedValue;
                                existingOrder.Count = int.Parse(CountTextBox.Text);
                                existingOrder.DateOrder = DatePicker.SelectedDate.Value;
                                existingOrder.Sum = decimal.Parse(SumTextBox.Text);
                                existingOrder.IDPayment = (int)PaymentComboBox.SelectedValue;

                                db.SaveChanges();
                                MessageBox.Show("Order updated successfully");
                            }
                            else
                            {
                                MessageBox.Show("Order not found in database");
                                return;
                            }
                        }
                        else
                        {
                            var order = new Orders
                            {
                                IDClient = (int)ClientComboBox.SelectedValue,
                                IDProduct = (int)ProductComboBox.SelectedValue,
                                IDEmployee = (int)EmployeeComboBox.SelectedValue,
                                Count = int.Parse(CountTextBox.Text),
                                DateOrder = DatePicker.SelectedDate.Value,
                                Sum = decimal.Parse(SumTextBox.Text),
                                IDPayment = (int)PaymentComboBox.SelectedValue
                            };

                            db.Orders.Add(order);
                            db.SaveChanges();
                            MessageBox.Show("New order saved successfully");
                        }

                        NavigationService.Navigate(new OrdersPage());
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
            if (ClientComboBox.SelectedValue == null)
            {
                MessageBox.Show("Please select a client.");
                return false;
            }

            if (ProductComboBox.SelectedValue == null)
            {
                MessageBox.Show("Please select a product.");
                return false;
            }

            if (EmployeeComboBox.SelectedValue == null)
            {
                MessageBox.Show("Please select an employee.");
                return false;
            }

            if (!int.TryParse(CountTextBox.Text, out int count) || count <= 0)
            {
                MessageBox.Show("Please enter a valid positive count.");
                return false;
            }

            if (!DatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Please select a date.");
                return false;
            }

            if (!decimal.TryParse(SumTextBox.Text, out decimal sum) || sum <= 0)
            {
                MessageBox.Show("Please enter a valid positive sum.");
                return false;
            }

            if (PaymentComboBox.SelectedValue == null)
            {
                MessageBox.Show("Please select a payment method.");
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
