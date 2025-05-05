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
    /// Логика взаимодействия для ShipmentsDialog.xaml
    /// </summary>
    public partial class ShipmentsDialog : Page
    {
        public Shipments Shipment;
        private List<Party> _parties;
        private List<Storage> _storages;

        public ShipmentsDialog(Shipments shipment)
        {
            InitializeComponent();
            LoadComboBoxData();

            if (shipment != null)
            {
                Shipment = shipment;
                DatePicker.SelectedDate = shipment.DateShipment;
                AmountTextBox.Text = shipment.Amount.ToString();
                PartyComboBox.SelectedValue = shipment.IDParty;
                StorageComboBox.SelectedValue = shipment.IDStorage;
            }
            else
            {
                DatePicker.SelectedDate = DateTime.Now;
            }
        }

        private void LoadComboBoxData()
        {
            using (var db = new DistilleryRassvetBase())
            {
                _parties = db.Party
                    .OrderBy(p => p.PartyDescription)
                    .ToList();

                _storages = db.Storage
                    .OrderBy(s => s.Address)
                    .ToList();

                PartyComboBox.ItemsSource = _parties;
                PartyComboBox.DisplayMemberPath = "PartyDescription";
                PartyComboBox.SelectedValuePath = "IDParty";

                StorageComboBox.ItemsSource = _storages;
                StorageComboBox.DisplayMemberPath = "Address";
                StorageComboBox.SelectedValuePath = "IDStorage";
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
                        if (Shipment != null && Shipment.IDShipment > 0)
                        {
                            var existingShipment = db.Shipments.Find(Shipment.IDShipment);

                            if (existingShipment != null)
                            {
                                existingShipment.DateShipment = DatePicker.SelectedDate.Value;
                                existingShipment.Amount = int.Parse(AmountTextBox.Text);
                                existingShipment.IDParty = (int)PartyComboBox.SelectedValue;
                                existingShipment.IDStorage = (int)StorageComboBox.SelectedValue;

                                db.SaveChanges();
                                MessageBox.Show("Shipment data updated successfully");
                            }
                            else
                            {
                                MessageBox.Show("Shipment not found in database");
                                return;
                            }
                        }
                        else
                        {
                            var shipment = new Shipments
                            {
                                DateShipment = DatePicker.SelectedDate.Value,
                                Amount = int.Parse(AmountTextBox.Text),
                                IDParty = (int)PartyComboBox.SelectedValue,
                                IDStorage = (int)StorageComboBox.SelectedValue
                            };

                            db.Shipments.Add(shipment);
                            db.SaveChanges();
                            MessageBox.Show("New shipment saved successfully");
                        }

                        NavigationService.Navigate(new ShipmentsPage());
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
            if (!DatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Please select a date.");
                return false;
            }

            if (!int.TryParse(AmountTextBox.Text, out int amount) || amount <= 0)
            {
                MessageBox.Show("Please enter a valid positive amount.");
                return false;
            }

            if (PartyComboBox.SelectedValue == null)
            {
                MessageBox.Show("Please select a party.");
                return false;
            }

            if (StorageComboBox.SelectedValue == null)
            {
                MessageBox.Show("Please select a storage.");
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
