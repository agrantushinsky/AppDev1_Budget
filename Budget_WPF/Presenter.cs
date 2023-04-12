using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Budget;
using System.Data.SQLite;
using System.Data.Entity.Infrastructure;
using Microsoft.Win32;
using System.CodeDom;

namespace Budget_WPF
{
    public class Presenter
    {
        private ViewInterface _view;
        private HomeBudget _model;

        const string USER_ROOT = "HKEY_CURRENT_USER";
        const string SUB_KEY = "AppDevBudget";
        const string KEY_NAME = USER_ROOT + "\\" + SUB_KEY;
        const string RECENT_FILE_VALUE = "recentFile";

        private bool _isConnected = false;
        private int _creditCardCategoryId;

        public Presenter(ViewInterface view)
        {
            _view = view;
        }

        public void ConnectToDatabase(string? filename, bool newDatabase)
        {
            if (string.IsNullOrEmpty(filename))
            {
                _view.ShowError("You must open a file before you may use Open Recent");
                return;
            }

            _model = new HomeBudget(filename, newDatabase);
            _isConnected = true;
            _view.ShowCurrentFile(filename);
            _view.SetLastAction($"Opened {filename}");
            _view.Refresh();
            SetRecentFile(filename);

            // Find the credit card category id
            List<Category> categories = _model.categories.List();
            Category? credit = categories.Find((category) => category.Type == Category.CategoryType.Credit);
            _creditCardCategoryId = credit.Id;
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
                    _view.SetLastAction($"Successfully added category: {description}");

                }
                catch (Exception ex)
                {

                }
            }
        }

        public void AddExpense(DateTime date, int category, string amountStr, string description, bool credit)
        {
            string errMsg = string.Empty;
            double amount;

            if (!_isConnected)
            {
                errMsg+=("No file is currently opened.");
            }
            if (category == -1)
            {
                errMsg += "An existing category must be selected.\n";
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

                    if(credit)
                        _model.expenses.Add(date, _creditCardCategoryId, -amount, $"{description} (on credit)");

                    _view.SetLastAction($"Successfully added expense: {description}");

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

        public string? GetRecentFile()
        {
            return (string?)Registry.GetValue(KEY_NAME, RECENT_FILE_VALUE, "");
        }

        public void SetRecentFile(string newRecent)
        {
            Registry.SetValue(KEY_NAME, RECENT_FILE_VALUE, newRecent);
        }

        public bool IsFileSelected()
        {
            if (!_isConnected)
            {
                _view.ShowError("Please select a file before continuing.");
            }
            return _isConnected;
        }
    }
}
