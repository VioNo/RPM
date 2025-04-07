using System;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace RPM
{
    public partial class StorageDialog : Page
    {
        public Storage Storage;

        public StorageDialog(Storage storage)
        {
            InitializeComponent();

            if (storage != null)
            {
                Storage = storage;
                AddressTextBox.Text = Storage.Address;
                FullnessTextBox.Text = Storage.Fullness.ToString();
            }
            else
            {
                FullnessTextBox.Text = "0";
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
                        if (Storage != null && Storage.IDStorage > 0)
                        {
                            var existingStorage = db.Storage.Find(Storage.IDStorage);
                            if (existingStorage != null)
                            {
                                UpdateStorage(existingStorage);
                                db.SaveChanges();
                                MessageBox.Show("Данные склада успешно обновлены");
                            }
                        }
                        else
                        {
                            var newStorage = new Storage();
                            UpdateStorage(newStorage);
                            db.Storage.Add(newStorage);
                            db.SaveChanges();
                            MessageBox.Show("Новый склад успешно сохранен");
                        }

                        NavigationService.Navigate(new StoragePage());
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

        private void UpdateStorage(Storage storage)
        {
            storage.Address = AddressTextBox.Text.Trim();
            storage.Fullness = decimal.Parse(FullnessTextBox.Text);
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(AddressTextBox.Text))
            {
                MessageBox.Show("Введите адрес склада");
                return false;
            }

            if (!decimal.TryParse(FullnessTextBox.Text, out decimal fullness) || fullness < 0 || fullness > 100)
            {
                MessageBox.Show("Заполненность должна быть числом от 0 до 100");
                return false;
            }

            return true;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new StoragePage());
        }
    }
}