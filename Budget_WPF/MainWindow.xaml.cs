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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, ViewInterface
    {
        private Presenter _presenter;
        private string _filename;

        public MainWindow()
        {
            InitializeComponent();
            _presenter = new Presenter(this);
            dp_Date.SelectedDate = DateTime.Now;
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
            AddExpense();
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
            txb_LastAction.Text = $"Opened {filename}";
            txb_CurrentFile.Text = filename;
        }

        public void AddCategory()
        {
            NewCategory newCat = new NewCategory(_presenter,cmbCategories.Text);
            newCat.ShowDialog();
            Refresh();

            //Set the selected index to the newly made category
            cmbCategories.SelectedIndex = cmbCategories.Items.Count - 1;
        }

        public void AddExpense()
        {
            // TODO: make sure a category is selected
            string errMsg = string.Empty;
            DateTime date = dp_Date.SelectedDate ?? new DateTime();
            string desc = string.Empty;
            double amount;
            int catID;

            if(txb_CurrentFile.Text.ToLower() == "none" || txb_CurrentFile.Text == "")
            {
                ShowError("No file is currently opened.");
                return;
            }

            if(cmbCategories.SelectedItem == null && cmbCategories.Text.Length == 0)
            {
                errMsg += "A category must be selected.\n";
            }
            else if(cmbCategories.SelectedItem == null)
            {
                MessageBoxResult result = MessageBox.Show($"Category \"{cmbCategories.Text}\" does not exist. Would you like to create a new category?", "Info", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (result == MessageBoxResult.Yes)
                    AddCategory();
                else
                    errMsg += "Select an existing";
            }

            if(string.IsNullOrEmpty(tbx_Description.Text))
            {
                errMsg += "A description must be added\n";
            }

            if (!(double.TryParse(tbx_Amount.Text, out amount)))
            {
                errMsg += "The amount is invalid.\n";
            }

            if(string.IsNullOrEmpty(errMsg))
            {
                catID = (cmbCategories.SelectedItem as Category).Id;
                _presenter.AddExpense(date, catID, amount, desc);
            }
            else
            {
                ShowError(errMsg);
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
            AddCategory();
        }

        public void ClearInputs()
        {
            dp_Date.SelectedDate = DateTime.Now;
            tbx_Amount.Text = "";
            tbx_Description.Text = "";
            // TODO: Probably keep the current category selected.
            //cmbCategories.SelectedIndex = -1;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !_presenter.UnsavedChangesCheck(tbx_Description.Text, tbx_Amount.Text);
        }

        public bool ShowMessageWithConfirmation(string message)
        {
            return MessageBox.Show(message, "Info", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes;
        }
    }
}
