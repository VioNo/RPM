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
    /// Логика взаимодействия для WaterDialog.xaml
    /// </summary>
    public partial class WaterDialog : Page
    {
        public Water Water;

        public WaterDialog(Water water)
        {
            InitializeComponent();
            Water = water;

            if (water != null)
            {
                DescriptionTextBox.Text = water.WaterDescription;
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
                        if (Water != null && Water.IDWater > 0)
                        {
                            var existingWater = db.Water.Find(Water.IDWater);

                            if (existingWater != null)
                            {
                                existingWater.WaterDescription = DescriptionTextBox.Text.Trim();
                                db.SaveChanges();
                                MessageBox.Show("Water data updated successfully");
                            }
                            else
                            {
                                MessageBox.Show("Water type not found in database");
                                return;
                            }
                        }
                        else
                        {
                            var water = new Water
                            {
                                WaterDescription = DescriptionTextBox.Text.Trim()
                            };

                            db.Water.Add(water);
                            db.SaveChanges();
                            MessageBox.Show("New water type saved successfully");
                        }

                        NavigationService.Navigate(new WaterPage());
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
