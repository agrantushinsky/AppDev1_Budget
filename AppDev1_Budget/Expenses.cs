using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Data.SQLite;
using System.Configuration.Internal;
using System.Net.Http.Headers;
using System.CodeDom;

// ============================================================================
// (c) Sandy Bultena 2018
// * Released under the GNU General Public License
// ============================================================================

namespace Budget
{
    /// <summary>
    /// Manages the expenses table in the database using the CRUD operations.
    /// </summary>
    public class Expenses
    {
        /// <summary>
        /// Adds an Expense object in the database using the arguments
        /// </summary>
        /// <param name="date">Date of transaction</param>
        /// <param name="category">Category ID</param>
        /// <param name="amount">Amount of the expense</param>
        /// <param name="description">Description of the expense</param>
        /// <exception cref="ArgumentException">If the category ID does not exist</exception>
        public void Add(DateTime date, int category, Double amount, String description)
        {
            if (_ValidateCategoryId(category))
            {
                _InsertExpense(date, category, amount, description);
            }
            else
            {
                throw new ArgumentException("Provided category ID does not exists.");
            }
        }

        /// <summary>
        /// Gets the expense object with the provided ID
        /// </summary>
        /// <param name="id">ID of the expense item to get</param>
        /// <returns>Expense object</returns>
        /// <exception cref="Exception">If the ID does not exist</exception>
        public Expense GetExpenseFromId(int id)
        {
            Expense? e = _SelectExpense(id);
            if (e == null)
            {
                throw new Exception($"Cannot find expense with id {id}.");
            }

            return e;
        }

        /// <summary>
        /// Gets a list with all the expense items
        /// </summary>
        /// <returns>List with all the Expense items</returns>
        public List<Expense> List()
        {
            return _GetExpenses();
        }

        /// <summary>
        /// Updates the date, category ID, amount and description of the the expense item with the provided ID
        /// </summary>
        /// <param name="id">ID of the expenses item to update</param>
        /// <param name="newDate">New date</param>
        /// <param name="newCategory">New category ID</param>
        /// <param name="newAmount">New Amount</param>
        /// <param name="newDescription">New Description</param>
        public void Update(int id, DateTime newDate, int newCategory, Double newAmount, String newDescription)
        {
            _UpdateExpense(id, newDate, newCategory, newAmount, newDescription);
        }

        /// <summary>
        /// Deletes the expenses item with the provided ID
        /// </summary>
        /// <param name="Id">ID of the expense item to delete</param>
        public void Delete(int Id)
        {
            _DeleteExpense(Id);
        }

        /// <summary>
        /// Deletes all items in the expenses table
        /// </summary>
        public void DeleteAllExpenses()
        {
            _TruncateExpensesTable();
        }

        private void _InsertExpense(DateTime date, int category, Double amount, String description)
        {
            //Create insert command
            const string insertCommandText = "INSERT INTO expenses(Date, CategoryId, Amount, Description) VALUES" +
                "(@Date, @CategoryId, @Amount, @Description)";

            //Initialize the insert command with the command text and connection
            using var insertCommand = new SQLiteCommand(insertCommandText, Database.dbConnection);

            //Setup parameters
            insertCommand.Parameters.Add(new SQLiteParameter("@Date", date.ToString("yyyy-MM-dd")));
            insertCommand.Parameters.Add(new SQLiteParameter("@CategoryId", category));
            insertCommand.Parameters.Add(new SQLiteParameter("@Amount", amount));
            insertCommand.Parameters.Add(new SQLiteParameter("@Description", description));
            insertCommand.Prepare();

            //Execute the command
            insertCommand.ExecuteNonQuery();
        }

        private bool _ValidateCategoryId(int catId)
        {
            Categories category = new Categories(Database.dbConnection, false);
            List<Category> categoriesList = category.List();

            //Returns true if the provided category ID exists in the categories table
            //Otherwise, false
            return categoriesList.Exists(c => c.Id == catId);
        }

