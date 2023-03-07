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
    /// <summary>
    /// Creates an individual expenses for budget program
    /// </summary>
    public class Expense
    {
        // ====================================================================
        // Properties
        // ====================================================================

        /// <summary>
        /// Gets the ID of the expenses
        /// </summary>
        /// <value>Expense ID</value>
        public int Id { get; }
        /// <summary>
        /// Gets the date of transaction
        /// </summary>
        /// <value>Date of transaction</value>
        public DateTime Date { get;  }
        /// <summary>
        /// Gets amount of the expense
        /// </summary>
        /// <value>Expense amount</value>
        public Double Amount { get; }
        /// <summary>
        /// Gets the expense description
        /// </summary>
        /// <value>Expense description</value>
        public String Description { get; }
        /// <summary>
        /// Gets the category ID of the expense
        /// </summary>
        /// <value>Category ID</value>
        public int Category { get; }

        // ====================================================================
        // Constructor
        //    NB: there is no verification the expense category exists in the
        //        categories object
        // ====================================================================
        /// <summary>
        /// Creates an Expense object ans sets the properties using the passed arguments
        /// </summary>
        /// <param name="id">The Expense ID</param>
        /// <param name="date">The date of transaction</param>
        /// <param name="category">The category ID</param>
        /// <param name="amount">The expense amount</param>
        /// <param name="description">The description of the expense</param>
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
        /// <summary>
        /// Does a deep copy of an existing Expense object
        /// </summary>
        /// <param name="obj">The Expense object to be copied</param>
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
