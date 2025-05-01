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
    /// Логика взаимодействия для PartyPage.xaml
    /// </summary>
    public partial class PartyPage : Page
    {
        private List<Party> _allParties = new List<Party>();

        public PartyPage()
        {
            InitializeComponent();
            LoadParties();
            this.IsVisibleChanged += Page_IsVisibleChanged;
        }

        private void LoadParties()
        {
            using (var context = new DistilleryRassvetBase())
            {
                _allParties = context.Party
                    .Include("Products")
                    .Include("Shipments")
                    .ToList();
                ListViewParty.ItemsSource = _allParties;
            }
        }

        private void ApplyFilters()
        {
            var filteredParties = _allParties.AsEnumerable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                string searchText = SearchTextBox.Text.ToLower();
                filteredParties = filteredParties.Where(p =>
                    p.PartyDescription != null && p.PartyDescription.ToLower().Contains(searchText));
            }

            ListViewParty.ItemsSource = filteredParties.ToList();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void SortByCount_Click(object sender, RoutedEventArgs e)
        {
            var parties = ListViewParty.ItemsSource as IEnumerable<Party> ?? _allParties;
            ListViewParty.ItemsSource = parties
                .OrderBy(p => p.Count)
                .ToList();
        }

        private void SortByDescription_Click(object sender, RoutedEventArgs e)
        {
            var parties = ListViewParty.ItemsSource as IEnumerable<Party> ?? _allParties;
            ListViewParty.ItemsSource = parties
                .OrderBy(p => p.PartyDescription)
                .ToList();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            PartyDialog dialog = new PartyDialog(null);
            NavigationService.Navigate(dialog);
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedParty = ListViewParty.SelectedItem as Party;
            if (selectedParty != null)
            {
                PartyDialog dialog = new PartyDialog(selectedParty);
                NavigationService.Navigate(dialog);
            }
            else
            {
                MessageBox.Show("Please select a party to edit.",
                              "Warning",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var partiesToDelete = ListViewParty.SelectedItems.Cast<Party>().ToList();

            if (partiesToDelete.Count == 0)
            {
                MessageBox.Show("Please select at least one party to delete.",
                              "Warning",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                return;
            }

            var confirmation = MessageBox.Show($"Are you sure you want to delete {partiesToDelete.Count} party(ies)?\n" +
                                            "This will affect all related products and shipments.",
                                            "Confirm Deletion",
                                            MessageBoxButton.YesNo,
                                            MessageBoxImage.Question);

            if (confirmation != MessageBoxResult.Yes)
                return;

            try
            {
                using (var context = new DistilleryRassvetBase())
                {
                    var partyIds = partiesToDelete.Select(p => p.IDParty).ToList();

                    // Check if any products or shipments are using these parties
                    var hasProducts = context.Products.Any(p => partyIds.Contains(p.IDParty));
                    var hasShipments = context.Shipments.Any(s => partyIds.Contains(s.IDParty));

                    if (hasProducts || hasShipments)
                    {
                        MessageBox.Show("Cannot delete parties that are linked to products or shipments.",
                                      "Error",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Error);
                        return;
                    }

                    var existingParties = context.Party
                        .Where(p => partyIds.Contains(p.IDParty))
                        .ToList();

                    if (existingParties.Count == 0)
                    {
                        MessageBox.Show("Selected parties not found in database.",
                                      "Error",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Error);
                        return;
                    }

                    context.Party.RemoveRange(existingParties);
                    int deletedCount = context.SaveChanges();

                    if (deletedCount > 0)
                    {
                        MessageBox.Show($"Successfully deleted {deletedCount} party(ies).",
                                      "Success",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Information);
                        LoadParties();
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete parties.",
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
                    MessageBox.Show("Cannot delete parties that are linked to products or shipments.",
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
                LoadParties();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new DescriptionDB());
        }
    }
}
