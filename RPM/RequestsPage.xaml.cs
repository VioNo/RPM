using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
    /// Логика взаимодействия для RequestsPage.xaml
    /// </summary>
    public partial class RequestsPage : Page
    {
        public RequestsPage()
        {
            InitializeComponent();
            this.DataContext = new RequestsViewModel();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new DescriptionDB());
        }
    }

    public class RequestsViewModel : INotifyPropertyChanged
    {
        private DataView _queryResults;
        private string _statusMessage;
        private bool _isBusy;
        private int _selectedYear = DateTime.Now.Year;
        private int _selectedMonth = DateTime.Now.Month;

        public DataView QueryResults
        {
            get => _queryResults;
            set
            {
                _queryResults = value;
                OnPropertyChanged(nameof(QueryResults));
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
            }
        }

        public int SelectedYear
        {
            get => _selectedYear;
            set
            {
                _selectedYear = value;
                OnPropertyChanged(nameof(SelectedYear));
            }
        }

        public int SelectedMonth
        {
            get => _selectedMonth;
            set
            {
                _selectedMonth = value;
                OnPropertyChanged(nameof(SelectedMonth));
            }
        }

        public ICommand YieldCommand { get; }
        public ICommand FermentationCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public RequestsViewModel()
        {
            YieldCommand = new RelayCommand(ExecuteYield);
            FermentationCommand = new RelayCommand(ExecuteFermentation);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ExecuteYield()
        {
            if (!ValidateYearMonth()) return;

            IsBusy = true;
            StatusMessage = $"Загрузка данных об урожае за {SelectedMonth}.{SelectedYear}...";

            try
            {
                using (var db = new DistilleryRassvetBase())
                {
                    var query = db.Yield
                        .Where(y => y.DateYield.Month == SelectedMonth && y.DateYield.Year == SelectedYear)
                        .Join(db.GrapeVarieties,
                            y => y.IDGrapeVarieties,
                            gv => gv.IDGrapeVarieties,
                            (y, gv) => new { y, gv })
                        .Join(db.GrowingConditions,
                            yg => yg.y.IDGrapeVarieties,
                            gc => gc.IDGrapeVarieties,
                            (yg, gc) => new
                            {
                                Сорт = yg.gv.NameGrapeVarieties,
                                Дата_сбора = yg.y.DateYield,
                                Количество = yg.y.Harvest,
                                Условия = gc.Description
                            })
                        .OrderByDescending(x => x.Дата_сбора)
                        .ToList();

                    QueryResults = ConvertToDataView(query);
                    StatusMessage = $"Найдено {query.Count} записей об урожае";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "Ошибка: " + ex.Message;
                QueryResults = null;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ExecuteFermentation()
        {
            if (!ValidateYearMonth()) return;

            IsBusy = true;
            StatusMessage = $"Загрузка данных о ферментации за {SelectedMonth}.{SelectedYear}...";

            try
            {
                using (var db = new DistilleryRassvetBase())
                {
                    // Преобразуем IDGrowingConditions в int для join
                    var finalQuery = db.Fermentation
                        .Where(f => f.EndDate.Month == SelectedMonth && f.EndDate.Year == SelectedYear)
                        .Join(db.StorageWine,
                            f => f.IDFermentation,
                            sw => sw.IDFermentation,
                            (f, sw) => new { Fermentation = f, StorageWine = sw })
                        .Join(db.Products,
                            temp => temp.StorageWine.IDParty,
                            p => p.IDParty,
                            (temp, p) => new { temp.Fermentation, temp.StorageWine, Product = p })
                        .Join(db.GrowingConditions,
                            temp => Convert.ToInt64(temp.StorageWine.IDGrowingConditions), // Преобразуем string в int
                            gc => gc.IDGrowingConditions,
                            (temp, gc) => new
                            {
                                Название = temp.Product.Name,
                                Дата_начала = temp.Fermentation.StartDate,
                                Дата_завершения = temp.Fermentation.EndDate,
                                Условия = gc.Description,
                                Дата_розлива = temp.StorageWine.ExpirationDate
                            })
                        .OrderByDescending(x => x.Дата_завершения)
                        .ToList();

                    QueryResults = ConvertToDataView(finalQuery);
                    StatusMessage = $"Найдено {finalQuery.Count} записей о ферментации";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "Ошибка: " + ex.Message;
                QueryResults = null;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool ValidateYearMonth()
        {
            if (SelectedYear < 2000 || SelectedYear > DateTime.Now.Year + 5)
            {
                MessageBox.Show("Укажите корректный год (2000-" + (DateTime.Now.Year + 5) + ")");
                return false;
            }

            if (SelectedMonth < 1 || SelectedMonth > 12)
            {
                MessageBox.Show("Укажите корректный месяц (1-12)");
                return false;
            }

            return true;
        }

        private DataView ConvertToDataView<T>(IEnumerable<T> list)
        {
            var table = new DataTable();
            var props = typeof(T).GetProperties();

            foreach (var prop in props)
            {
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }

            foreach (var item in list)
            {
                var row = table.NewRow();
                foreach (var prop in props)
                {
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                }
                table.Rows.Add(row);
            }

            return table.DefaultView;
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public event EventHandler CanExecuteChanged;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object parameter) => _execute();

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        
    }

}
