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
using Budget;

namespace Budget_WPF
{
    /// <summary>
    /// Interaction logic for NewCategory.xaml
    /// </summary>
    public partial class NewCategory : Window
    {
        private Presenter _presenter;
        public NewCategory(Presenter presenter, string description)
        {
            _presenter = presenter;
            InitializeComponent();
            viewRefresh(description);
        }

        private void viewRefresh(string description)
        {
            Category.CategoryType[] types = (Category.CategoryType[])Enum.GetValues(typeof(Category.CategoryType));

            cmbCatType.ItemsSource = types;
            txtboxCatDesc.Text = description;

        }

        private void btnAddCat_Click(object sender, RoutedEventArgs e)
        {
            string description = txtboxCatDesc.Text;
            Category.CategoryType? type = cmbCatType.SelectedItem as Category.CategoryType?;
            _presenter.AddCategory(description, type);
            this.Close();
        }
    }
}
