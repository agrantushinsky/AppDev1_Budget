using Budget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Budget_WPF
{
    public class BudgetPresenter
    {
        private IBudgetView _view;
        private HomeBudget _model;

        public BudgetPresenter(IBudgetView view)
        {
            _view = view;
        }

        public void FiltersChange(DateTime start, DateTime end, int categoryId, bool shouldFilterCategory, bool groupByMonth, bool groupByCategory)
        {

        }
    }
}
