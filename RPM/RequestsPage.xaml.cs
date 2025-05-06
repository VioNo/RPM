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
        private int _startMonth = 1;
        private int _endMonth = 12;

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

        public int StartMonth
        {
            get => _startMonth;
            set
            {
                _startMonth = value;
                OnPropertyChanged(nameof(StartMonth));
            }
        }

        public int EndMonth
        {
            get => _endMonth;
            set
            {
                _endMonth = value;
                OnPropertyChanged(nameof(EndMonth));
            }
        }

        public ICommand YieldCommand { get; }
        public ICommand OrdersReportCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public RequestsViewModel()
        {
            YieldCommand = new RelayCommand(ExecuteYield);
            OrdersReportCommand = new RelayCommand(ExecuteOrdersReport);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ExecuteYield()
        {
            if (!ValidateYearMonth()) return;

            IsBusy = true;
            StatusMessage = $"Загрузка данных об урожае за {StartMonth}-{EndMonth}.{SelectedYear}...";

            try
            {
                using (var db = new DistilleryRassvetBase())
                {
                    var query = db.Yield
                        .Where(y => y.DateYield.Year == SelectedYear &&
                                   y.DateYield.Month >= StartMonth &&
                                   y.DateYield.Month <= EndMonth)
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

        private void ExecuteOrdersReport()
        {
            if (!ValidateYearMonthRange()) return;

            IsBusy = true;
            StatusMessage = $"Загрузка данных о заказах за период {StartMonth}-{EndMonth}.{SelectedYear}...";

            try
            {
                using (var db = new DistilleryRassvetBase())
                {
                    var query = db.Orders
                        .Where(o => o.DateOrder.Year == SelectedYear &&
                                   o.DateOrder.Month >= StartMonth &&
                                   o.DateOrder.Month <= EndMonth)
                        .Join(db.Clients,
                            o => o.IDClient,
                            c => c.IDClient,
                            (o, c) => new { o, c })
                        .Join(db.Products,
                            oc => oc.o.IDProduct,
                            p => p.IDProduct,
                            (oc, p) => new
                            {
                                Название = p.Name,
                                Количество = oc.o.Count,
                                Дата = oc.o.DateOrder,
                                Клиент = oc.c.Name + " " + oc.c.Surname,
                                Сумма = oc.o.Sum
                            })
                        .OrderByDescending(x => x.Дата)
                        .ToList();

                    QueryResults = ConvertToDataView(query);
                    StatusMessage = $"Найдено {query.Count} заказов за период {StartMonth}-{EndMonth}.{SelectedYear}";
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

            return ValidateMonthRange();
        }

        private bool ValidateYearMonthRange()
        {
            if (SelectedYear < 2000 || SelectedYear > DateTime.Now.Year + 5)
            {
                MessageBox.Show("Укажите корректный год (2000-" + (DateTime.Now.Year + 5) + ")");
                return false;
            }

            return ValidateMonthRange();
        }

        private bool ValidateMonthRange()
        {
            if (StartMonth < 1 || StartMonth > 12)
            {
                MessageBox.Show("Укажите корректный начальный месяц (1-12)");
                return false;
            }

            if (EndMonth < 1 || EndMonth > 12)
            {
                MessageBox.Show("Укажите корректный конечный месяц (1-12)");
                return false;
            }

            if (StartMonth > EndMonth)
            {
                MessageBox.Show("Начальный месяц не может быть больше конечного");
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