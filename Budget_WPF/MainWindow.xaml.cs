﻿using System;
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
        public MainWindow()
        {
            InitializeComponent();
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
    }
}
