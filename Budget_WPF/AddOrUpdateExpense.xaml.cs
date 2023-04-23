using Budget;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
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

        public enum Mode
        {
            Add,
            Update
        }

        public AddOrUpdateExpense(Mode mode)
        {
            InitializeComponent();

            currentMode = mode;

            if(currentMode == Mode.Add)
            {
                dp_Date.SelectedDate = DateTime.Now;
                txb_Title.Text = "Add Expense";
                btn_CloseOrDelete.Content = "Close";
            }
            else if(currentMode == Mode.Update)
            {
                txb_Title.Text = "Update Expense";
                btn_CloseOrDelete.Content = "Delete";
                //save selected object 
            }

        }

        private void Menu_NewFile_Click(object sender, RoutedEventArgs e)
        {
            OpenNewFile();
        }

        private void Menu_OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenExistingFile();
        }

        private void btn_Save_Click(object sender, RoutedEventArgs e)
        {
            SaveExpense();
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

        public void ShowCurrentFile(string filename)
        {
            txb_CurrentFile.Text = filename;
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
            else
            {
                //get selected item's id
                //call presenter update expense

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

        private void Menu_OpenRecent_Click(object sender, RoutedEventArgs e)
        {
            _presenter.ConnectToDatabase(_presenter.GetRecentFile(), false);
        }
        public void SetLastAction(string message)
        {
            txb_LastAction.Text = $"[{DateTime.Now.ToShortTimeString()}] {message}";
        }

        private void btn_Discard_Click(object sender, RoutedEventArgs e)
        {
            ClearInputs();
        }

        private void btn_CloseOrDelete_Click(object sender, RoutedEventArgs e)
        {
            if (currentMode == Mode.Add)
            {
                //does this trigger the unsaved changes function?
                this.Close();
            }
            else if (currentMode == Mode.Update)
            {
                //Get id of selected item
                //call delete expense from presenter
            }
        }
    }
}
