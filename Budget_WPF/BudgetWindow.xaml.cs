using Budget;
using System;
using System.CodeDom;
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
using System.Windows.Shapes;

namespace Budget_WPF
{
    /// <summary>
    /// Interaction logic for BudgetWindow.xaml
    /// </summary>
    public partial class BudgetWindow : Window, IBudgetView
    {
        private Presenter _presenter;
        private string _filename;

        public BudgetWindow()
        {
            InitializeComponent();
            AddOrUpdateExpense addOrUpdateExpense = new AddOrUpdateExpense();
            _presenter = new Presenter(this, addOrUpdateExpense);

        }

        public void Refresh()
        {
            throw new NotImplementedException();
        }

        public void ShowCurrentFile(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                txtb_currentFile.Text = "Budget File: None";
            }
            else
            {
                txtb_currentFile.Text = $"Budget File {filename}";
            }
        }

        public void ShowError(string message)
        {
            throw new NotImplementedException();
        }

        public void UpdateView(object items)
        {
            dgExpenses.Columns.Clear();

            Style rightAligned = new Style();
            rightAligned.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Right));

            // TODO: The ordering of by category is off

            if (items is List<BudgetItem>)
            {
                List<BudgetItem> budgetItems = (List<BudgetItem>)items;
                dgExpenses.ItemsSource = budgetItems;

                DataGridTextColumn dateColumn = new();
                dateColumn.Header = "Date";
                dateColumn.Binding = new Binding("Date");
                dateColumn.Binding.StringFormat = "dd/MM/yyyy";
                dgExpenses.Columns.Add(dateColumn);

                DataGridTextColumn categoryColumn = new();
                categoryColumn.Header = "Category";
                categoryColumn.Binding = new Binding("Category");
                dgExpenses.Columns.Add(categoryColumn);

                DataGridTextColumn descriptionColumn = new();
                descriptionColumn.Header = "Description";
                descriptionColumn.Binding = new Binding("ShortDescription");
                dgExpenses.Columns.Add(descriptionColumn);

                DataGridTextColumn amountColumn = new();
                amountColumn.Header = "Amount";
                amountColumn.Binding = new Binding("Amount");
                amountColumn.Binding.StringFormat = "C";
                amountColumn.CellStyle = rightAligned;
                dgExpenses.Columns.Add(amountColumn);

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

                DataGridTextColumn categoryColumn = new();
                categoryColumn.Header = "Category";
                categoryColumn.Binding = new Binding("Category");
                dgExpenses.Columns.Add(categoryColumn);

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

                DataGridTextColumn monthColumn = new();
                monthColumn.Header = "Month";
                monthColumn.Binding = new Binding("Month");
                dgExpenses.Columns.Add(monthColumn);

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

                DataGridTextColumn monthColumn = new();
                monthColumn.Header = "Month";
                monthColumn.Binding = new Binding("[Month]");
                dgExpenses.Columns.Add(monthColumn);

                Dictionary<string, bool> categories = new();

                foreach (Dictionary<string, object> monthlyBudget in budgetItems)
                {
                    foreach (KeyValuePair<string, object> entry in monthlyBudget)
                    {
                        bool outValue;
                        if (entry.Key == "Month" || entry.Key == "Total" || entry.Key.StartsWith("details:")
                            || categories.TryGetValue(entry.Key, out outValue))
                            continue;

                        DataGridTextColumn categoryColumn = new();
                        categoryColumn.Header = entry.Key;
                        categoryColumn.Binding = new Binding($"[{entry.Key}]");
                        categoryColumn.Binding.StringFormat = "C";
                        categoryColumn.CellStyle = rightAligned;
                        dgExpenses.Columns.Add(categoryColumn);

                        categories.Add(entry.Key, true);
                    }
                }


                DataGridTextColumn totalColumn = new();
                totalColumn.Header = "Total";
                totalColumn.Binding = new Binding("[Total]");
                totalColumn.Binding.StringFormat = "C";
                totalColumn.CellStyle = rightAligned;
                dgExpenses.Columns.Add(totalColumn);
            }
            else
            {
                // TODO: Show error?
            }
        }
    }
}
