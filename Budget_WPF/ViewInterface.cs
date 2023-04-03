using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Budget_WPF
{
    interface ViewInterface
    {
        //Rough Idea of what is needed
        void ShowExpensesWindow();

        void ShowCategoriesWindow();

        void AddCategory();
        
        void AddExpense();

        void Refresh();
    }
}
