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
    // CLASS: Category
    //        - An individual category for budget program
    //        - Valid category types: Income, Expense, Credit, Saving
    // ====================================================================
    public class Category
    {

        // ====================================================================
        // Properties
        // ====================================================================

        //changed to be readonly
        public int Id { get; }
        public String Description { get; }
        public CategoryType Type { get;  }
        public enum CategoryType
        {
            Income=1,
            Expense,
            Credit,
            Savings
        };

        // ====================================================================
        // Constructor
        // ====================================================================
        public Category(int id, String description, CategoryType type = CategoryType.Expense)
        {
            this.Id = id;
            this.Description = description;
            this.Type = type;
        }

        // ====================================================================
        // Copy Constructor
        // ====================================================================

        //TODO: Check for removal
        public Category(Category category)
        {
            this.Id = category.Id;;
            this.Description = category.Description;
            this.Type = category.Type;
        }
        // ====================================================================
        // String version of object
        // ====================================================================
        public override string ToString()
        {
            return Description;
        }

    }
}

