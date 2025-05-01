using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
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
    /// Логика взаимодействия для JobTitlesDialog.xaml
    /// </summary>
    public partial class JobTitlesDialog : Page
    {
        public JobTitles JobTitle;

        public JobTitlesDialog(JobTitles jobTitle)
        {
            InitializeComponent();

            if (jobTitle != null)
            {
                JobTitle = jobTitle;
                JobTitleTextBox.Text = jobTitle.JobTitle;
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
                        if (JobTitle != null && JobTitle.IDJobTitle > 0)
                        {
                            var existingJobTitle = db.JobTitles.Find(JobTitle.IDJobTitle);

                            if (existingJobTitle != null)
                            {
                                existingJobTitle.JobTitle = JobTitleTextBox.Text.Trim();
                                db.SaveChanges();
                                MessageBox.Show("Job title updated successfully");
                            }
                            else
                            {
                                MessageBox.Show("Job title not found in database");
                                return;
                            }
                        }
                        else
                        {
                            var jobTitle = new JobTitles
                            {
                                JobTitle = JobTitleTextBox.Text.Trim()
                            };

                            db.JobTitles.Add(jobTitle);
                            db.SaveChanges();
                            MessageBox.Show("New job title saved successfully");
                        }

                        NavigationService.Navigate(new JobTitlesPage());
                    }
                    catch (DbEntityValidationException ex)
                    {
                        var errorMessages = ex.EntityValidationErrors
                            .SelectMany(x => x.ValidationErrors)
                            .Select(x => $"{x.PropertyName}: {x.ErrorMessage}");

                        MessageBox.Show($"Validation errors:\n{string.Join("\n", errorMessages)}");
                    }
                    catch (DbUpdateException dbEx)
                    {
                        MessageBox.Show($"Database error: {(dbEx.InnerException?.Message ?? dbEx.Message)}");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Unexpected error: {ex.Message}");
                    }
                }
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(JobTitleTextBox.Text))
            {
                MessageBox.Show("Please enter a job title.");
                return false;
            }

            if (JobTitleTextBox.Text.Length > 100)
            {
                MessageBox.Show("Job title cannot exceed 100 characters.");
                return false;
            }

            return true;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
