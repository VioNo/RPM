using System;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace RPM
{
    public partial class GrapeVarietiesDialog : Page
    {
        public GrapeVarieties GrapeVariety;

        public GrapeVarietiesDialog(GrapeVarieties grapeVariety)
        {
            InitializeComponent();

            if (grapeVariety != null)
            {
                GrapeVariety = grapeVariety;
                NameTextBox.Text = GrapeVariety.NameGrapeVarieties;
                DescriptionTextBox.Text = GrapeVariety.Description;
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
                        if (GrapeVariety != null && GrapeVariety.IDGrapeVarieties > 0)
                        {
                            var existingVariety = db.GrapeVarieties.Find(GrapeVariety.IDGrapeVarieties);
                            if (existingVariety != null)
                            {
                                UpdateVariety(existingVariety);
                                db.SaveChanges();
                                MessageBox.Show("Данные сорта успешно обновлены");
                            }
                        }
                        else
                        {
                            var newVariety = new GrapeVarieties();
                            UpdateVariety(newVariety);
                            db.GrapeVarieties.Add(newVariety);
                            db.SaveChanges();
                            MessageBox.Show("Новый сорт успешно сохранен");
                        }

                        NavigationService.Navigate(new GrapeVarietiesPage());
                    }
                    catch (DbEntityValidationException ex)
                    {
                        var errorMessages = ex.EntityValidationErrors
                            .SelectMany(x => x.ValidationErrors)
                            .Select(x => $"{x.PropertyName}: {x.ErrorMessage}");

                        MessageBox.Show($"Ошибки валидации:\n{string.Join("\n", errorMessages)}");
                    }
                    catch (DbUpdateException dbEx)
                    {
                        MessageBox.Show($"Ошибка базы данных: {(dbEx.InnerException?.Message ?? dbEx.Message)}");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Неожиданная ошибка: {ex.Message}");
                    }
                }
            }
        }

        private void UpdateVariety(GrapeVarieties variety)
        {
            variety.NameGrapeVarieties = NameTextBox.Text.Trim();
            variety.Description = DescriptionTextBox.Text.Trim();
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Введите название сорта");
                return false;
            }

            return true;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new GrapeVarietiesPage());
        }
    }
}