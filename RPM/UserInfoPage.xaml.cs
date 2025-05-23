using System;
using System.Collections.Generic;
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
    /// Логика взаимодействия для UserInfoPage.xaml
    /// </summary>
    public partial class UserInfoPage : Page
    {
        private Clients _client;

        public UserInfoPage(Clients client)
        {
            InitializeComponent();

            if (client != null)
            {
                _client = client;
                DataContext = _client;

                IdTextBlock.Text = _client.IDClient.ToString();
                LoginTextBlock.Text = _client.Login;
                PhoneTextBlock.Text = _client.Phone;
                EmailTextBlock.Text = _client.Email;

                LoadClientOrders();
            }
        }

        private void LoadClientOrders()
        {
            try
            {
                using (var db = new DistilleryRassvetBase())
                {
                    // Загружаем заказы клиента с связанными данными
                    var orders = db.Orders
                        .Include("Products")
                        .Include("Payment")
                        .Where(o => o.IDClient == _client.IDClient)
                        .OrderByDescending(o => o.DateOrder)
                        .Select(o => new
                        {
                            o.IDOrders,
                            o.DateOrder,
                            ProductName = o.Products.Name,
                            o.Count,
                            o.Sum,
                            PaymentStatus = o.Payment != null ? "Оплачено: " + o.Payment.PaymentMethod   : "Не оплачено"
                        })
                        .ToList();

                    OrdersDataGrid.ItemsSource = orders;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке заказов: {ex.Message}");
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new MainPage());
        }
    }
}


