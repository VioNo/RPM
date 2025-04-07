using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace RPM
{
    /// <summary>
    /// Логика взаимодействия для ProductDialog.xaml
    /// </summary>
    public partial class ProductDialog : Page
    {
        public Products Product;
        private List<Party> _parties;
        private List<Storage> _storages;

        public ProductDialog(Products product)
        {
            InitializeComponent();
            LoadComboBoxData();

            if (product != null)
            {
                Product = product;
                DataContext = product;

                NameTextBox.Text = Product.Name;
                PrimeCostTextBox.Text = Product.primecost.ToString();
                DescriptionTextBox.Text = Product.Description;

                // Устанавливаем выбранные элементы в ComboBox
                if (Product.IDParty > 0)
                    PartyComboBox.SelectedValue = Product.IDParty;

                if (Product.IDStorage > 0)
                    StorageComboBox.SelectedValue = Product.IDStorage;
            }
        }

        private void LoadComboBoxData()
        {
            using (var db = new DistilleryRassvetBase())
            {
                _parties = db.Party.ToList();
                _storages = db.Storage.ToList();

                // Настройка ComboBox для партий
                PartyComboBox.ItemsSource = _parties;
                PartyComboBox.DisplayMemberPath = "PartyDescription";
                PartyComboBox.SelectedValuePath = "IDParty";

                // Настройка ComboBox для складов
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
                        if (Product != null && Product.IDProduct > 0)
                        {
                            var existingProduct = db.Products.Find(Product.IDProduct);

                            if (existingProduct != null)
                            {
                                if (!decimal.TryParse(PrimeCostTextBox.Text, out decimal primeCost))
                                {
                                    MessageBox.Show("Некорректное значение стоимости продукта");
                                    return;
                                }

                                existingProduct.Name = NameTextBox.Text.Trim();
                                existingProduct.primecost = primeCost;
                                existingProduct.Description = DescriptionTextBox.Text.Trim();
                                existingProduct.IDParty = (int)PartyComboBox.SelectedValue;
                                existingProduct.IDStorage = (int)StorageComboBox.SelectedValue;

                                var validationErrors = db.GetValidationErrors();
                                if (validationErrors.Any())
                                {
                                    var errorMessages = validationErrors
                                        .SelectMany(validationError => validationError.ValidationErrors)
                                        .Select(validationErrorItem => $"{validationErrorItem.PropertyName}: {validationErrorItem.ErrorMessage}");

                                    MessageBox.Show($"Ошибки валидации:\n{string.Join("\n", errorMessages)}");
                                    return;
                                }

                                db.SaveChanges();
                                MessageBox.Show("Данные продукта успешно обновлены");
                            }
                            else
                            {
                                MessageBox.Show("Продукт не найден в базе данных");
                                return;
                            }
                        }
                        else
                        {
                            if (!decimal.TryParse(PrimeCostTextBox.Text, out decimal primeCost))
                            {
                                MessageBox.Show("Некорректное значение стоимости продукта");
                                return;
                            }

                            var product = new Products
                            {
                                Name = NameTextBox.Text.Trim(),
                                primecost = primeCost,
                                Description = DescriptionTextBox.Text.Trim(),
                                IDParty = (int)PartyComboBox.SelectedValue,
                                IDStorage = (int)StorageComboBox.SelectedValue
                            };

                            db.Products.Add(product);
                            db.SaveChanges();
                            MessageBox.Show("Новый продукт успешно сохранен");
                        }

                        NavigationService.Navigate(new ProductsPage());
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
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ProductsPage());
        }
        private bool ValidateInput()
        {
            if (!decimal.TryParse(PrimeCostTextBox.Text, out decimal primeCost))
            {
                MessageBox.Show("Пожалуйста, введите корректное значение для себестоимости.");
                return false;
            }

            if (PartyComboBox.SelectedValue == null)
            {
                MessageBox.Show("Пожалуйста, выберите партию.");
                return false;
            }

            if (StorageComboBox.SelectedValue == null)
            {
                MessageBox.Show("Пожалуйста, выберите склад.");
                return false;
            }

            return true;
        }
    }
}