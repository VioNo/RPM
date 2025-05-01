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
    /// Логика взаимодействия для ClimateDialog.xaml
    /// </summary>
    public partial class ClimateDialog : Page
    {
        public Climate Climate;

        public ClimateDialog(Climate climate)
        {
            InitializeComponent();

            if (climate != null)
            {
                Climate = climate;
                DataContext = climate;
                DescriptionTextBox.Text = Climate.ClimateDescription;
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
                        if (Climate != null && Climate.IDClimate > 0)
                        {
                            var existingClimate = db.Climate.Find(Climate.IDClimate);

                            if (existingClimate != null)
                            {
                                existingClimate.ClimateDescription = DescriptionTextBox.Text.Trim();

                                var validationErrors = db.GetValidationErrors();
                                if (validationErrors.Any())
                                {
                                    var errorMessages = validationErrors
                                        .SelectMany(validationError => validationError.ValidationErrors)
                                        .Select(validationErrorItem => $"{validationErrorItem.PropertyName}: {validationErrorItem.ErrorMessage}");

                                    MessageBox.Show($"Validation errors:\n{string.Join("\n", errorMessages)}");
                                    return;
                                }

                                db.SaveChanges();
                                MessageBox.Show("Climate data successfully updated");
                            }
                            else
                            {
                                MessageBox.Show("Climate not found in database");
                                return;
                            }
                        }
                        else
                        {
                            var climate = new Climate
                            {
                                ClimateDescription = DescriptionTextBox.Text.Trim()
                            };

                            db.Climate.Add(climate);
                            db.SaveChanges();
                            MessageBox.Show("New climate successfully saved");
                        }

                        NavigationService.Navigate(new ClimatePage());
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
                MessageBox.Show("Please enter climate description.");
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
