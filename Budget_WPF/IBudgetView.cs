using Budget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Budget_WPF
{
    public interface IBudgetView
    {
        /// <summary>
        /// Sets up the budget item datagrid 
        /// </summary>
        /// <param name="items">List of items to display</param>
        public void UpdateView(object items);
        /// <summary>
        /// Displays current file opened
        /// </summary>
        /// <param name="filename">File path to current file</param>
        void ShowCurrentFile(string filename);
        /// <summary>
        ///  Displays error messages
        /// </summary>
        /// <param name="message">Error message</param>
        void ShowError(string message);
        /// <summary>
        /// Updates budget item datagrid whenever a filter change or an add/update/delete operation occurs
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
        /// Displys the list of categories in the budget view
        /// </summary>
        /// <param name="categories">List of categories</param>
        void ShowCategories(List<Category> categories);
        /// <summary>
        /// Displays message box with a yes/no
        /// </summary>
        /// <param name="message">Confirmation message</param>
        /// <returns>True if user answered yes. False otherwise.</returns>
        bool ShowMessageWithConfirmation(string message);

    }
}
