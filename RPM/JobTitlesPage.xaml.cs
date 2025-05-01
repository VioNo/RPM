using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
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
    /// Логика взаимодействия для JobTitlesPage.xaml
    /// </summary>
    public partial class JobTitlesPage : Page
    {
        private List<JobTitles> _allJobTitles = new List<JobTitles>();

        public JobTitlesPage()
        {
            InitializeComponent();
            LoadJobTitles();
            this.IsVisibleChanged += Page_IsVisibleChanged;
        }

        private void LoadJobTitles()
        {
            using (var context = new DistilleryRassvetBase())
            {
                _allJobTitles = context.JobTitles
                    .Include("Employees")
                    .ToList();
                ListViewJobTitles.ItemsSource = _allJobTitles;
            }
        }

        private void ApplyFilters()
        {
            var filteredJobTitles = _allJobTitles.AsEnumerable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                string searchText = SearchTextBox.Text.ToLower();
                filteredJobTitles = filteredJobTitles.Where(j =>
                    j.JobTitle != null && j.JobTitle.ToLower().Contains(searchText));
            }

            ListViewJobTitles.ItemsSource = filteredJobTitles.ToList();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void SortAscending_Click(object sender, RoutedEventArgs e)
        {
            var jobTitles = ListViewJobTitles.ItemsSource as IEnumerable<JobTitles> ?? _allJobTitles;
            ListViewJobTitles.ItemsSource = jobTitles
                .OrderBy(j => j.JobTitle)
                .ToList();
        }

        private void SortDescending_Click(object sender, RoutedEventArgs e)
        {
            var jobTitles = ListViewJobTitles.ItemsSource as IEnumerable<JobTitles> ?? _allJobTitles;
            ListViewJobTitles.ItemsSource = jobTitles
                .OrderByDescending(j => j.JobTitle)
                .ToList();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            JobTitlesDialog dialog = new JobTitlesDialog(null);
            NavigationService.Navigate(dialog);
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedJobTitle = ListViewJobTitles.SelectedItem as JobTitles;
            if (selectedJobTitle != null)
            {
                JobTitlesDialog dialog = new JobTitlesDialog(selectedJobTitle);
                NavigationService.Navigate(dialog);
            }
            else
            {
                MessageBox.Show("Please select a job title to edit.",
                              "Warning",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var jobTitlesToDelete = ListViewJobTitles.SelectedItems.Cast<JobTitles>().ToList();

            if (jobTitlesToDelete.Count == 0)
            {
                MessageBox.Show("Please select at least one job title to delete.",
                              "Warning",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                return;
            }

            var confirmation = MessageBox.Show($"Are you sure you want to delete {jobTitlesToDelete.Count} job title(s)?\n" +
                                            "This will affect all employees with these job titles.",
                                            "Confirm Deletion",
                                            MessageBoxButton.YesNo,
                                            MessageBoxImage.Question);

            if (confirmation != MessageBoxResult.Yes)
                return;

            try
            {
                using (var context = new DistilleryRassvetBase())
                {
                    var jobTitleIds = jobTitlesToDelete.Select(j => j.IDJobTitle).ToList();

                    // Check if any employees are using these job titles
                    var employeesWithTitles = context.Employees
                        .Any(emp => jobTitleIds.Contains(emp.IDJobTitle));

                    if (employeesWithTitles)
                    {
                        MessageBox.Show("Cannot delete job titles that are assigned to employees.",
                                      "Error",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Error);
                        return;
                    }

                    var existingJobTitles = context.JobTitles
                        .Where(j => jobTitleIds.Contains(j.IDJobTitle))
                        .ToList();

                    if (existingJobTitles.Count == 0)
                    {
                        MessageBox.Show("Selected job titles not found in database.",
                                      "Error",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Error);
                        return;
                    }

                    context.JobTitles.RemoveRange(existingJobTitles);
                    int deletedCount = context.SaveChanges();

                    if (deletedCount > 0)
                    {
                        MessageBox.Show($"Successfully deleted {deletedCount} job title(s).",
                                      "Success",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Information);
                        LoadJobTitles();
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete job titles.",
                                      "Error",
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
                    MessageBox.Show("Cannot delete job titles that are assigned to employees.",
                                  "Error",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show($"Database error: {errorMessage}",
                                  "Error",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}",
                              "Error",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.Visibility == Visibility.Visible)
            {
                LoadJobTitles();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new DescriptionDB());
        }
    }
}
