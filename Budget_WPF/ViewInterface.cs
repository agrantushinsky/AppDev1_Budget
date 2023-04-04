using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Budget_WPF
{
    public interface ViewInterface
    {
        void OpenNewFile();
        void OpenExistingFile();
        void ShowCurrentFile(string filename);
    }
}
