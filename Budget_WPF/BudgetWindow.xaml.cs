using Budget;
using Microsoft.Win32;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Shapes;
using Path = System.IO.Path;
using System.IO;

namespace Budget_WPF
{
    /// <summary>
    /// Interaction logic for BudgetWindow.xaml
    /// </summary>
    public partial class BudgetWindow : Window, IBudgetView
    {
        private AddOrUpdateExpense _addOrUpdateExpense;
        private Presenter _presenter;
        private string _filename;

        public BudgetWindow()
        {
            InitializeComponent();
            _presenter = new Presenter(this);
            _presenter.ShowFirstTimeUserSetup();
            InitializeWindow();
        }

        public void InitializeWindow()
        {
            //Disables open recent menu item if there is no recent file
            if (string.IsNullOrEmpty(_presenter.GetRecentFile()))
            {
                miOpenRecent.IsEnabled = false;
            }
            miModify.IsEnabled = miDelete.IsEnabled = false;

            dpStartDate.SelectedDate = DateTime.Today.AddYears(-25);
            dpEndDate.SelectedDate = DateTime.Today;
        }

        public void Refresh()
        {
            Category? category = cmbCategories.SelectedItem as Category;
            _presenter.FiltersChange(dpStartDate.SelectedDate, dpEndDate.SelectedDate, category is null ? -1 : category.Id, cbFilterCategory.IsChecked == true, cbFilterByMonth.IsChecked == true, cbFilterByCategory.IsChecked == true);

        }

        public void ShowCurrentFile(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                txtbCurrentFile.Text = "Budget File: None";
            }
            else
            {
                txtbCurrentFile.Text = $"Budget File {filename}";
                Menu_SaveAs.IsEnabled = true;
                btn_AddExpense.IsEnabled = true;
            }
        }

