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
    /// Логика взаимодействия для FermentationDialog.xaml
    /// </summary>
    public partial class FermentationDialog : Page
    {
        public Fermentation Fermentation;
        private List<Yield> _yields;
        private List<GrowingConditions> _growingConditions;

        public FermentationDialog(Fermentation fermentation)
        {
            InitializeComponent();
            LoadComboBoxData();

            if (fermentation != null)
            {
                Fermentation = fermentation;

                YieldComboBox.SelectedValue = fermentation.IDYield;
                StartDatePicker.SelectedDate = fermentation.StartDate;
                EndDatePicker.SelectedDate = fermentation.EndDate;
                TemperatureTextBox.Text = fermentation.Temperature.ToString();
                SugarLevelTextBox.Text = fermentation.LevelSugar.ToString();
                DescriptionTextBox.Text = fermentation.Description;
                GrowingConditionsComboBox.SelectedValue = fermentation.IDGrowingConditions;
                CountTextBox.Text = fermentation.Count?.ToString();
                MeasureTextBox.Text = fermentation.Measure;
            }
        }

        private void LoadComboBoxData()
        {
            using (var db = new DistilleryRassvetBase())
            {
                // Загрузка данных с включением связанных сущностей
                _yields = db.Yield
                    .Include("GrapeVarieties")
                    .OrderBy(y => y.DateYield)
                    .ToList();

                _growingConditions = db.GrowingConditions
                    .Include("GrapeVarieties")
                    .Include("Climate")
                    .Include("Soil")
                    .Include("Water")
                    .OrderBy(g => g.Description)
                    .ToList();

                // Настройка ComboBox для урожая
                YieldComboBox.ItemsSource = _yields;
                YieldComboBox.DisplayMemberPath = "DisplayText";
                YieldComboBox.SelectedValuePath = "IDYield";

                // Настройка ComboBox для условий выращивания
                GrowingConditionsComboBox.ItemsSource = _growingConditions;
                GrowingConditionsComboBox.DisplayMemberPath = "FullDescription";
                GrowingConditionsComboBox.SelectedValuePath = "IDGrowingConditions";
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
                        if (Fermentation != null && Fermentation.IDFermentation > 0)
                        {
                            var existingFermentation = db.Fermentation.Find(Fermentation.IDFermentation);

                            if (existingFermentation != null)
                            {
                                existingFermentation.IDYield = (int)YieldComboBox.SelectedValue;
                                existingFermentation.StartDate = StartDatePicker.SelectedDate.Value;
                                existingFermentation.EndDate = EndDatePicker.SelectedDate.Value;
                                existingFermentation.Temperature = decimal.Parse(TemperatureTextBox.Text);
                                existingFermentation.LevelSugar = decimal.Parse(SugarLevelTextBox.Text);
                                existingFermentation.Description = DescriptionTextBox.Text.Trim();
                                existingFermentation.IDGrowingConditions = (int)GrowingConditionsComboBox.SelectedValue;

                                if (int.TryParse(CountTextBox.Text, out int count))
                                {
                                    existingFermentation.Count = count;
                                }
                                else
                                {
                                    existingFermentation.Count = null;
                                }

                                existingFermentation.Measure = MeasureTextBox.Text.Trim();

                                db.SaveChanges();
                                MessageBox.Show("Данные брожения успешно обновлены");
                            }
                            else
                            {
                                MessageBox.Show("Процесс брожения не найден в базе данных");
                                return;
                            }
                        }
                        else
                        {
                            var fermentation = new Fermentation
                            {
                                IDYield = (int)YieldComboBox.SelectedValue,
                                StartDate = StartDatePicker.SelectedDate.Value,
                                EndDate = EndDatePicker.SelectedDate.Value,
                                Temperature = decimal.Parse(TemperatureTextBox.Text),
                                LevelSugar = decimal.Parse(SugarLevelTextBox.Text),
                                Description = DescriptionTextBox.Text.Trim(),
                                IDGrowingConditions = (int)GrowingConditionsComboBox.SelectedValue,
                                Measure = MeasureTextBox.Text.Trim()
                            };

                            if (int.TryParse(CountTextBox.Text, out int count))
                            {
                                fermentation.Count = count;
                            }

                            db.Fermentation.Add(fermentation);
                            db.SaveChanges();
                            MessageBox.Show("Новый процесс брожения успешно сохранен");
                        }

                        NavigationService.Navigate(new FermentationPage());
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
            if (YieldComboBox.SelectedValue == null)
            {
                MessageBox.Show("Пожалуйста, выберите урожай.");
                return false;
            }

            if (!StartDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Пожалуйста, выберите дату начала.");
                return false;
            }

            if (!EndDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Пожалуйста, выберите дату окончания.");
                return false;
            }

            if (StartDatePicker.SelectedDate > EndDatePicker.SelectedDate)
            {
                MessageBox.Show("Дата окончания должна быть после даты начала.");
                return false;
            }

            if (!decimal.TryParse(TemperatureTextBox.Text, out decimal temp) || temp <= 0)
            {
                MessageBox.Show("Пожалуйста, введите корректную положительную температуру.");
                return false;
            }

            if (!decimal.TryParse(SugarLevelTextBox.Text, out decimal sugar) || sugar < 0)
            {
                MessageBox.Show("Пожалуйста, введите корректный уровень сахара (положительное число).");
                return false;
            }

            if (GrowingConditionsComboBox.SelectedValue == null)
            {
                MessageBox.Show("Пожалуйста, выберите условия выращивания.");
                return false;
            }

            if (!string.IsNullOrEmpty(CountTextBox.Text) && !int.TryParse(CountTextBox.Text, out int count))
            {
                MessageBox.Show("Пожалуйста, введите корректное количество (целое число) или оставьте пустым.");
                return false;
            }

            return true;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }

    // Расширение класса Yield для отображения в ComboBox
    public partial class Yield
    {
        public string DisplayText
        {
            get
            {
                return $"{DateYield:dd.MM.yyyy} - {GrapeVarieties?.NameGrapeVarieties ?? "Неизвестный сорт"} ({Harvest} кг)";
            }
        }
    }

    // Расширение класса GrowingConditions для отображения в ComboBox
    public partial class GrowingConditions
    {
        public string FullDescription
        {
            get
            {
                return $"{Description} (Сорт: {GrapeVarieties?.NameGrapeVarieties ?? "Неизвестно"})";
            }
        }
    }
}
