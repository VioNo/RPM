using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Security.Policy;
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
    /// Логика взаимодействия для ProductDialog.xaml
    /// </summary>
    public partial class ProductDialog : Page
    {
        public Products Product;

        public ProductDialog(Products product)
        {
            InitializeComponent();
            if (product != null)
            {
                Product = product;
                DataContext = product;

                NameTextBox.Text = Product.Name;
                PrimeCostTextBox.Text = Product.primecost.ToString();
                DescriptionTextBox.Text = Product.Description;
                IDStorageTextBox.Text = Product.IDParty.ToString();
                IDPartyTextBox.Text = Product.IDParty.ToString();
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

                                if (!int.TryParse(IDPartyTextBox.Text, out int idParty) || !db.Party.Any(p => p.IDParty == idParty))
                                {
                                    MessageBox.Show("Некорректный или несуществующий ID партии");
                                    return;
                                }

                                if (!int.TryParse(IDStorageTextBox.Text, out int idStorage) || !db.Storage.Any(s => s.IDStorage == idStorage))
                                {
                                    MessageBox.Show("Некорректный или несуществующий ID склада");
                                    return;
                                }

                                existingProduct.Name = NameTextBox.Text.Trim();
                                existingProduct.primecost = primeCost;
                                existingProduct.Description = DescriptionTextBox.Text.Trim();
                                existingProduct.IDParty = idParty;
                                existingProduct.IDStorage = idStorage;

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

                            if (!int.TryParse(IDPartyTextBox.Text, out int idParty) || !db.Party.Any(p => p.IDParty == idParty))
                            {
                                MessageBox.Show("Некорректный или несуществующий ID партии");
                                return;
                            }

                            if (!int.TryParse(IDStorageTextBox.Text, out int idStorage) || !db.Storage.Any(s => s.IDStorage == idStorage))
                            {
                                MessageBox.Show("Некорректный или несуществующий ID склада");
                                return;
                            }

                            var product = new Products
                            {
                                Name = NameTextBox.Text.Trim(),
                                primecost = primeCost,
                                Description = DescriptionTextBox.Text.Trim(),
                                IDParty = idParty,
                                IDStorage = idStorage
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
        private bool ValidateInput()
        {
            decimal primeCost;
            int idParty;
            int idStorage;

            if (!decimal.TryParse(PrimeCostTextBox.Text, out primeCost))
            {
                MessageBox.Show("Пожалуйста, введите корректное значение для себестоимости.");
                return false;
            }

            if (!int.TryParse(IDPartyTextBox.Text, out idParty))
            {
                MessageBox.Show("Пожалуйста, введите корректное значение для ID партии.");
                return false;
            }

            if (!int.TryParse(IDStorageTextBox.Text, out idStorage))
            {
                MessageBox.Show("Пожалуйста, введите корректное значение для ID склада.");
                return false;
            }

            return true;
        }
    }
}
