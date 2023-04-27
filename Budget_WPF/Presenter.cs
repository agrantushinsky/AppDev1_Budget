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
        private IExpenseView _expenseView;
        private IBudgetView _budgetView;
        private HomeBudget _model;


        const string SOFTWATRE_ROOT = "HKEY_CURRENT_USER\\SOFTWARE";
        const string SUB_KEY = "AppDevBudget";
        const string KEY_NAME = SOFTWATRE_ROOT + "\\" + SUB_KEY;
        const string RECENT_FILE_VALUE = "recentFile";

        private bool _isConnected = false;
        private int _creditCardCategoryId;

        /// <summary>
        /// Creates a Presenter object and saves the budget view
        /// </summary>
        /// <param name="budgetView">Object that represents the budget UI</param>
        public Presenter(IBudgetView budgetView)
        {
            _budgetView = budgetView;
        }

        /// <summary>
        /// Sets the expense view
        /// </summary>
        public IExpenseView expenseView
        {
            set { _expenseView = value; }
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
                _budgetView.ShowError("You must open a file before you may use Open Recent");
                return;
            }

            _model = new HomeBudget(filename, newDatabase);
            _isConnected = true;

            // Set up the UI
            _budgetView.ShowCurrentFile(filename);
            SetRecentFile(filename);

            // Find the credit card category id
            List<Category> categories = _model.categories.List();
            Category? credit = categories.Find((category) => category.Type == Category.CategoryType.Credit);
            _creditCardCategoryId = credit is null? -1 : credit.Id;

            _budgetView.ShowCategories(categories);

            _budgetView.Refresh();
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
                _expenseView.ShowError("Invalid description, please try again.");
            else if (type is null)
                _expenseView.ShowError("Please select a type.");
            else
            {
                try
                {
                    //Change type to non-nullable
                    _model.categories.Add(description, (Category.CategoryType)type);
                    _expenseView.SetLastAction($"Successfully added category: {description}");
                    _budgetView.ShowCategories(_model.categories.List());

                }
                catch (Exception ex)
                {

                }
            }
        }

        /// <summary>
        /// Adds the new expense to the database. If any of the user inputs is invalid, error messsage will be shown
        /// If credit is used to pay, an additonal expense is created. Once saved, the datagrid is refreshed.
        /// </summary>
        /// <param name="date">Date of the transaction</param>
        /// <param name="category">Category ID</param>
        /// <param name="amountStr">Expense amount</param>
        /// <param name="description">Short description</param>
        /// <param name="credit">If credit was used to pay</param>
        public void AddExpense(DateTime date, int category, string amountStr, string description, bool credit)
        {

            // Attempt to add the expense
            try
            {
                if (ValidateUserInput(date, category, amountStr, description))
                {
                    double amount = double.Parse(amountStr);

                    _model.expenses.Add(date, category, amount, description);

                    if (credit)
                        _model.expenses.Add(date, _creditCardCategoryId, -amount, $"{description} (on credit)");

                    _expenseView.SetLastAction($"Successfully added expense: {description}");
                }
                else
                    return;
            }
            catch (Exception ex)
            {

            }
            _expenseView.ClearInputs();
            _budgetView.Refresh();
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
                return _expenseView.ShowMessageWithConfirmation("You have unsaved changes, are you sure you wanted to exit?");
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
                _expenseView.ShowError("Please select a file before continuing.");
            }
            return _isConnected;
        }

        /// <summary>
        /// Shows welcome message for first time users
        /// </summary>
        public void ShowFirstTimeUserSetup()
        {
            if (string.IsNullOrEmpty(GetRecentFile()))
                if (_budgetView.ShowMessageWithConfirmation("Welcome first time user, would you like to browse to create a new budget?"))
                    _budgetView.OpenNewFile();
        }

        /// <summary>
        /// Gets the list of budget items according to the filters and updates the budget view
        /// </summary>
        /// <param name="start">Start date</param>
        /// <param name="end">End date</param>
        /// <param name="categoryId">The ID of the category to filter by, if filter flag is true</param>
        /// <param name="shouldFilterCategory">Flag to indicate whether to filter by category ID or not</param>
        /// <param name="groupByMonth">Flag to indicate whether to group by month</param>
        /// <param name="groupByCategory">Flag to indicate whether to group by category</param>
        public void FiltersChange(DateTime? start, DateTime? end, int categoryId, bool shouldFilterCategory, bool groupByMonth, bool groupByCategory)
        {
            // Don't do anything if the model has not been initialize (no file open)
            if (_model is null)
                return;

            object items;
            if(groupByCategory && groupByMonth)
            {
                items = _model.GetBudgetDictionaryByCategoryAndMonth(start, end, shouldFilterCategory, categoryId);
            }
            else if(groupByCategory)
            {
                items = _model.GetBudgetItemsByCategory(start, end, shouldFilterCategory, categoryId);
            }
            else if(groupByMonth)
            {
                items = _model.GetBudgetItemsByMonth(start, end, shouldFilterCategory, categoryId);
            }
            else 
            {
                items = _model.GetBudgetItems(start, end, shouldFilterCategory, categoryId);
            }

            _budgetView.UpdateView(items);
        }

        /// <summary>
        /// Gets the list of Expenses
        /// </summary>
        /// <returns>List of expenses</returns>
        public List<Expense> GetExpenses()
        {
            return _model.expenses.List();
        }

        /// <summary>
        /// Updates the selected expense item. If any of the user inputs is invalid, error messsage will be shown.
        /// Once saved, the datagrid is refreshed.
        /// </summary>
        /// <param name="expId">Expense ID</param>
        /// <param name="date">Transaction date</param>
        /// <param name="category">Category ID</param>
        /// <param name="amountStr">Expense amount</param>
        /// <param name="description">Short description</param>
        public void UpdateExpense(int expId, DateTime date, int category, string amountStr, string description)
        {
            try
            {
                if (ValidateUserInput(date, category, amountStr, description))
                {
                    double amount = double.Parse(amountStr);

                    _model.expenses.Update(expId, date, category, amount, description);

                    _expenseView.SetLastAction($"Successfully updated expense: {description}");

                }
                else
                    return;
            }

            catch (Exception ex)
            {

            }

            _expenseView.ClearInputs();
            _budgetView.Refresh();
        }

        /// <summary>
        /// Deletes the selected expense item. Once done, the datagrid is refreshed.
        /// </summary>
        /// <param name="expId">Expense ID</param>
        /// <param name="description">Short description</param>
        public void DeleteExpense(int expId, string description)
        {
            try
            {
                _model.expenses.Delete(expId);

                _expenseView.SetLastAction($"Successfully deleted expense: {description}");
            }

            catch (Exception ex)
            {

            }
            _expenseView.ClearInputs();
            _budgetView.Refresh();
        }

        /// <summary>
        /// Validates user input. Expense date, category ID, amount and description must not be null.
        /// </summary>
        /// <param name="date">Transaction date</param>
        /// <param name="category">Category ID</param>
        /// <param name="amountStr">Expense amount</param>
        /// <param name="description">Short description</param>
        /// <returns>True if all the arguments are valid. False otherwise.</returns>
        private bool ValidateUserInput(DateTime date, int category, string amountStr, string description)
        {
            StringBuilder errorMessage = new();
            double amount;

            if (!_isConnected)
            {
                errorMessage.AppendLine("No file is currently opened.");
            }


            if (category < 0)
            {
                errorMessage.AppendLine("An existing category must be selected.");
            }
            else
            {
                try
                {
                    // Throws when the ID is not found.
                    _ = _model.categories.GetCategoryFromId(category);
                }
                catch
                {
                    errorMessage.AppendLine("An existing category must be selected.");
                }
            }

            if (string.IsNullOrEmpty(description))
            {
                errorMessage.AppendLine("A description must be added.");
            }

            if (!double.TryParse(amountStr, out amount))
            {
                errorMessage.AppendLine("The amount is invalid.");
            }

            // If an error occurred, show the error and return this method.
            if (errorMessage.Length > 0)
            {
                _expenseView.ShowError(errorMessage.ToString());
                return false;
            }

            return true;
        }
    }
}
