﻿using System;
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

        public HomeBudget(String databaseFile)
        {
            Database.existingDatabase(databaseFile);
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
            const int IDX_EXPENSE_DATECODE = 0, IDX_EXPENSE_ID = 1, IDX_DATE = 2, IDX_DESCRIPTION = 3, IDX_AMOUNT = 4, IDX_CATEGORY_ID = 5,
                IDX_CATEGORY_DESCRIPTION = 6;

            //DateTime? doesnt have overload for toString cast to DateTime
            string StartTime = (Start ?? new DateTime(1900, 1, 1)).ToString();
            string EndTime = (End ?? new DateTime(2500, 1, 1)).ToString();

            // Create the select command
            const string QUERY_BUDGET_ITEMS = @"
                SELECT SUBSTR(e.Date, 1, 7) AS 'DateCode', e.Id, e.Date, e.Description, e.Amount, e.CategoryId, c.Description as 'CategoryDescription'
                FROM expenses as e
                JOIN categories as c ON e.CategoryId = c.Id
                WHERE e.Date >= @StartTime AND e.Date <= @EndTime 
                    AND (NOT @FilterFlag OR @CategoryId == e.CategoryId)
                ORDER BY SUBSTR(e.Date, 1, 7), e.Date;
            ";
            // TODO: Maybe not sort by the Date twice? Not sure.

            // Initialize the select command with the command text and connection.
            using var queryCommand = new SQLiteCommand(QUERY_BUDGET_ITEMS, Database.dbConnection);

            queryCommand.Parameters.Add(new SQLiteParameter("@StartTime", StartTime));
            queryCommand.Parameters.Add(new SQLiteParameter("@EndTime", EndTime));
            queryCommand.Parameters.Add(new SQLiteParameter("@CategoryId", CategoryID));
            queryCommand.Parameters.Add(new SQLiteParameter("@FilterFlag", FilterFlag));

            // Execute the reader
            using SQLiteDataReader reader = queryCommand.ExecuteReader();

            List<BudgetItemsByMonth> items = new List<BudgetItemsByMonth>();
            double total = 0;

            double amount;
            string? lastDateCode = null;
            BudgetItemsByMonth? currentMonth = null;

            while (reader.Read())
            {
                string dateCode = reader.GetString(IDX_EXPENSE_DATECODE);
                if(dateCode != lastDateCode)
                {
                    if(currentMonth != null)
                        items.Add(currentMonth);

                    currentMonth = new BudgetItemsByMonth
                    {
                        Details = new List<BudgetItem>(),
                        Month = dateCode
                    };
                }

                amount = reader.GetDouble(IDX_AMOUNT);
                total += amount;
                currentMonth.Details.Add(new BudgetItem
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
                currentMonth.Total += amount;
                lastDateCode = dateCode;
            }
            // Add the final month to the List
            if(currentMonth != null)
                items.Add(currentMonth);

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
            const int IDX_EXPENSE_ID = 0, IDX_DATE = 1, IDX_DESCRIPTION = 2, IDX_AMOUNT = 3, IDX_CATEGORY_ID = 4,
                          IDX_CATEGORY_DESCRIPTION = 5;

            //DateTime? doesnt have overload for toString cast to DateTime
            string startTime = (Start ?? new DateTime(1900, 1, 1)).ToString();
            string endTime = (End ?? new DateTime(2500, 1, 1)).ToString();

            // Create the select command
            const string query = @"
            SELECT e.Id, e.Date, e.Description, e.Amount, e.CategoryId, c.Description as 'CategoryDescription'
            FROM expenses as e
            JOIN categories as c ON e.CategoryId = c.Id
            WHERE e.Date >= @StartTime AND e.Date <= @EndTime 
                AND (NOT @FilterFlag OR @CategoryId == e.CategoryId)
            ORDER BY c.Description, e.Date;";

            // Initialize the select command with the command text and connection.
            using var command = new SQLiteCommand(query, Database.dbConnection);

            command.Parameters.Add(new SQLiteParameter("@StartTime", startTime));
            command.Parameters.Add(new SQLiteParameter("@EndTime", endTime));
            command.Parameters.Add(new SQLiteParameter("@CategoryId", CategoryID));
            command.Parameters.Add(new SQLiteParameter("@FilterFlag", FilterFlag));

            // Execute the reader
            using SQLiteDataReader reader = command.ExecuteReader();

            List<BudgetItemsByCategory> items = new List<BudgetItemsByCategory>();
            double total = 0;
            string currentCategory = null;
            List<BudgetItem> currentCategoryItems = null;

            double amount;
            while (reader.Read())
            {
                var category = reader.GetString(IDX_CATEGORY_DESCRIPTION);
                if (category != currentCategory)
                {
                    if (currentCategory != null)
                    {
                        items.Add(new BudgetItemsByCategory
                        {
                            Category = currentCategory,
                            Details = new List<BudgetItem>(),
                            Total = total
                        });
                        total = 0;
                    }
                    currentCategory = category;
                    currentCategoryItems = new List<BudgetItem>();
                }

                amount = reader.GetDouble(IDX_AMOUNT);
                total += amount;
                currentCategoryItems.Add(new BudgetItem
                {
                    ExpenseID = reader.GetInt32(IDX_EXPENSE_ID),
                    Date = DateTime.ParseExact(
                        reader.GetString(IDX_DATE),
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture),
                    ShortDescription = reader.GetString(IDX_DESCRIPTION),
                    Amount = amount,
                    CategoryID = reader.GetInt32(IDX_CATEGORY_ID),
                    Category = category,
                    Balance = total
                });
            }

            // Add the last category
            if (currentCategory != null)
            {
                items.Add(new BudgetItemsByCategory
                {
                    Category = currentCategory,
                    Details = new List<BudgetItem>(),
                    Total = total
                });
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
