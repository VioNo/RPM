using System;
using System.Collections.Generic;
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
    /// Логика взаимодействия для GrapeVarietiesPage.xaml
    /// </summary>
    public partial class GrapeVarietiesPage : Page
    {
        public GrapeVarietiesPage()
        {
            InitializeComponent();
            LoadGrapeVarieties();
        }
        private void LoadGrapeVarieties()
        {
            using (var context = new DistilleryRassvetBase())
            {
                var grape = context.GrapeVarieties.ToList();
                ListViewGrapeVarieties.ItemsSource = grape;
            }
        }
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new DescriptionDB());
        }
    }
}
