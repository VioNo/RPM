using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
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
    /// Логика взаимодействия для ProductsPage.xaml
    /// </summary>
    public partial class ProductsPage : Page
    {
        public ProductsPage()
        {
            InitializeComponent();
            LoadProducts();
            this.IsVisibleChanged += Page_IsVisibleChanged;

        }
        private void LoadProducts()
        {
            using (var context = new DistilleryRassvetBase())
            {
                var products = context.Products.ToList();
                ListViewProducts.ItemsSource = products;
            }
        }
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            ProductDialog productDialog = new ProductDialog(null);
            NavigationService.Navigate(productDialog);
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedProduct = ListViewProducts.SelectedItem as Products;
            if (selectedProduct != null)
            {
                ProductDialog productDialog = new ProductDialog(selectedProduct);
                NavigationService.Navigate(productDialog);
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите один элемент для редактирования.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var productsToDelete = ListViewProducts.SelectedItems.Cast<Products>().ToList();

            if (productsToDelete.Count == 0)
            {
                MessageBox.Show("Пожалуйста, выберите хотя бы один продукт для удаления.",
                              "Предупреждение",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                return;
            }

            var confirmation = MessageBox.Show($"Вы точно хотите удалить продукты в кол-ве {productsToDelete.Count}?",
                                            "Подтверждение удаления",
                                            MessageBoxButton.YesNo,
                                            MessageBoxImage.Question);

            if (confirmation != MessageBoxResult.Yes)
                return;

            try
            {
                using (var context = new DistilleryRassvetBase())
                {
                    var productIds = productsToDelete.Select(p => p.IDProduct).ToList();
                    
                    var existingProducts = context.Products
                        .Where(p => productIds.Contains(p.IDProduct))
                        .ToList();

                    if (existingProducts.Count == 0)
                    {
                        MessageBox.Show("Выбранные продукты не найдены в базе данных.",
                                      "Ошибка",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Error);
                        return;
                    }

                    context.Products.RemoveRange(existingProducts);
                    int deletedCount = context.SaveChanges();

                    if (deletedCount > 0)
                    {
                        MessageBox.Show($"Успешно удалено продукты в кол-ве {deletedCount}.",
                                      "Успех",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Information);
                        LoadProducts();
                    }
                    else
                    {
                        MessageBox.Show("Не удалось удалить продукты.",
                                      "Ошибка",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Error);
                    }
                }
            }
            catch (DbUpdateException dbEx)
            {
                string errorMessage = dbEx.InnerException?.Message ?? dbEx.Message;

                if (errorMessage.Contains("FK_") || errorMessage.Contains("foreign key"))
                {
                    MessageBox.Show("Невозможно удалить продукт, так как он используется в других таблицах.",
                                  "Ошибка",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show($"Ошибка базы данных: {errorMessage}",
                                  "Ошибка",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }
        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.Visibility == Visibility.Visible)
            {
                LoadProducts();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
           NavigationService.Navigate(new DescriptionDB());
        }
    }
}
