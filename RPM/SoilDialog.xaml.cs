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
    /// Логика взаимодействия для SoilDialog.xaml
    /// </summary>
    public partial class SoilDialog : Page
    {
        public Soil Soil;

        public SoilDialog(Soil soil)
        {
            InitializeComponent();
            Soil = soil;

            if (soil != null)
            {
                DescriptionTextBox.Text = soil.SoilDescription;
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
                        if (Soil != null && Soil.IDSoil > 0)
                        {
                            var existingSoil = db.Soil.Find(Soil.IDSoil);

                            if (existingSoil != null)
                            {
                                existingSoil.SoilDescription = DescriptionTextBox.Text.Trim();
                                db.SaveChanges();
                                MessageBox.Show("Soil data updated successfully");
                            }
                            else
                            {
                                MessageBox.Show("Soil not found in database");
                                return;
                            }
                        }
                        else
                        {
                            var soil = new Soil
                            {
                                SoilDescription = DescriptionTextBox.Text.Trim()
                            };

                            db.Soil.Add(soil);
                            db.SaveChanges();
                            MessageBox.Show("New soil saved successfully");
                        }

                        NavigationService.Navigate(new SoilPage());
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
            if (string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
            {
                MessageBox.Show("Please enter a description.",
                              "Warning",
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
