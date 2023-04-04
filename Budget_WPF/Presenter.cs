using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Budget;
using System.Data.SQLite;
using System.Data.Entity.Infrastructure;

namespace Budget_WPF
{
    public class Presenter
    {
        private ViewInterface _view;
        private HomeBudget _model;

        public Presenter(ViewInterface view)
        {
            _view = view;
        }

        public void ConnectToDatabase(string filename, bool newDatabase)
        {
            _model = new HomeBudget(filename, newDatabase);
            _view.ShowCurrentFile(filename);
            _view.Refresh();
        }

        public void AddCategory(string description, Category.CategoryType type)
        {
            try
            {
                _model.categories.Add(description, type);
            }
            catch(Exception ex)
            {

            }
        }

        public void AddExpense(DateTime date, int category, double amount, string description)
        {
            try
            {
                _model.expenses.Add(date, category, amount, description);
            }
            catch(Exception ex)
            {

            }
        }

        public List<Category> GetCategories()
        {
            return _model.categories.List();
        }
    }
}
