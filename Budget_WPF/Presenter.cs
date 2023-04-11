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

        public void AddCategory(string description, Category.CategoryType? type)
        {
            if (string.IsNullOrEmpty(description))
                _view.ShowError("Invalid description, please try again.");
            else if (type is null)
                _view.ShowError("Please select a type.");
            else
            {
                try
                {
                    //Change type to non-nullable
                    _model.categories.Add(description, (Category.CategoryType)type);
                }
                catch (Exception ex)
                {

                }
            }
        }

        public void AddExpense(DateTime date, int category, string amountStr, string description)
        {
            string errMsg = string.Empty;
            double amount;

            if (category == -1)
            {
                errMsg += "An existing catgory must be selected.\n";
            }

            if (string.IsNullOrEmpty(description))
            {
                errMsg += "A description must be added.\n";
            }

            if(!double.TryParse(amountStr, out amount))
            {
                errMsg += "The amount is invalid.\n";
            }
        
            if(!string.IsNullOrEmpty(errMsg))
            {
                _view.ShowError(errMsg);
            }
            else
            {
                try
                {
                    _model.expenses.Add(date, category, amount, description);
                }
                catch (Exception ex)
                {

                }
                _view.ClearInputs();
            }
        }

        public bool UnsavedChangesCheck(string description, string amount)
        {
            if(!string.IsNullOrEmpty(description) ||
                !string.IsNullOrEmpty(amount))
            {
                return _view.ShowMessageWithConfirmation("You have unsaved changes, are you sure you wanted to exit?");
            }

            return true;
        }

        public List<Category> GetCategories()
        {
            return _model.categories.List();
        }
    }
}
