using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// ============================================================================
// (c) Sandy Bultena 2018
// * Released under the GNU General Public License
// ============================================================================

namespace Budget
{
    // ====================================================================
    // CLASS: Expense
    //        - An individual expens for budget program
    // ====================================================================
    public class Expense
    {
        private double _amount;        

        // ====================================================================
        // Properties
        // ====================================================================
        public int Id { get; }
        public DateTime Date { get;  }
        public Double Amount
        {
            get { return _amount; }
            set
            {
                if(this.Category == Convert.ToInt32(Budget.Category.CategoryType.Expense))
                {
                    _amount = value * -1;
                }
                else
                {
                    _amount = value;
                }
            }
        }
        public String Description { get; set; }
        public int Category { get; set; }

        // ====================================================================
        // Constructor
        //    NB: there is no verification the expense category exists in the
        //        categories object
        // ====================================================================
        public Expense(int id, DateTime date, int category, Double amount, String description)
        {
            this.Id = id;
            this.Date = date;
            this.Category = category;
            this.Amount = amount;
            this.Description = description;
        }

        // ====================================================================
        // Copy constructor - does a deep copy
        // ====================================================================
        public Expense (Expense obj)
        {
            this.Id = obj.Id;
            this.Date = obj.Date;
            this.Category = obj.Category;
            this.Amount = obj.Amount;
            this.Description = obj.Description;
           
        }
    }
}