        public void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void UpdateView(object items)
        {
            //Updates the Menu items on the datagrid
            if (cbFilterByMonth.IsChecked == true || cbFilterByCategory.IsChecked == true)
                miModify.IsEnabled = miDelete.IsEnabled = false;
            else
                miModify.IsEnabled = miDelete.IsEnabled = true;

            dgExpenses.Columns.Clear();

            // Create rightAligned style.
            Style rightAligned = new Style();
            rightAligned.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Right));

            // Setup columns according to the type of items.
            if (items is List<BudgetItem>)
            {
                List<BudgetItem> budgetItems = (List<BudgetItem>)items;
                dgExpenses.ItemsSource = budgetItems;

                // Date column
                DataGridTextColumn dateColumn = new();
                dateColumn.Header = "Date";
                dateColumn.Binding = new Binding("Date");
                dateColumn.Binding.StringFormat = "dd/MM/yyyy";
                dgExpenses.Columns.Add(dateColumn);

                // Category column
                DataGridTextColumn categoryColumn = new();
                categoryColumn.Header = "Category";
                categoryColumn.Binding = new Binding("Category");
                dgExpenses.Columns.Add(categoryColumn);

                // Description column
                DataGridTextColumn descriptionColumn = new();
                descriptionColumn.Header = "Description";
                descriptionColumn.Binding = new Binding("ShortDescription");
                dgExpenses.Columns.Add(descriptionColumn);

                // Amount column
                DataGridTextColumn amountColumn = new();
                amountColumn.Header = "Amount";
                amountColumn.Binding = new Binding("Amount");
                amountColumn.Binding.StringFormat = "C";
                amountColumn.CellStyle = rightAligned;
                dgExpenses.Columns.Add(amountColumn);

                // Balance column
                DataGridTextColumn balanceColumn = new();
                balanceColumn.Header = "Balance";
                balanceColumn.Binding = new Binding("Balance");
                balanceColumn.Binding.StringFormat = "C";
                balanceColumn.CellStyle = rightAligned;
                dgExpenses.Columns.Add(balanceColumn);
            }
            else if (items is List<BudgetItemsByCategory>)
            {
                List<BudgetItemsByCategory> budgetItems = (List<BudgetItemsByCategory>)items;
                dgExpenses.ItemsSource = budgetItems;

                // Category column
                DataGridTextColumn categoryColumn = new();
                categoryColumn.Header = "Category";
                categoryColumn.Binding = new Binding("Category");
                dgExpenses.Columns.Add(categoryColumn);

                // Total column
                DataGridTextColumn totalColumn = new();
                totalColumn.Header = "Total";
                totalColumn.Binding = new Binding("Total");
                totalColumn.Binding.StringFormat = "C";
                totalColumn.CellStyle = rightAligned;
                dgExpenses.Columns.Add(totalColumn);
            }
            else if (items is List<BudgetItemsByMonth>)
            {
                List<BudgetItemsByMonth> budgetItems = (List<BudgetItemsByMonth>)items;
                dgExpenses.ItemsSource = budgetItems;

                // Month column
                DataGridTextColumn monthColumn = new();
                monthColumn.Header = "Month";
                monthColumn.Binding = new Binding("Month");
                dgExpenses.Columns.Add(monthColumn);

                // Total column
                DataGridTextColumn totalColumn = new();
                totalColumn.Header = "Total";
                totalColumn.Binding = new Binding("Total");
                totalColumn.Binding.StringFormat = "C";
                totalColumn.CellStyle = rightAligned;
                dgExpenses.Columns.Add(totalColumn);
            }
            else if (items is List<Dictionary<string, object>>)
            {
                List<Dictionary<string, object>> budgetItems = (List<Dictionary<string, object>>)items;
                dgExpenses.ItemsSource = budgetItems;

                // Month column
                DataGridTextColumn monthColumn = new();
                monthColumn.Header = "Month";
                monthColumn.Binding = new Binding("[Month]");
                dgExpenses.Columns.Add(monthColumn);

                // Keep track of category columns that have already been added in a map.
                Dictionary<string, bool> categories = new();

                // Foreach month
                foreach (Dictionary<string, object> monthlyBudget in budgetItems)
                {
                    // Foreach category summary for the current month
                    foreach (KeyValuePair<string, object> entry in monthlyBudget)
                    {
                        // Skip Month, Total, details and duplicate category keys.
                        if (entry.Key == "Month" || entry.Key == "Total" || entry.Key.StartsWith("details:")
                            || categories.TryGetValue(entry.Key, out _))
                            continue;

                        // Add the category column
                        DataGridTextColumn categoryColumn = new();
                        categoryColumn.Header = entry.Key;
                        categoryColumn.Binding = new Binding($"[{entry.Key}]");
                        categoryColumn.Binding.StringFormat = "C";
                        categoryColumn.CellStyle = rightAligned;
                        dgExpenses.Columns.Add(categoryColumn);

                        // Add to the map to prevent duplicates.
                        categories.Add(entry.Key, true);
                    }
                }

                // Add the totals column.
                DataGridTextColumn totalColumn = new();
                totalColumn.Header = "Total";
                totalColumn.Binding = new Binding("[Total]");
                totalColumn.Binding.StringFormat = "C";
                totalColumn.CellStyle = rightAligned;
                dgExpenses.Columns.Add(totalColumn);
            }
            else
            {
                // This should never happen, but here is an error:
                ShowError("Fatal error occurred when attempting to update expenses view.");
            }
        }

        private void Menu_OpenRecent_Click(object sender, RoutedEventArgs e)
        {
            _presenter.ConnectToDatabase(_presenter.GetRecentFile(), false);
            _filename = _presenter.GetRecentFile();
        }

        private void Menu_OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenExistingFile();
        }

        private void Menu_NewFile_Click(object sender, RoutedEventArgs e)
        {
            OpenNewFile();
        }

        public void OpenExistingFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Database File | *.db";

            if (openFileDialog.ShowDialog() == true)
            {
                _filename = openFileDialog.FileName;
                _presenter.ConnectToDatabase(_filename, false);
            }
        }

        public void OpenNewFile()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Database File | *.db";

            if (saveFileDialog.ShowDialog() == true)
            {
                _filename = saveFileDialog.FileName;
                _presenter.ConnectToDatabase(_filename, true);
            }
        }

        private void btn_AddExpense_Click(object sender, RoutedEventArgs e)
        {
            _addOrUpdateExpense = new AddOrUpdateExpense();
            _presenter.ExpenseView = _addOrUpdateExpense;
            _addOrUpdateExpense.SetAddOrUpdateView(AddOrUpdateExpense.Mode.Add, _presenter);
            _addOrUpdateExpense.ShowDialog();
        }

        private void miModify_Click(object sender, RoutedEventArgs e)
        {
            if (dgExpenses.SelectedValue is not null)
            {
                _addOrUpdateExpense = new AddOrUpdateExpense();
                _presenter.ExpenseView = _addOrUpdateExpense;
                _addOrUpdateExpense.SetAddOrUpdateView(AddOrUpdateExpense.Mode.Update, _presenter, (BudgetItem)dgExpenses.SelectedItem);
                _addOrUpdateExpense.ShowDialog();
            }
        }

        private void miDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgExpenses.SelectedValue is not null)
            {
                BudgetItem expense = (BudgetItem)dgExpenses.SelectedItem;
                _presenter.DeleteExpense(expense.ExpenseID, expense.ShortDescription);
            }
        }

        private void Menu_SaveAs_Click(object sender, RoutedEventArgs e)
        {

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Database File | *.db";

            string saveLocation = "";

            if(saveFileDialog.ShowDialog() == true)
            {
                saveLocation = saveFileDialog.FileName;
            }

            if (!string.IsNullOrEmpty(saveLocation))
            {
                System.IO.File.Copy(_filename, saveLocation, true);
            }
        }

        private void SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            Refresh();
        }

        private void CheckBox_Changed(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        private void cmbCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Refresh();
        }

        public void ShowCategories(List<Category> categories)
        {
            cmbCategories.DisplayMemberPath = "Description";
            cmbCategories.ItemsSource = categories;
        }

        private void dgExpenses_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgExpenses.SelectedValue is not null)
            {
                _addOrUpdateExpense = new AddOrUpdateExpense();
                _presenter.ExpenseView = _addOrUpdateExpense;
                _addOrUpdateExpense.SetAddOrUpdateView(AddOrUpdateExpense.Mode.Update, _presenter, (BudgetItem)dgExpenses.SelectedItem);
                _addOrUpdateExpense.ShowDialog();
            }
        }

        public bool ShowMessageWithConfirmation(string message)
        {
            return MessageBox.Show(message, "Info", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes;
        }

        private int lastIndex = 0;
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if(dgExpenses.ItemsSource.GetType() == typeof(List<BudgetItem>))
            {
                List<BudgetItem> budgetItems = (List<BudgetItem>)dgExpenses.ItemsSource;
                lastIndex %= budgetItems.Count;
                bool allowIndexReset = true;
                for(int i = lastIndex; i < budgetItems.Count; i++)
                {
                    BudgetItem item = budgetItems[i];
                    // TODO: Skip ids
                    foreach(var prop in item.GetType().GetProperties())
                    {
                        if(prop.GetValue(item).ToString().Contains(txbSearch.Text))
                        {
                            dgExpenses.SelectedItem = item;
                            lastIndex++;

                            return;
                        }
                    }
                    if (i == budgetItems.Count - 1 && allowIndexReset)
                    {
                        i = -1;
                        allowIndexReset = false;
                    }
                    lastIndex++;
                }
            }
        }

        private void Menu_Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void txbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            lastIndex = 0;
        }
    }
}
