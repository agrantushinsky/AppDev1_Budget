using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Budget;
using System.Data.SQLite;
using System.Data.Entity.Infrastructure;

namespace Budget_WPF
{
    public class Presenter
    {
        private ViewInterface _view;
        private HomeBudget _model;

        public Presenter(ViewInterface view)
        {
            _view = view;
        }

        public void ConnectToDatabase(string filename, bool newDatabase)
        {
            _model = new HomeBudget(filename, newDatabase);
            _view.ShowCurrentFile(filename);
        }
    }
}
