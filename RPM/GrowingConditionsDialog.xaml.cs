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
    /// Логика взаимодействия для GrowingConditionsDialog.xaml
    /// </summary>
    public partial class GrowingConditionsDialog : Page
    {
        public GrowingConditions GrowingCondition;
        // листы для вывода описания через соответствующий id 
        private List<GrapeVarieties> _grapeVarieties;
        private List<Soil> _soils;
        private List<Water> _waters;
        private List<Climate> _climates;

        public GrowingConditionsDialog(GrowingConditions growingCondition)
        {
            InitializeComponent();
            LoadComboBoxData();

            if (growingCondition != null)
            {
                GrowingCondition = growingCondition;

                GrapeVarietyComboBox.SelectedValue = growingCondition.IDGrapeVarieties;
                SoilComboBox.SelectedValue = growingCondition.IDSoil;
                WaterComboBox.SelectedValue = growingCondition.IDWater;
                ClimateComboBox.SelectedValue = growingCondition.IDClimate;
                DescriptionTextBox.Text = growingCondition.Description;
            }
        }

        private void LoadComboBoxData()
        {
            using (var db = new DistilleryRassvetBase())
            {
                _grapeVarieties = db.GrapeVarieties.ToList();
                _soils = db.Soil.ToList();
                _waters = db.Water.ToList();
                _climates = db.Climate.ToList();

                // Настройка ComboBox для сортов винограда
                GrapeVarietyComboBox.ItemsSource = _grapeVarieties;
                GrapeVarietyComboBox.DisplayMemberPath = "NameGrapeVarieties";
                GrapeVarietyComboBox.SelectedValuePath = "IDGrapeVarieties";

                // Настройка ComboBox для типов почвы
                SoilComboBox.ItemsSource = _soils;
                SoilComboBox.DisplayMemberPath = "SoilDescription";
                SoilComboBox.SelectedValuePath = "IDSoil";

                // Настройка ComboBox для типов воды
                WaterComboBox.ItemsSource = _waters;
                WaterComboBox.DisplayMemberPath = "WaterDescription";
                WaterComboBox.SelectedValuePath = "IDWater";

                // Настройка ComboBox для типов климата
                ClimateComboBox.ItemsSource = _climates;
                ClimateComboBox.DisplayMemberPath = "ClimateDescription";
                ClimateComboBox.SelectedValuePath = "IDClimate";
            }
        }

        // Остальные методы остаются без изменений
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                using (var db = new DistilleryRassvetBase())
                {
                    try
                    {
                        if (GrowingCondition != null && GrowingCondition.IDGrowingConditions > 0)
                        {
                            var existingCondition = db.GrowingConditions.Find(GrowingCondition.IDGrowingConditions);

                            if (existingCondition != null)
                            {
                                existingCondition.IDGrapeVarieties = (int)GrapeVarietyComboBox.SelectedValue;
                                existingCondition.IDSoil = (int)SoilComboBox.SelectedValue;
                                existingCondition.IDWater = (int)WaterComboBox.SelectedValue;
                                existingCondition.IDClimate = (int)ClimateComboBox.SelectedValue;
                                existingCondition.Description = DescriptionTextBox.Text.Trim();

                                db.SaveChanges();
                                MessageBox.Show("Условия выращивания успешно обновлены");
                            }
                            else
                            {
                                MessageBox.Show("Условия выращивания не найдены в базе данных");
                                return;
                            }
                        }
                        else
                        {
                            var condition = new GrowingConditions
                            {
                                IDGrapeVarieties = (int)GrapeVarietyComboBox.SelectedValue,
                                IDSoil = (int)SoilComboBox.SelectedValue,
                                IDWater = (int)WaterComboBox.SelectedValue,
                                IDClimate = (int)ClimateComboBox.SelectedValue,
                                Description = DescriptionTextBox.Text.Trim()
                            };

                            db.GrowingConditions.Add(condition);
                            db.SaveChanges();
                            MessageBox.Show("Новые условия выращивания успешно сохранены");
                        }

                        NavigationService.Navigate(new GrowingConditionsPage());
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

        private bool ValidateInput()
        {
            if (GrapeVarietyComboBox.SelectedValue == null)
            {
                MessageBox.Show("Пожалуйста, выберите сорт винограда.");
                return false;
            }

            if (SoilComboBox.SelectedValue == null)
            {
                MessageBox.Show("Пожалуйста, выберите тип почвы.");
                return false;
            }

            if (WaterComboBox.SelectedValue == null)
            {
                MessageBox.Show("Пожалуйста, выберите тип воды.");
                return false;
            }

            if (ClimateComboBox.SelectedValue == null)
            {
                MessageBox.Show("Пожалуйста, выберите тип климата.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите описание.");
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
