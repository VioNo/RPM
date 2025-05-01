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
    /// Логика взаимодействия для PartyDialog.xaml
    /// </summary>
    public partial class PartyDialog : Page
    {
        public Party Party;

        public PartyDialog(Party party)
        {
            InitializeComponent();

            if (party != null)
            {
                Party = party;
                DescriptionTextBox.Text = party.PartyDescription;
                CountTextBox.Text = party.Count?.ToString();
                MeasureTextBox.Text = party.Measure;
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
                        if (Party != null && Party.IDParty > 0)
                        {
                            var existingParty = db.Party.Find(Party.IDParty);

                            if (existingParty != null)
                            {
                                existingParty.PartyDescription = DescriptionTextBox.Text.Trim();
                                existingParty.Count = int.TryParse(CountTextBox.Text, out int count) ? count : (int?)null;
                                existingParty.Measure = MeasureTextBox.Text.Trim();

                                db.SaveChanges();
                                MessageBox.Show("Party updated successfully");
                            }
                            else
                            {
                                MessageBox.Show("Party not found in database");
                                return;
                            }
                        }
                        else
                        {
                            var party = new Party
                            {
                                PartyDescription = DescriptionTextBox.Text.Trim(),
                                Count = int.TryParse(CountTextBox.Text, out int count) ? count : (int?)null,
                                Measure = MeasureTextBox.Text.Trim()
                            };

                            db.Party.Add(party);
                            db.SaveChanges();
                            MessageBox.Show("New party saved successfully");
                        }

                        NavigationService.Navigate(new PartyPage());
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
                MessageBox.Show("Please enter a description.");
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
