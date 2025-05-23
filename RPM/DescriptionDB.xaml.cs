﻿using System;
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
using System.Diagnostics;

namespace RPM
{
    /// <summary>
    /// Логика взаимодействия для DescriptionDB.xaml
    /// </summary>
    public partial class DescriptionDB : Page
    {
        public DescriptionDB()
        {
            InitializeComponent();
        }

        private void ProductsButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ProductsPage());
        }

        private void EmployeesButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new EmployeesPage());
        }

        private void GrapeVarietiesButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new GrapeVarietiesPage());
        }

        private void StorageButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new StoragePage());
        }

        private void ClientsButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ClientsPage());
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new MainPage());
        }

        private void ClimateButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ClimatePage());
        }

        private void FermentationButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new FermentationPage());
        }

        private void GrowingConditionsButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new GrowingConditionsPage());
        }

        private void JobTitlesButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new JobTitlesPage());
        }

        private void OrdersButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new OrdersPage());
        }

        private void PartyButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new PartyPage());
        }

        private void PaymentButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new PaymentPage());
        }

        private void ShipmentsButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ShipmentsPage());
        }

        private void SoilButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new SoilPage());
        }

        private void StorageWineButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new StorageWinePage());
        }

        private void WaterButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new WaterPage());
        }

        private void YieldButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new YieldPage());
        }

        private void PaymentsOrderButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new PaymentsOrderPage());
        }

        private void RequestsButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new RequestsPage());
        }

        private void GraphsButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new GraphsPage());
        }

        private void spr_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("справочки.chm");
        }
    }
}
