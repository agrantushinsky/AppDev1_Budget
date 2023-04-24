using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Budget_WPF
{
    public interface IBudgetView
    {
        public void UpdateView(object items);

        void ShowCurrentFile(string filename);

        void ShowError(string message);
        void Refresh();
        /// <summary>
        /// Opens the file dialog to create a new database file for Homebudget program
        /// </summary>
        void OpenNewFile();
        /// <summary>
        /// Opens the file dialog for users to search and open their existing database file
        /// </summary>
        void OpenExistingFile();

    }
}
