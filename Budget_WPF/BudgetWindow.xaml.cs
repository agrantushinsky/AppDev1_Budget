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
            throw new NotImplementedException();
        }
    }
}
