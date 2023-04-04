using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Budget_WPF
{
    public interface ViewInterface
    {
        //Rough Idea of what is needed
        void AddCategory();
        
        void AddExpense();

        void Refresh();

        void OpenNewFile();
        void OpenExistingFile();
        void ShowCurrentFile(string filename);
        void ShowError(string message);
    }
}
