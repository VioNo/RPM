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
                }
            IdTextBlock.Text = _client.IDClient.ToString();
            LoginTextBlock.Text = _client.Login;
            PhoneTextBlock.Text = _client.Phone;
            EmailTextBlock.Text = _client.Email;
            }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new MainPage());
        }
    }
    }


