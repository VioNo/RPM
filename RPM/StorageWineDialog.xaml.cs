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
    /// Логика взаимодействия для StorageWineDialog.xaml
    /// </summary>
    public partial class StorageWineDialog : Page
    {
        public StorageWine StorageWine;
        private List<Fermentation> _fermentations;
        private List<Party> _parties;

        public StorageWineDialog(StorageWine storageWine)
        {
            InitializeComponent();
            LoadComboBoxData();

            if (storageWine != null)
            {
                StorageWine = storageWine;
                ExpirationDatePicker.SelectedDate = storageWine.ExpirationDate;
                FermentationComboBox.SelectedValue = storageWine.IDFermentation;
                PartyComboBox.SelectedValue = storageWine.IDParty;
                CountTextBox.Text = storageWine.Count?.ToString();
                MeasureTextBox.Text = storageWine.Measure;
            }
            else
            {
                ExpirationDatePicker.SelectedDate = DateTime.Now.AddYears(1); // Default expiration 1 year from now
            }
        }

        private void LoadComboBoxData()
        {
            using (var db = new DistilleryRassvetBase())
            {
                _fermentations = db.Fermentation
                    .OrderBy(f => f.Description)
                    .ToList();

                _parties = db.Party
                    .OrderBy(p => p.PartyDescription)
                    .ToList();

                FermentationComboBox.ItemsSource = _fermentations;
                FermentationComboBox.DisplayMemberPath = "Description";
                FermentationComboBox.SelectedValuePath = "IDFermentation";

                PartyComboBox.ItemsSource = _parties;
                PartyComboBox.DisplayMemberPath = "PartyDescription";
                PartyComboBox.SelectedValuePath = "IDParty";
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
                        if (StorageWine != null && StorageWine.IDStorageWine > 0)
                        {
                            var existingStorageWine = db.StorageWine.Find(StorageWine.IDStorageWine);

                            if (existingStorageWine != null)
                            {
                                existingStorageWine.ExpirationDate = ExpirationDatePicker.SelectedDate.Value;
                                existingStorageWine.IDFermentation = (int)FermentationComboBox.SelectedValue;
                                existingStorageWine.IDParty = (int)PartyComboBox.SelectedValue;

                                if (int.TryParse(CountTextBox.Text, out int count))
                                {
                                    existingStorageWine.Count = count;
                                }
                                else
                                {
                                    existingStorageWine.Count = null;
                                }

                                existingStorageWine.Measure = MeasureTextBox.Text.Trim();

                                db.SaveChanges();
                                MessageBox.Show("Wine storage data updated successfully");
                            }
                            else
                            {
                                MessageBox.Show("Wine storage record not found in database");
                                return;
                            }
                        }
                        else
                        {
                            var storageWine = new StorageWine
                            {
                                ExpirationDate = ExpirationDatePicker.SelectedDate.Value,
                                IDFermentation = (int)FermentationComboBox.SelectedValue,
                                IDParty = (int)PartyComboBox.SelectedValue,
                                Measure = MeasureTextBox.Text.Trim()
                            };

                            if (int.TryParse(CountTextBox.Text, out int count))
                            {
                                storageWine.Count = count;
                            }

                            db.StorageWine.Add(storageWine);
                            db.SaveChanges();
                            MessageBox.Show("New wine storage record saved successfully");
                        }

                        NavigationService.Navigate(new StorageWinePage());
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
            if (!ExpirationDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Please select an expiration date.");
                return false;
            }

            if (FermentationComboBox.SelectedValue == null)
            {
                MessageBox.Show("Please select a fermentation process.");
                return false;
            }

            if (PartyComboBox.SelectedValue == null)
            {
                MessageBox.Show("Please select a party.");
                return false;
            }

            if (!string.IsNullOrEmpty(CountTextBox.Text) && !int.TryParse(CountTextBox.Text, out int count))
            {
                MessageBox.Show("Please enter a valid count (whole number) or leave empty.");
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
