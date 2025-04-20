using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace RPM
{
    public partial class ClientsPage : Page
    {
        private List<Clients> _allClients = new List<Clients>();

        public ClientsPage()
        {
            InitializeComponent();
            LoadClients();
            this.IsVisibleChanged += Page_IsVisibleChanged;
        }

        private void LoadClients()
        {
            using (var context = new DistilleryRassvetBase())
            {
                _allClients = context.Clients.ToList();
                ListViewClients.ItemsSource = _allClients;
            }
        }

        private void ApplyFilters()
        {
            var filteredClients = _allClients.AsEnumerable();

            // Apply search filter (Name or Surname)
            if (!string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                string searchText = SearchTextBox.Text.ToLower();
                filteredClients = filteredClients.Where(c =>
                    (c.Name != null && c.Name.ToLower().Contains(searchText)) ||
                    (c.Surname != null && c.Surname.ToLower().Contains(searchText)));
            }

            // Apply email filter
            if (!string.IsNullOrWhiteSpace(EmailFilterTextBox.Text))
            {
                string emailFilter = EmailFilterTextBox.Text.ToLower();
                filteredClients = filteredClients.Where(c =>
                    c.Email != null && c.Email.ToLower().Contains(emailFilter));
            }

            ListViewClients.ItemsSource = filteredClients.ToList();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void EmailFilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void SortByPhone_Click(object sender, RoutedEventArgs e)
        {
            var clients = ListViewClients.ItemsSource as IEnumerable<Clients> ?? _allClients;

            ListViewClients.ItemsSource = clients
                .OrderBy(c => GetFirstDigitInPhone(c.Phone))
                .ToList();
        }

        private int GetFirstDigitInPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return int.MaxValue; // Put empty phones at the end

            var match = Regex.Match(phone, @"\d");
            return match.Success ? int.Parse(match.Value) : int.MaxValue;
        }

        // ... (keep all other existing methods unchanged)
        private void AddButton_Click(object sender, RoutedEventArgs e) { /* unchanged */ }
        private void UpdateButton_Click(object sender, RoutedEventArgs e) { /* unchanged */ }
        private void DeleteButton_Click(object sender, RoutedEventArgs e) { /* unchanged */ }
        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) { /* unchanged */ }
        private void BackButton_Click(object sender, RoutedEventArgs e) { /* unchanged */ }
    }
}