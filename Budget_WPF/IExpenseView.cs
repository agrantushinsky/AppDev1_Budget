using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Budget_WPF
{
    public interface IExpenseView
    {
        /// <summary>
        /// Creates a new Category from user input
        /// </summary>
        void AddCategory();
        /// <summary>
        /// Creates a new Expense from user input
        /// </summary>
        void SaveExpense();
        /// <summary>
        /// Sets list of categories in the combo box
        /// </summary>
        void Refresh();
        /// <summary>
        /// Opens the file dialog to create a new database file for Homebudget program
        /// </summary>
        void OpenNewFile();
        /// <summary>
        /// Opens the file dialog for users to search and open their existing database file
        /// </summary>
        void OpenExistingFile();
        /// <summary>
        /// Displays the current file opened
        /// </summary>
        /// <param name="filename">Path to the current file</param>
        void ShowCurrentFile(string filename);
        /// <summary>
        /// Displays error messages
        /// </summary>
        /// <param name="message">Error message</param>
        void ShowError(string message);
        /// <summary>
        /// Displays message box for confirming unsaved changes
        /// </summary>
        /// <param name="message">Confirmation message</param>
        /// <returns>True if user wants to save. False otherwise.</returns>
        bool ShowMessageWithConfirmation(string message);
        /// <summary>
        /// Clears the content in the description and amount fields
        /// </summary>
        void ClearInputs();
        /// <summary>
        /// Displays the last action
        /// </summary>
        /// <param name="message">Last action message</param>
        void SetLastAction(string message);
    }
}
