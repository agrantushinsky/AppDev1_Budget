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
using System.Windows;

namespace Budget_WPF
{
    public class Presenter
    {
        private ViewInterface _view;
        private HomeBudget _model;

        const string SOFTWATRE_ROOT = "HKEY_CURRENT_USER\\SOFTWARE";
        const string SUB_KEY = "AppDevBudget";
        const string KEY_NAME = SOFTWATRE_ROOT + "\\" + SUB_KEY;
        const string RECENT_FILE_VALUE = "recentFile";

        private bool _isConnected = false;
        private int _creditCardCategoryId;

        /// <summary>
        /// Creates a Presenter object and saves the view object
        /// </summary>
        /// <param name="view">Object that represents the UI</param>
        public Presenter(ViewInterface view)
        {
            _view = view;
        }

        /// <summary>
        /// Opens a connection to the database using the provided filename and sets up the UI. 
        /// An error message is shown if the filename is invalid.
        /// </summary>
        /// <param name="filename">Path to the budget database file.</param>
        /// <param name="newDatabase">If a new database will be created.</param>
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
            _creditCardCategoryId = credit is null? -1 : credit.Id;
        }

        /// <summary>
        /// Adds the new category to the database. 
        /// If the description or type is invalid, an error message will be shown.
        /// </summary>
        /// <param name="description">Category description</param>
        /// <param name="type">Category type</param>
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

        /// <summary>
        /// Adds the new expense to the database. If any of the arguments is invalid, an error message will be shown. 
        /// If credit is used to pay, an additonal expense is created. 
        /// </summary>
        /// <param name="date">Date of the transaction</param>
        /// <param name="category">Category ID</param>
        /// <param name="amountStr">Expense amount</param>
        /// <param name="description">Short description</param>
        /// <param name="credit">If credit was used to pay</param>
        public void AddExpense(DateTime date, int category, string amountStr, string description, bool credit)
        {
            StringBuilder errorMessage = new();
            double amount;

            if (!_isConnected)
            {
                errorMessage.AppendLine("No file is currently opened.");
            }

            if (_isConnected && (category <= 0 || category > GetCategories().Count))
            {
                errorMessage.AppendLine("An existing category must be selected.");
            }

            if (string.IsNullOrEmpty(description))
            {
                errorMessage.AppendLine("A description must be added.");
            }

            if(!double.TryParse(amountStr, out amount))
            {
                errorMessage.AppendLine("The amount is invalid.");
            }


            // If an error occurred, show the error and return this method.
            if (errorMessage.Length > 0)
            {
                _view.ShowError(errorMessage.ToString());
                return;
            }

            // Attempt to add the expense
            try
            {
                _model.expenses.Add(date, category, amount, description);

                if (credit)
                    _model.expenses.Add(date, _creditCardCategoryId, -amount, $"{description} (on credit)");

                _view.SetLastAction($"Successfully added expense: {description}");

            }
            catch (Exception ex)
            {

            }
            _view.ClearInputs();
        }

        /// <summary>
        /// Checks if there are any unsaved changes.
        /// </summary>
        /// <param name="description">Content of the description field</param>
        /// <param name="amount">Content of the amount field</param>
        /// <returns>True if description and amount are empty. Otherwise, shows pop up with confirmation message to exit</returns>
        public bool UnsavedChangesCheck(string description, string amount)
        {
            if(!string.IsNullOrEmpty(description) ||
                !string.IsNullOrEmpty(amount))
            {
                return _view.ShowMessageWithConfirmation("You have unsaved changes, are you sure you wanted to exit?");
            }

            return true;
        }

        /// <summary>
        /// Gets the list of categories
        /// </summary>
        /// <returns>List of categories</returns>
        public List<Category> GetCategories()
        {
            return _model.categories.List();
        }

        /// <summary>
        /// Gets the path of recent file opened
        /// </summary>
        /// <returns>Path to recent file</returns>
        public string? GetRecentFile()
        {
            return (string?)Registry.GetValue(KEY_NAME, RECENT_FILE_VALUE, "");
        }

        /// <summary>
        /// Sets the recent file
        /// </summary>
        /// <param name="newRecent">File path to recent file</param>
        public void SetRecentFile(string newRecent)
        {
            Registry.SetValue(KEY_NAME, RECENT_FILE_VALUE, newRecent);
        }

        /// <summary>
        /// Checks if there is a file opened
        /// </summary>
        /// <returns>True if there is a file. False otherwise.</returns>
        public bool IsFileSelected()
        {
            if (!_isConnected)
            {
                _view.ShowError("Please select a file before continuing.");
            }
            return _isConnected;
        }

        /// <summary>
        /// Shows welcome message for first time users
        /// </summary>
        public void ShowFirstTimeUserSetup()
        {
            if(string.IsNullOrEmpty(GetRecentFile()))
                if (_view.ShowMessageWithConfirmation("Welcome first time user, would you like to browse to create a new budget?"))
                    _view.OpenNewFile();
        }
    }
}