        private Expense _SelectExpense(int id)
        {
            const int IDX_ID = 0, IDX_DATE = 1, IDX_AMOUNT = 2, IDX_DESCRIPTION = 3, IDX_CATEGORY = 4;

            //Create select command
            const string selectCommandText = "SELECT Id, Date, Amount, Description, CategoryId FROM expenses WHERE Id = @Id";

            //Initialize the select command with the command text and connection
            using var selectCommand = new SQLiteCommand(selectCommandText, Database.dbConnection);

            //Setup parameters
            selectCommand.Parameters.Add(new SQLiteParameter("@Id", id));
            selectCommand.Prepare();

            //Execute reader
            using SQLiteDataReader reader = selectCommand.ExecuteReader();

            //If expense not found, return null
            if (!reader.Read())
            {
                return null;
            }

            //Return Expense object
            return new Expense(
                    reader.GetInt32(IDX_ID),
                    DateTime.Parse(reader.GetString(IDX_DATE)),
                    reader.GetInt32(IDX_CATEGORY),
                    reader.GetDouble(IDX_AMOUNT),
                    reader.GetString(IDX_DESCRIPTION));

        }

        public List<Expense> _GetExpenses()
        {
            List<Expense> expenses = new List<Expense>();

            //Constants for reader indices
            const int IDX_ID = 0, IDX_DATE = 1, IDX_AMOUNT = 2, IDX_DESCRIPTION = 3, IDX_CATEGORY = 4;

            //Create select command
            const string selectCommandText = "SELECT Id, Date, Amount, Description, CategoryId FROM expenses ORDER BY Id";

            //Initialize the select command with the command text and connection
            using var selectCommand = new SQLiteCommand(selectCommandText, Database.dbConnection);

            //Execute reader
            using SQLiteDataReader reader = selectCommand.ExecuteReader();

            //Adds each Expense item into the list
            while (reader.Read())
            {
                expenses.Add(new Expense(
                    reader.GetInt32(IDX_ID),
                    DateTime.Parse(reader.GetString(IDX_DATE)),
                    reader.GetInt32(IDX_CATEGORY),
                    reader.GetDouble(IDX_AMOUNT),
                    reader.GetString(IDX_DESCRIPTION)));
            }

            return expenses;
        }

        private void _UpdateExpense(int id, DateTime newDate, int newCategory, Double newAmount, String newDesptiption)
        {
            //Create update command
            const string updateCommandText = "UPDATE expenses SET Date = @Date, CategoryId = @Category, Amount = @Amount, Description = @Description WHERE Id = @Id";

            //Initialize the update command with the command text and connection
            using var updateCommand = new SQLiteCommand(updateCommandText, Database.dbConnection);

            //Setup parameters
            updateCommand.Parameters.Add(new SQLiteParameter("@Id", id));
            updateCommand.Parameters.Add(new SQLiteParameter("@Date", newDate.ToString("yyyy-MM-dd")));
            updateCommand.Parameters.Add(new SQLiteParameter("@Category", newCategory));
            updateCommand.Parameters.Add(new SQLiteParameter("@Amount", newAmount));
            updateCommand.Parameters.Add(new SQLiteParameter("@Description", newDesptiption));
            updateCommand.Prepare();

            //Execute the command
            updateCommand.ExecuteNonQuery();
        }

        private void _DeleteExpense(int id)
        {
            //Create delete command
            const string deleteCommandText = "DELETE FROM expenses WHERE Id = @Id";

            //Initialize the delete command with the command text and connection
            using var deleteCommand = new SQLiteCommand(deleteCommandText, Database.dbConnection);

            //Setup parameters
            deleteCommand.Parameters.Add(new SQLiteParameter("@Id", id));
            deleteCommand.Prepare();

            try
            {
                //Execute the command
                deleteCommand.ExecuteNonQuery();
            }
            catch(Exception ex)
            {
                //throws exception if not allowed to delete in database
                throw new SQLiteException($"Error while deleting category of id: {id} from database: {ex.Message}");
            }

        }

        private void _TruncateExpensesTable()
        {
            //Create delete command
            const string deleteCommandText = "DELETE FROM expenses";

            //Initialize the delete command with the command text and connection
            using var deleteCommand = new SQLiteCommand(deleteCommandText, Database.dbConnection);

            try
            {
                //Execute the command
                deleteCommand.ExecuteNonQuery();
            }
            catch(Exception ex)
            {
                //throws exception if not allowed to delete in database
                throw new SQLiteException($"Error while deleting all values from the expenses table: {ex.Message}");
            }
        }

    }
}

