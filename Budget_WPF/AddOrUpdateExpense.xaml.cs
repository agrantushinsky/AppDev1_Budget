using Budget;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Security.Permissions;
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

namespace Budget_WPF
{
    /// <summary>
    /// Interaction logic for AddOrUpdateExpense.xaml
    /// </summary>
    public partial class AddOrUpdateExpense : Window, IExpenseView
    {
        private Presenter _presenter;
        private string _filename;
        private Mode currentMode;
        private Expense currentExpenseItem;

        public enum Mode
        {
            Add,
            Update
        }

        public AddOrUpdateExpense()
        {
            InitializeComponent();
        }

        private void btn_Save_Click(object sender, RoutedEventArgs e)
        {
            SaveExpense();
        }


        public void AddCategory()
        {
            if (!_presenter.IsFileSelected())
                return;

            NewCategory newCat = new NewCategory(_presenter,cmbCategories.Text);
            newCat.ShowDialog();
            Refresh();

            // Set the selected index to the newly made category
            if(newCat.Success)
                cmbCategories.SelectedIndex = cmbCategories.Items.Count - 1;
        }

        public void SaveExpense()
        {
            if (!_presenter.IsFileSelected())
                return;

            // Impossible to be null, but SelectedDate is nullabe.
            DateTime date = dp_Date.SelectedDate ?? DateTime.Now;
            string desc = tbx_Description.Text;
            string amount = tbx_Amount.Text;

            if (!string.IsNullOrEmpty(cmbCategories.Text) &&
                cmbCategories.SelectedIndex == -1)
            {
                if(ShowMessageWithConfirmation($"Category \"{cmbCategories.Text}\",does not exist. Would you like to create a new category?"))
                    AddCategory();
            }

            Category? selectedCat = cmbCategories.SelectedValue as Category;
            int catID = (selectedCat) is null ? -1 : selectedCat.Id;

            if(currentMode == Mode.Add)
                _presenter.AddExpense(date, catID, amount, desc, cbCredit.IsChecked == true);
            else if (currentMode == Mode.Update)
                _presenter.UpdateExpense(currentExpenseItem.Id, date, catID, amount, desc);

            if (currentMode == Mode.Update)
            {
                this.Close();
            }

        }

        public void Refresh()
        {
            cmbCategories.DisplayMemberPath = "Description";
            cmbCategories.ItemsSource = _presenter.GetCategories();
        }
        public void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }        

        public void ShowWarning(string message)
        {
            MessageBox.Show(message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }


        private void btnAddCategory_Click(object sender, RoutedEventArgs e)
        {
            if(_presenter.IsFileSelected())    
                AddCategory();
        }

        public void ClearInputs()
        {
            tbx_Amount.Text = "";
            tbx_Description.Text = "";
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !_presenter.UnsavedChangesCheck(tbx_Description.Text, tbx_Amount.Text);
        }

        public bool ShowMessageWithConfirmation(string message)
        {
            return MessageBox.Show(message, "Info", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes;
        }

        public void SetLastAction(string message)
        {
            txb_LastAction.Text = $"[{DateTime.Now.ToShortTimeString()}] {message}";
        }

        private void btn_DiscardOrClose_Click(object sender, RoutedEventArgs e)
        {
            ClearInputs();

            if(currentMode == Mode.Update)
            {
                this.Close();
            }
        }

        private void btn_CloseOrDelete_Click(object sender, RoutedEventArgs e)
        {
            if (currentMode == Mode.Add)
            {
                this.Close();
            }
            else if (currentMode == Mode.Update)
            {
                _presenter.DeleteExpense(currentExpenseItem.Id, currentExpenseItem.Description);
            }
        }

        public void SetAddOrUpdateView(Mode mode, Presenter presenter, BudgetItem budgetItem = null)
        {
            _presenter = presenter;
            currentMode = mode;
            Refresh();
            ClearInputs();
            
            cbCredit.IsEnabled = mode == Mode.Add;

            if (currentMode == Mode.Add)
            {
                dp_Date.SelectedDate = DateTime.Now;
                txb_Title.Text = "Add Expense";
                btn_CloseOrDelete.Content = "Close";
                btn_DiscardOrCancel.Content = "Discard";
            }
            else if (currentMode == Mode.Update)
            {
                txb_Title.Text = "Update Expense";
                btn_CloseOrDelete.Content = "Delete";
                btn_DiscardOrCancel.Content = "Cancel";
                BudgetItem item = budgetItem;
                //Display info
                currentExpenseItem = _presenter.GetExpenses().Find(exp => exp.Id == item.ExpenseID);
                foreach(Category cat in cmbCategories.Items)
                {
                    if(cat.Id == item.CategoryID)
                    {
                        cmbCategories.SelectedValue = cat;
                        break;
                    }
                }
                dp_Date.SelectedDate = currentExpenseItem.Date;
                tbx_Description.Text = currentExpenseItem.Description;
                tbx_Amount.Text = currentExpenseItem.Amount.ToString();
            }
        }

    }
}
