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
    /// Логика взаимодействия для YieldDialog.xaml
    /// </summary>
    public partial class YieldDialog : Page
    {
        public Yield Yield;
        private List<GrapeVarieties> _grapeVarieties;

        public YieldDialog(Yield yield)
        {
            InitializeComponent();
            LoadComboBoxData();

            if (yield != null)
            {
                Yield = yield;
                DatePicker.SelectedDate = yield.DateYield;
                GrapeVarietyComboBox.SelectedValue = yield.IDGrapeVarieties;
                HarvestTextBox.Text = yield.Harvest.ToString();
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
                _grapeVarieties = db.GrapeVarieties
                    .OrderBy(g => g.NameGrapeVarieties)
                    .ToList();

                GrapeVarietyComboBox.ItemsSource = _grapeVarieties;
                GrapeVarietyComboBox.DisplayMemberPath = "NameGrapeVarieties";
                GrapeVarietyComboBox.SelectedValuePath = "IDGrapeVarieties";
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
                        if (Yield != null && Yield.IDYield > 0)
                        {
                            var existingYield = db.Yield.Find(Yield.IDYield);

                            if (existingYield != null)
                            {
                                existingYield.DateYield = DatePicker.SelectedDate.Value;
                                existingYield.IDGrapeVarieties = (int)GrapeVarietyComboBox.SelectedValue;
                                existingYield.Harvest = decimal.Parse(HarvestTextBox.Text);

                                db.SaveChanges();
                                MessageBox.Show("Yield data updated successfully");
                            }
                            else
                            {
                                MessageBox.Show("Yield not found in database");
                                return;
                            }
                        }
                        else
                        {
                            var yield = new Yield
                            {
                                DateYield = DatePicker.SelectedDate.Value,
                                IDGrapeVarieties = (int)GrapeVarietyComboBox.SelectedValue,
                                Harvest = decimal.Parse(HarvestTextBox.Text)
                            };

                            db.Yield.Add(yield);
                            db.SaveChanges();
                            MessageBox.Show("New yield saved successfully");
                        }

                        NavigationService.Navigate(new YieldPage());
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

            if (GrapeVarietyComboBox.SelectedValue == null)
            {
                MessageBox.Show("Please select a grape variety.");
                return false;
            }

            if (!decimal.TryParse(HarvestTextBox.Text, out decimal harvest) || harvest <= 0)
            {
                MessageBox.Show("Please enter a valid positive harvest amount.");
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
