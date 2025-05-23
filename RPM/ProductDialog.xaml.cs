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
                PrimeCostTextBox.Text = Product.PrimeCost.ToString();
                DescriptionTextBox.Text = Product.Description;
                CountTextBox.Text = Product.Count?.ToString() ?? "0"; // Добавлено поле количества
            }
        }

        private void LoadComboBoxData()
        {
            using (var db = new DistilleryRassvetBase())
            {
                _parties = db.Party.ToList();
                _storages = db.Storage.ToList();

                PartyComboBox.ItemsSource = _parties;
                PartyComboBox.DisplayMemberPath = "PartyDescription";
                PartyComboBox.SelectedValuePath = "IDParty";

                StorageComboBox.ItemsSource = _storages;
                StorageComboBox.DisplayMemberPath = "Address";
                StorageComboBox.SelectedValuePath = "IDStorage";

                if (Product != null)
                {
                    if (Product.IDParty > 0)
                        PartyComboBox.SelectedValue = Product.IDParty;

                    if (Product.IDStorageWine > 0)
                        StorageComboBox.SelectedValue = Product.IDStorageWine;
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
                return;

            using (var db = new DistilleryRassvetBase())
            {
                try
                {
                    if (!int.TryParse(CountTextBox.Text, out int count) || count <= 0)
                    {
                        MessageBox.Show("Введите корректное количество продукта");
                        return;
                    }

                    if (Product != null && Product.IDProduct > 0)
                    {
                        UpdateExistingProduct(db, count);
                    }
                    else
                    {
                        CreateNewProduct(db, count);
                    }
                    NavigationService.Navigate(new ProductsPage());
                }
                catch (DbEntityValidationException ex)
                {
                    ShowValidationErrors(ex);
                }
                catch (DbUpdateException dbEx)
                {
                    MessageBox.Show($"Ошибка базы данных: {dbEx.InnerException?.Message ?? dbEx.Message}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Неожиданная ошибка: {ex.Message}");
                }
            }
        }

        private void UpdateExistingProduct(DistilleryRassvetBase db, int count)
        {
            var existingProduct = db.Products.Find(Product.IDProduct);
            if (existingProduct == null)
            {
                MessageBox.Show("Продукт не найден в базе данных");
                return;
            }

            if (!decimal.TryParse(PrimeCostTextBox.Text, out decimal primeCost) || primeCost <= 0)
            {
                MessageBox.Show("Некорректное значение стоимости продукта");
                return;
            }

            // Проверка изменения склада
            int newStorageId = (int)StorageComboBox.SelectedValue;
            if (existingProduct.IDStorageWine != newStorageId)
            {
                // Если склад изменился, проверяем новый склад
                if (!ValidateStorageCapacity(db, newStorageId, count))
                    return;
            }
            else
            {
                // Если склад не изменился, проверяем изменение количества
                int quantityChange = count - (existingProduct.Count ?? 0);
                if (quantityChange > 0 && !ValidateStorageCapacity(db, newStorageId, quantityChange))
                    return;
            }

            existingProduct.Name = NameTextBox.Text.Trim();
            existingProduct.PrimeCost = primeCost;
            existingProduct.Description = DescriptionTextBox.Text.Trim();
            existingProduct.Count = count;
            existingProduct.IDParty = (int)PartyComboBox.SelectedValue;
            existingProduct.IDStorageWine = newStorageId;

            db.SaveChanges();
            MessageBox.Show("Данные продукта успешно обновлены");
        }

        private void CreateNewProduct(DistilleryRassvetBase db, int count)
        {
            if (!decimal.TryParse(PrimeCostTextBox.Text, out decimal primeCost) || primeCost <= 0)
            {
                MessageBox.Show("Некорректное значение стоимости продукта");
                return;
            }

            int storageId = (int)StorageComboBox.SelectedValue;
            if (!ValidateStorageCapacity(db, storageId, count))
                return;

            var product = new Products
            {
                Name = NameTextBox.Text.Trim(),
                PrimeCost = primeCost,
                Description = DescriptionTextBox.Text.Trim(),
                Count = count,
                IDParty = (int)PartyComboBox.SelectedValue,
                IDStorageWine = storageId
            };

            db.Products.Add(product);
            db.SaveChanges();
            MessageBox.Show("Новый продукт успешно сохранен");
        }

        private bool ValidateStorageCapacity(DistilleryRassvetBase db, int storageId, int productCount)
        {
            var storage = db.Storage.FirstOrDefault(s => s.IDStorage == storageId);
            if (storage == null)
            {
                MessageBox.Show("Выбранный склад не найден в базе данных");
                return false;
            }

            // Расчет объема продукта (количество / 100)
            decimal productVolume = productCount / 100m;

            if (storage.Fullness + productVolume > 100m)
            {
                MessageBox.Show($"Ошибка: Склад '{storage.Address}' заполнен на {storage.Fullness}%.\n" +
                              $"Добавление этого продукта (+{productVolume:F2}%) превысит максимальную вместимость (100%).\n" +
                              $"Максимально можно добавить: {(100 - storage.Fullness) * 100:F0} единиц продукта.\n" +
                              $"Пожалуйста, выберите другой склад или уменьшите количество.");
                return false;
            }

            return true;
        }

        private void ShowValidationErrors(DbEntityValidationException ex)
        {
            var errorMessages = ex.EntityValidationErrors
                .SelectMany(x => x.ValidationErrors)
                .Select(x => $"{x.PropertyName}: {x.ErrorMessage}");

            MessageBox.Show($"Ошибки валидации:\n{string.Join("\n", errorMessages)}");
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ProductsPage());
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите название продукта.");
                return false;
            }

            if (!decimal.TryParse(PrimeCostTextBox.Text, out decimal primeCost) || primeCost <= 0)
            {
                MessageBox.Show("Пожалуйста, введите корректное положительное значение для себестоимости.");
                return false;
            }

            if (!int.TryParse(CountTextBox.Text, out int count) || count <= 0)
            {
                MessageBox.Show("Пожалуйста, введите корректное положительное значение количества.");
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