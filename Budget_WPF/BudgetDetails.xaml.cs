using Budget;
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
using System.Windows.Shapes;

namespace Budget_WPF
{
    /// <summary>
    /// Interaction logic for BudgetDetails.xaml
    /// </summary>
    public partial class BudgetDetails : Window
    {
        public BudgetDetails(object items)
        {
            InitializeComponent();
            ShowBudgetDetails(items);
        }

        public void ShowBudgetDetails(object items)
        {
            dgDetails.Columns.Clear();

            Style rightAligned = new Style();
            rightAligned.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Right));

            if(items is BudgetItemsByMonth)
            {
                BudgetItemsByMonth budgetByMonth = (BudgetItemsByMonth)items;
                dgDetails.ItemsSource = budgetByMonth.Details;

            }
            else if (items is BudgetItemsByCategory) 
            { 
                BudgetItemsByCategory budgetByCategory = (BudgetItemsByCategory)items;
                dgDetails.ItemsSource = budgetByCategory.Details;
            }
            else{
                MessageBox.Show("Fatal error when trying to display budget details","Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Date column
            DataGridTextColumn dateColumn = new();
            dateColumn.Header = "Date";
            dateColumn.Binding = new Binding("Date");
            dateColumn.Binding.StringFormat = "dd/MM/yyyy";
            dgDetails.Columns.Add(dateColumn);

            // Category column
            DataGridTextColumn categoryColumn = new();
            categoryColumn.Header = "Category";
            categoryColumn.Binding = new Binding("Category");
            dgDetails.Columns.Add(categoryColumn);

            // Description column
            DataGridTextColumn descriptionColumn = new();
            descriptionColumn.Header = "Description";
            descriptionColumn.Binding = new Binding("ShortDescription");
            dgDetails.Columns.Add(descriptionColumn);

            // Amount column
            DataGridTextColumn amountColumn = new();
            amountColumn.Header = "Amount";
            amountColumn.Binding = new Binding("Amount");
            amountColumn.Binding.StringFormat = "C";
            amountColumn.CellStyle = rightAligned;
            dgDetails.Columns.Add(amountColumn);

            // Balance column
            DataGridTextColumn balanceColumn = new();
            balanceColumn.Header = "Balance";
            balanceColumn.Binding = new Binding("Balance");
            balanceColumn.Binding.StringFormat = "C";
            balanceColumn.CellStyle = rightAligned;
            dgDetails.Columns.Add(balanceColumn);
        }
    }
}
