using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Dynamic;
using System.Data.SQLite;
using System.Globalization;

// ============================================================================
// (c) Sandy Bultena 2018
// * Released under the GNU General Public License
// ============================================================================


namespace Budget
{
    // ====================================================================
    // CLASS: HomeBudget
    //        - Combines categories Class and expenses Class
    //        - One File defines Category and Budget File
    //        - etc
    // ====================================================================

    public class HomeBudget
    {
        private Categories _categories;
        private Expenses _expenses;


        // Properties (categories and expenses object)
        public Categories categories { get { return _categories; } }
        public Expenses expenses { get { return _expenses; } }

        //Used for tests
        public HomeBudget(String databaseFile, string expenseFile, bool newDatabase=false) 
        {
            if (!newDatabase && File.Exists(databaseFile))
            {
                Database.existingDatabase(databaseFile);
            }
            else
            {
                Database.newDatabase(databaseFile);
                newDatabase = true;

            }
            _categories = new Categories(Database.dbConnection, newDatabase);
            _expenses = new Expenses();

            //read the expense from the xml
            _expenses.ReadFromFile(expenseFile);
        }

        #region GetList



        // ============================================================================
        // Get all expenses list
        // NOTE: VERY IMPORTANT... budget amount is the negative of the expense amount
        // Reasoning: an expense of $15 is -$15 from your bank account.
        // ============================================================================

        public List<BudgetItem> GetBudgetItems(DateTime? Start, DateTime? End, bool FilterFlag, int CategoryID)
        {
            const int IDX_EXPID = 0, IDX_DATE = 1, IDX_DESCRIPTION = 2, IDX_AMOUNT = 3, IDX_CATEGORYID = 4,
                IDX_CATEGORYDESCRIPTION = 5;

            //DateTime? doesnt have overload for toString cast to DateTime
            string StartTime = ((DateTime)Start).ToString("yyyy-MM-dd") ?? new string("1900-1-1");
            string EndTime = ((DateTime)End).ToString("yyyy-MM-dd") ?? new string("2500-1-1");

            // Create the select command
            const string QUERY_BUDGET_ITEMS = @"
                SELECT e.Id, e.Date, e.Description, e.Amount, e.CategoryId, c.Description as 'CategoryDescription'
                FROM expenses as e
                JOIN categories as c ON e.CategoryId = c.Id
                WHERE e.Date >= @StartTime AND e.Date <= @EndTime 
                AND (NOT @FilterFlag OR @CategoryId == e.CategoryId)
                ORDER BY e.Date; 
            ";

            // Initialize the select command with the command text and connection.
            using var queryCommand = new SQLiteCommand(QUERY_BUDGET_ITEMS, Database.dbConnection);

            queryCommand.Parameters.Add(new SQLiteParameter("@StartTime", StartTime));
            queryCommand.Parameters.Add(new SQLiteParameter("@EndTime", EndTime));
            queryCommand.Parameters.Add(new SQLiteParameter("@CategoryId", CategoryID));
            queryCommand.Parameters.Add(new SQLiteParameter("@FilterFlag", FilterFlag));

            // Execute the reader
            using SQLiteDataReader reader = queryCommand.ExecuteReader();

            List<BudgetItem> items = new List<BudgetItem>();
            Double total = 0;

            while (reader.Read())
            {
                total = total + reader.GetDouble(IDX_AMOUNT);
                items.Add(new BudgetItem
                {
                    ExpenseID = reader.GetInt32(IDX_EXPID),
                    Date = DateTime.ParseExact(
                        reader.GetString(IDX_DATE),
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture),
                    ShortDescription = reader.GetString(IDX_DESCRIPTION),
                    Amount = reader.GetDouble(IDX_AMOUNT),
                    CategoryID = reader.GetInt32(IDX_CATEGORYID),
                    Category = reader.GetString(IDX_CATEGORYDESCRIPTION),
                    Balance = total
                });
            }
            return items;
        }

        // ============================================================================
        // Group all expenses month by month (sorted by year/month)
        // returns a list of BudgetItemsByMonth which is 
        // "year/month", list of budget items, and total for that month
        // ============================================================================
        public List<BudgetItemsByMonth> GetBudgetItemsByMonth(DateTime? Start, DateTime? End, bool FilterFlag, int CategoryID)
        {
            // -----------------------------------------------------------------------
            // get all items first
            // -----------------------------------------------------------------------
            List<BudgetItem> items = GetBudgetItems(Start, End, FilterFlag, CategoryID);

            // -----------------------------------------------------------------------
            // Group by year/month
            // -----------------------------------------------------------------------
            var GroupedByMonth = items.GroupBy(c => c.Date.Year.ToString("D4") + "/" + c.Date.Month.ToString("D2"));

            // -----------------------------------------------------------------------
            // create new list
            // -----------------------------------------------------------------------
            var summary = new List<BudgetItemsByMonth>();
            foreach (var MonthGroup in GroupedByMonth)
            {
                // calculate total for this month, and create list of details
                double total = 0;
                var details = new List<BudgetItem>();
                foreach (var item in MonthGroup)
                {
                    total = total + item.Amount;
                    details.Add(item);
                }

                // Add new BudgetItemsByMonth to our list
                summary.Add(new BudgetItemsByMonth
                {
                    Month = MonthGroup.Key,
                    Details = details,
                    Total = total
                });
            }

            return summary;
        }

