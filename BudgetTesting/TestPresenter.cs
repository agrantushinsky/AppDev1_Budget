using Budget_WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetCodeTests
{
    public class TestView : ViewInterface
    {
        bool calledAddCategory = false;
        bool calledAddExpense = false;
        bool calledClearInputs = false;
        bool calledOpenExistingFile = false;
        bool calledCloseExistingFile = false;
        bool calledOpenNewFile = false;
        bool calledRefresh = false;
        bool calledShowCurrentFile = false;
        bool calledShowError = false;
        bool calledShowMessageWithConfirmation = false;


        public void AddCategory()
        {
            calledAddCategory = true;
        }

        public void AddExpense()
        {
            calledAddExpense |= true;
        }

        public void ClearInputs()
        {
            calledClearInputs = true;
        }

        public void OpenExistingFile()
        {
            calledOpenExistingFile = true;
        }

        public void OpenNewFile()
        {
            calledOpenNewFile = true;
        }

        public void Refresh()
        {
            calledRefresh = true;
        }

        public void ShowCurrentFile(string filename)
        {
            calledShowCurrentFile = true;
        }

        public void ShowError(string message)
        {
            calledShowError = true;
        }

        public void ShowMessageWithConfirmation(string message)
        {
            calledShowMessageWithConfirmation = true;
        }
    }


    [Collection("Sequential")]
    public class TestPresenter
    {
    }
}
