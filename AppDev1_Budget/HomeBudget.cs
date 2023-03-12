using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Dynamic;
using System.Data.SQLite;
using System.Globalization;
using System.CodeDom;

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
            //_expenses.ReadFromFile(expenseFile);
        }

        public HomeBudget(string databaseFile)
        {
            Database.existingDatabase(databaseFile);
            _categories = new Categories(Database.dbConnection, false);
        }

        #region GetList



        // ============================================================================
        // Get all expenses list
        // NOTE: VERY IMPORTANT... budget amount is the negative of the expense amount
        // Reasoning: an expense of $15 is -$15 from your bank account.
        // ============================================================================

        public List<BudgetItem> GetBudgetItems(DateTime? Start, DateTime? End, bool FilterFlag, int CategoryID)
        {
            const int IDX_EXPENSE_ID = 0, IDX_DATE = 1, IDX_DESCRIPTION = 2, IDX_AMOUNT = 3, IDX_CATEGORY_ID = 4,
                IDX_CATEGORY_DESCRIPTION = 5;

            //DateTime? doesnt have overload for toString cast to DateTime
            string StartTime = (Start ?? new DateTime(1900, 1, 1)).ToString();
            string EndTime = (End ?? new DateTime(2500, 1, 1)).ToString();

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
            double total = 0;

            double amount;
            while (reader.Read())
            {
                amount = reader.GetDouble(IDX_AMOUNT);
                total += amount;
                items.Add(new BudgetItem
                {
                    ExpenseID = reader.GetInt32(IDX_EXPENSE_ID),
                    Date = DateTime.ParseExact(
                        reader.GetString(IDX_DATE),
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture),
                    ShortDescription = reader.GetString(IDX_DESCRIPTION),
                    Amount = amount,
                    CategoryID = reader.GetInt32(IDX_CATEGORY_ID),
                    Category = reader.GetString(IDX_CATEGORY_DESCRIPTION),
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
            // Indices for the reader
            const int IDX_DATE_CODE = 0, IDX_MONTHLY_TOTAL = 1;

            DateTime startDate = Start ?? new DateTime(1900, 1, 1);
            DateTime endDate = End ?? new DateTime(2500, 1, 1);

            string startTime = startDate.ToString();
            string endTime = endDate.ToString();

            // Create the select command
            const string QUERY_BUDGET_ITEMS = @"
                SELECT SUBSTR(e.Date, 1, 7) AS 'DateCode', SUM(e.Amount) AS 'Monthly Total'
                FROM expenses as e
                JOIN categories as c ON e.CategoryId = c.Id
                WHERE e.Date >= @StartTime AND e.Date <= @EndTime 
                    AND (NOT @FilterFlag OR @CategoryId == e.CategoryId)
                GROUP BY SUBSTR(e.Date, 1, 7)
                ORDER BY e.Date;
            ";

            // Initialize the select command with the command text and connection.
            using var queryCommand = new SQLiteCommand(QUERY_BUDGET_ITEMS, Database.dbConnection);

            queryCommand.Parameters.Add(new SQLiteParameter("@StartTime", startTime));
            queryCommand.Parameters.Add(new SQLiteParameter("@EndTime", endTime));
            queryCommand.Parameters.Add(new SQLiteParameter("@CategoryId", CategoryID));
            queryCommand.Parameters.Add(new SQLiteParameter("@FilterFlag", FilterFlag));

            // Execute the reader
            using SQLiteDataReader reader = queryCommand.ExecuteReader();

            List<BudgetItemsByMonth> items = new List<BudgetItemsByMonth>();

            const int DATE_YEAR = 0, DATE_MONTH = 1;

            while (reader.Read())
            {
                string dateCode = reader.GetString(IDX_DATE_CODE);
                double monthlyTotal = reader.GetDouble(IDX_MONTHLY_TOTAL);

                string[] dateCodeSplit = dateCode.Split('-');

                int year = int.Parse(dateCodeSplit[DATE_YEAR]);
                int month = int.Parse(dateCodeSplit[DATE_MONTH]);
                int startDay = 1;
                int endDay = DateTime.DaysInMonth(year, month);

                if (startDate.Year == year && startDate.Month == month)
                    startDay = startDate.Day;

                if (endDate.Year == year && endDate.Month == month)
                    endDay = endDate.Day;

                DateTime start = new DateTime(year, month, startDay);
                DateTime end = new DateTime(year, month, endDay);

                List<BudgetItem> monthlyBudgetItems = GetBudgetItems(start, end, FilterFlag, CategoryID);

                BudgetItemsByMonth monthlyBudget = new BudgetItemsByMonth()
                {
                    Month = dateCode,
                    Details = monthlyBudgetItems,
                    Total = monthlyTotal
                };

                items.Add(monthlyBudget);
            }

            return items;
        }

        // ============================================================================
        // Group all expenses by category (ordered by category name)
        // ============================================================================

        /// <summary>
        /// Gets a list of budget items by category, including the category name, total amount, and number of expenses.
        /// </summary>
        /// <param name="Start">Optional start date of the expenses to retrieve,default is 1-1-1900</param>
        /// <param name="End">Optional end date of the expenses to retrieve, default is 1-1-2500</param>
        /// <param name="FilterFlag">A flag to indicate whether to filter by category ID or not.</param>
        /// <param name="CategoryID">The ID of the category to filter by, if filter flag is true.</param>
        /// <returns>A list of budget items by category.</returns>
        public List<BudgetItemsByCategory> GetBudgetItemsByCategory(DateTime? Start, DateTime? End, bool FilterFlag, int CategoryID)
        {
            // Indices for the reader
            const int IDX_CATEGORY = 0, IDX_CATEGORY_ID = 1, IDX_CATEGORY_TOTAL = 2;

            DateTime startDate = Start ?? new DateTime(1900, 1, 1);
            DateTime endDate = End ?? new DateTime(2500, 1, 1);

            string startTime = startDate.ToString();
            string endTime = endDate.ToString();

            // Create the select command
            const string QUERY_BUDGET_ITEMS = @"
                SELECT c.Description, c.Id, SUM(e.Amount)
                FROM expenses as e
                JOIN categories as c ON e.CategoryId = c.Id   
                WHERE e.Date >= @StartTime AND e.Date <= @EndTime 
                    AND (NOT @FilterFlag OR @CategoryId == e.CategoryId)
                GROUP BY c.Description
                ORDER BY e.Date;
            ";

            // Initialize the select command with the command text and connection.
            using var queryCommand = new SQLiteCommand(QUERY_BUDGET_ITEMS, Database.dbConnection);

            queryCommand.Parameters.Add(new SQLiteParameter("@StartTime", startTime));
            queryCommand.Parameters.Add(new SQLiteParameter("@EndTime", endTime));
            queryCommand.Parameters.Add(new SQLiteParameter("@CategoryId", CategoryID));
            queryCommand.Parameters.Add(new SQLiteParameter("@FilterFlag", FilterFlag));

            // Execute the reader
            using SQLiteDataReader reader = queryCommand.ExecuteReader();

            List<BudgetItemsByCategory> items = new List<BudgetItemsByCategory>();

            while (reader.Read())
            {
                string category = reader.GetString(IDX_CATEGORY);
                int categoryId = reader.GetInt32(IDX_CATEGORY_ID);
                double categoryTotal = reader.GetDouble(IDX_CATEGORY_TOTAL);

                List<BudgetItem> categoryBudgetItems = GetBudgetItems(Start, End, true, categoryId);

                BudgetItemsByCategory categoryBudget = new BudgetItemsByCategory()
                {
                    Category = category,
                    Details = categoryBudgetItems,
                    Total = categoryTotal
                };

                items.Add(categoryBudget);
            }

            return items;
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
