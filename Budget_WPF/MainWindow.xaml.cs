using Microsoft.Win32;
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
            openFileDialog.Filter = "Data Base File | *.db";

            if (openFileDialog.ShowDialog() == true)
            {
                _filename = openFileDialog.FileName;
                _presenter.ConnectToDatabase(_filename, false);
            }
        }

        public void OpenNewFile()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Data Base File | *.db";

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
            throw new NotImplementedException();
        }

        public void AddExpense()
        {
            throw new NotImplementedException();
        }

        public void Refresh()
        {
            throw new NotImplementedException();
        }

        public void ShowCategoriesWindow()
        {
            throw new NotImplementedException();
        }

        public void ShowExpensesWindow()
        {
            throw new NotImplementedException();
        }

        public void ShowError()
        {
            throw new NotImplementedException();
        }
    }
}
