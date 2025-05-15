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
using System.Windows.Forms.DataVisualization.Charting;
using Word = Microsoft.Office.Interop.Word;
using Excel = Microsoft.Office.Interop.Excel;
using Microsoft.Office.Interop.Word;
using System.Data.Entity;
using System.Runtime.InteropServices;


namespace RPM
{
    /// <summary>
    /// Логика взаимодействия для GraphsPage.xaml
    /// </summary>
    public partial class GraphsPage : System.Windows.Controls.Page
    {
        private DistilleryRassvetBase _context;

        public GraphsPage()
        {
            InitializeComponent();
            Loaded += Page_Loaded;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _context = DistilleryRassvetBase.GetContext();
                if (_context == null)
                {
                    MessageBox.Show("Не удалось подключиться к базе данных");
                    return;
                }
                InitializeChart();
                LoadComboBoxData();
                UpdateChart(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void InitializeChart()
        {
            ChartPayments.ChartAreas.Add(new System.Windows.Forms.DataVisualization.Charting.ChartArea("MainArea"));

            var series = new System.Windows.Forms.DataVisualization.Charting.Series("OrdersSeries")
            {
                ChartType = SeriesChartType.Column,
                IsValueShownAsLabel = true,
                Color = System.Drawing.Color.SteelBlue
            };
            ChartPayments.Series.Add(series);
        }

        
        private void LoadComboBoxData()
        {
            try
            {
                var jobTitles = _context.JobTitles.ToList();
                if (!jobTitles.Any())
                {
                    MessageBox.Show("Список должностей пуст");
                }
                CmbJobTitle.ItemsSource = jobTitles;
                CmbJobTitle.DisplayMemberPath = "JobTitle";
                CmbJobTitle.SelectedIndex = 0;

                var chartTypes = Enum.GetValues(typeof(SeriesChartType)).Cast<SeriesChartType>().ToList();
                CmbDiagramType.ItemsSource = chartTypes;
                CmbDiagramType.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void UpdateChart(object sender, SelectionChangedEventArgs e)
        {
            if (CmbJobTitle.SelectedItem == null || CmbDiagramType.SelectedItem == null)
                return;

            try
            {
                var selectedJob = CmbJobTitle.SelectedItem as JobTitles;
                var chartType = (SeriesChartType)CmbDiagramType.SelectedItem;

                var series = ChartPayments.Series["OrdersSeries"];
                series.Points.Clear();
                series.ChartType = chartType;

                var employees = _context.Employees
                    .Where(emp => emp.IDJobTitle == selectedJob.IDJobTitle)
                    .ToList();

                foreach (var employee in employees)
                {
                    double totalSum = _context.Orders
                        .Where(o => o.IDEmployee == employee.IDEmployee)
                        .Sum(o => (double)o.Sum);

                    series.Points.AddXY(employee.FullName, totalSum);
                }

                ChartPayments.ChartAreas["MainArea"].AxisX.Title = "Сотрудники";
                ChartPayments.ChartAreas["MainArea"].AxisY.Title = "Сумма заказов";
                ChartPayments.Titles.Clear();
                ChartPayments.Titles.Add($"Заказы сотрудников ({selectedJob.JobTitle})");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления диаграммы: {ex.Message}");
            }
        }

        private void BtnExcel_Click(object sender, RoutedEventArgs e)
        {
            Excel.Application excelApp = null;
            Excel.Workbook workbook = null;

            try
            {
                
                var allEmployees = _context.Employees
                    .Include(ex => ex.Orders.Select(o => o.Products))
                    .OrderBy(er => er.FullName)
                    .ToList();

                excelApp = new Excel.Application();
                workbook = excelApp.Workbooks.Add();
                Excel.Worksheet worksheet = (Excel.Worksheet)workbook.Sheets[1];
                worksheet.Name = "Все сотрудники";

                int startRow = 1;

                
                worksheet.Cells[startRow, 1] = "Сотрудник";
                worksheet.Cells[startRow, 2] = "Должность";
                worksheet.Cells[startRow, 3] = "Дата заказа";
                worksheet.Cells[startRow, 4] = "Товар";
                worksheet.Cells[startRow, 5] = "Количество";
                worksheet.Cells[startRow, 6] = "Сумма";

               
                Excel.Range headerRange = worksheet.Range["A1:F1"];
                headerRange.Font.Bold = true;
                headerRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                startRow++;

                foreach (var employee in allEmployees)
                {
                    if (employee.Orders == null || !employee.Orders.Any())
                    {
                        worksheet.Cells[startRow, 1] = employee.FullName;
                        worksheet.Cells[startRow, 2] = employee.JobTitles?.JobTitle ?? "Не указано";
                        worksheet.Cells[startRow, 3] = "Нет заказов";
                        startRow++;
                        continue;
                    }

                    foreach (var order in employee.Orders)
                    {
                        worksheet.Cells[startRow, 1] = employee.FullName;
                        worksheet.Cells[startRow, 2] = employee.JobTitles?.JobTitle ?? "Не указано";
                        worksheet.Cells[startRow, 3] = order.DateOrder.ToString("dd.MM.yyyy");
                        worksheet.Cells[startRow, 4] = order.Products?.Name ?? "Не указано";
                        worksheet.Cells[startRow, 5] = order.Count;
                        worksheet.Cells[startRow, 6] = order.Sum;

                        startRow++;
                    }
                }

               
                worksheet.Columns.AutoFit();

                string path = @"V:\source\repos\RPM\EmployeesOrders.xlsx";
                workbook.SaveAs(path);
                excelApp.Visible = true;

                MessageBox.Show($"Файл сохранен: {path}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
            finally
            {
                if (workbook != null) Marshal.ReleaseComObject(workbook);
                if (excelApp != null) Marshal.ReleaseComObject(excelApp);
            }
        }

        private void BtnWord_Click(object sender, RoutedEventArgs e)
        {
            Word.Application wordApp = null;
            Word.Document document = null;

            try
            {
                
                var allEmployees = _context.Employees
                    .Include(et => et.JobTitles)
                    .Include(ek => ek.Orders.Select(o => o.Products))
                    .OrderBy(eb => eb.FullName)
                    .ToList();

                wordApp = new Word.Application();
                document = wordApp.Documents.Add();

                foreach (var employee in allEmployees)
                {
                 
                    Word.Paragraph para = document.Paragraphs.Add();
                    Word.Range range = para.Range;
                    range.Text = $"{employee.FullName} ({employee.JobTitles?.JobTitle ?? "Должность не указана"})";
                    range.Font.Bold = 1;
                    range.Font.Size = 14;
                    range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    range.InsertParagraphAfter();

                    if (employee.Orders == null || !employee.Orders.Any())
                    {
                        para = document.Paragraphs.Add();
                        range = para.Range;
                        range.Text = "Нет данных о заказах";
                        range.Font.Italic = 1;
                        range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                        range.InsertParagraphAfter();
                        continue;
                    }

                    Word.Table table = document.Tables.Add(
                        document.Paragraphs.Add().Range,
                        employee.Orders.Count + 1,
                        4);

                    table.Cell(1, 1).Range.Text = "Дата";
                    table.Cell(1, 2).Range.Text = "Товар";
                    table.Cell(1, 3).Range.Text = "Количество";
                    table.Cell(1, 4).Range.Text = "Сумма";

                    for (int i = 0; i < employee.Orders.Count; i++)
                    {
                        var order = employee.Orders.ElementAt(i);
                        table.Cell(i + 2, 1).Range.Text = order.DateOrder.ToString("dd.MM.yyyy");
                        table.Cell(i + 2, 2).Range.Text = order.Products?.Name ?? "Не указано";
                        table.Cell(i + 2, 3).Range.Text = order.Count.ToString();
                        table.Cell(i + 2, 4).Range.Text = order.Sum.ToString("N2") + " руб.";
                    }

                    Word.Paragraph totalPara = document.Paragraphs.Add();
                    Word.Range totalRange = totalPara.Range;
                    totalRange.Text = $"Итого: {employee.Orders.Sum(o => o.Sum):N2} руб.";
                    totalRange.Font.Bold = 1;
                    totalRange.InsertParagraphAfter();

                    if (employee != allEmployees.Last())
                    {
                        document.Words.Last.InsertBreak(Word.WdBreakType.wdPageBreak);
                    }
                }

                string docPath = @"V:\source\repos\RPM\EmployeesOrders.docx";
                string pdfPath = @"V:\source\repos\RPM\EmployeesOrders.pdf";

                document.SaveAs2(docPath);
                document.SaveAs2(pdfPath, Word.WdExportFormat.wdExportFormatPDF);
                wordApp.Visible = true;

                MessageBox.Show($"Документы сохранены:\n{docPath}\n{pdfPath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
            finally
            {
                if (document != null) Marshal.ReleaseComObject(document);
                if (wordApp != null) Marshal.ReleaseComObject(wordApp);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new DescriptionDB());
        }
    }
}