        // ============================================================================
        // Group all expenses by category (ordered by category name)
        // ============================================================================
        public List<BudgetItemsByCategory> GeBudgetItemsByCategory(DateTime? Start, DateTime? End, bool FilterFlag, int CategoryID)
        {
            // -----------------------------------------------------------------------
            // get all items first
            // -----------------------------------------------------------------------
            List<BudgetItem> items = GetBudgetItems(Start, End, FilterFlag, CategoryID);

            // -----------------------------------------------------------------------
            // Group by Category
            // -----------------------------------------------------------------------
            var GroupedByCategory = items.GroupBy(c => c.Category);

            // -----------------------------------------------------------------------
            // create new list
            // -----------------------------------------------------------------------
            var summary = new List<BudgetItemsByCategory>();
            foreach (var CategoryGroup in GroupedByCategory.OrderBy(g => g.Key))
            {
                // calculate total for this category, and create list of details
                double total = 0;
                var details = new List<BudgetItem>();
                foreach (var item in CategoryGroup)
                {
                    total = total + item.Amount;
                    details.Add(item);
                }

                // Add new BudgetItemsByCategory to our list
                summary.Add(new BudgetItemsByCategory
                {
                    Category = CategoryGroup.Key,
                    Details = details,
                    Total = total
                });
            }

            return summary;
        }


        // ============================================================================
        // Group all events by category and Month
        // creates a list of Dictionary objects (which are objects that contain key value pairs).
        // The list of Dictionary objects includes:
        //          one dictionary object per month with expenses,
        //          and one dictionary object for the category totals
        // 
        // Each per month dictionary object has the following key value pairs:
        //           "Month", <the year/month for that month as a string>
        //           "Total", <the total amount for that month as a double>
        //            and for each category for which there is an expense in the month:
        //             "items:category", a List<BudgetItem> of all items in that category for the month
        //             "category", the total amount for that category for this month
        //
        // The one dictionary for the category totals has the following key value pairs:
        //             "Month", the string "TOTALS"
        //             for each category for which there is an expense in ANY month:
        //             "category", the total for that category for all the months
        // ============================================================================
        public List<Dictionary<string,object>> GetBudgetDictionaryByCategoryAndMonth(DateTime? Start, DateTime? End, bool FilterFlag, int CategoryID)
        {
            // -----------------------------------------------------------------------
            // get all items by month 
            // -----------------------------------------------------------------------
            List<BudgetItemsByMonth> GroupedByMonth = GetBudgetItemsByMonth(Start, End, FilterFlag, CategoryID);

            // -----------------------------------------------------------------------
            // loop over each month
            // -----------------------------------------------------------------------
            var summary = new List<Dictionary<string, object>>();
            var totalsPerCategory = new Dictionary<String, Double>();

            foreach (var MonthGroup in GroupedByMonth)
            {
                // create record object for this month
                Dictionary<string, object> record = new Dictionary<string, object>();
                record["Month"] = MonthGroup.Month;
                record["Total"] = MonthGroup.Total;

                // break up the month details into categories
                var GroupedByCategory = MonthGroup.Details.GroupBy(c => c.Category);

                // -----------------------------------------------------------------------
                // loop over each category
                // -----------------------------------------------------------------------
                foreach (var CategoryGroup in GroupedByCategory.OrderBy(g => g.Key))
                {

                    // calculate totals for the cat/month, and create list of details
                    double total = 0;
                    var details = new List<BudgetItem>();

                    foreach (var item in CategoryGroup)
                    {
                        total = total + item.Amount;
                        details.Add(item);
                    }

                    // add new properties and values to our record object
                    record["details:" + CategoryGroup.Key] =  details;
                    record[CategoryGroup.Key] = total;

                    // keep track of totals for each category
                    if (totalsPerCategory.TryGetValue(CategoryGroup.Key, out Double CurrentCatTotal))
                    {
                        totalsPerCategory[CategoryGroup.Key] = CurrentCatTotal + total;
                    }
                    else
                    {
                        totalsPerCategory[CategoryGroup.Key] = total;
                    }
                }

                // add record to collection
                summary.Add(record);
            }
            // ---------------------------------------------------------------------------
            // add final record which is the totals for each category
            // ---------------------------------------------------------------------------
            Dictionary<string, object> totalsRecord = new Dictionary<string, object>();
            totalsRecord["Month"] = "TOTALS";

            foreach (var cat in categories.List())
            {
                try
                {
                    totalsRecord.Add(cat.Description, totalsPerCategory[cat.Description]);
                }
                catch { }
            }
            summary.Add(totalsRecord);


            return summary;
        }




        #endregion GetList

    }
}
