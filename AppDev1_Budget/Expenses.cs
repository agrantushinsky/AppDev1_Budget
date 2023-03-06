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
    // ====================================================================
    // CLASS: expenses
    //        - A collection of expense items,
    //        - Read / write to file
    //        - etc
    // ====================================================================
    public class Expenses
    {
        public void Add(DateTime date, int category, Double amount, String description)
        {
            if (ValidateCategoryId(category))
            {
                _InsertExpense(date, category, amount, description);
            }
            else
            {
                throw new ArgumentException("Provided category ID does not exists.");
            }
        }

        private void _InsertExpense(DateTime date, int category, Double amount, String description)
        {
            const string insertCommandText = "INSERT INTO expenses(Date, CategoryId, Amount, Description) VALUES" +
                "(@Date, @CategoryId, @Amount, @Description)";

            using var insertCommand = new SQLiteCommand(insertCommandText, Database.dbConnection);

            insertCommand.Parameters.Add(new SQLiteParameter("@Date", date.ToString("yyyy-MM-dd")));
            insertCommand.Parameters.Add(new SQLiteParameter("@CategoryId", category));
            insertCommand.Parameters.Add(new SQLiteParameter("@Amount", amount));
            insertCommand.Parameters.Add(new SQLiteParameter("@Description", description));
            insertCommand.Prepare();

            insertCommand.ExecuteNonQuery();
        }

        public void Delete(int Id)
        {
            _DeleteExpense(Id);
        }

        public void DeleteAllExpenses()
        {
            _TruncateExpensesTable();
        }

        private void _DeleteExpense(int id)
        {
            const string deleteCommandText = "DELETE FROM expenses WHERE Id = @Id";

            using var deleteCommand = new SQLiteCommand(deleteCommandText, Database.dbConnection);

            deleteCommand.Parameters.Add(new SQLiteParameter("@Id", id));
            deleteCommand.Prepare();

            try
            {
                deleteCommand.ExecuteNonQuery();
            }
            catch(Exception ex)
            {
                throw new SQLiteException($"Error while deleting category of id: {id} from database: {ex.Message}");
            }

        }

        private void _TruncateExpensesTable()
        {
            const string deleteCommandText = "DELETE FROM expenses";

            using var deleteCommand = new SQLiteCommand(deleteCommandText, Database.dbConnection);

            try
            {
                deleteCommand.ExecuteNonQuery();
            }
            catch(Exception ex)
            {
                throw new SQLiteException($"Error while deleting all values from the expenses table: {ex.Message}");
            }
        }

        public void Update(int id, DateTime newDate, int newCategory, Double newAmount, String newDescription)
        {
            _UpdateExpense(id, newDate, newCategory, newAmount, newDescription);
        }

        private void _UpdateExpense(int id, DateTime newDate, int newCategory, Double newAmount, String newDesptiption)
        {
            const string updateCommandText = "UPDATE expenses SET Date = @Date, CategoryId = @Category, Amount = @Amount, Description = @Description WHERE Id = @Id";

            using var updateCommand = new SQLiteCommand(updateCommandText, Database.dbConnection);

            updateCommand.Parameters.Add(new SQLiteParameter("@Id", id));
            updateCommand.Parameters.Add(new SQLiteParameter("@Date", newDate.ToString("yyyy-MM-dd")));
            updateCommand.Parameters.Add(new SQLiteParameter("@Category", newCategory));
            updateCommand.Parameters.Add(new SQLiteParameter("@Amount", newAmount));
            updateCommand.Parameters.Add(new SQLiteParameter("@Description", newDesptiption));
            updateCommand.Prepare();

            updateCommand.ExecuteNonQuery();
        }

        public Expense GetExpenseFromId(int id)
        {
            Expense? e = _SelectExpense(id);
            if(e == null)
            {
                throw new Exception($"Cannot find expense with id {id}.");
            }

            return e;
        }

        private Expense _SelectExpense(int id)
        {
            const int IDX_ID = 0, IDX_DATE = 1, IDX_AMOUNT = 2, IDX_DESCRIPTION = 3, IDX_CATEGORY = 4;

            const string selectCommandText = "SELECT Id, Date, Amount, Description, CategoryId FROM expenses WHERE Id = @Id";
            
            using var selectCommand = new SQLiteCommand(selectCommandText, Database.dbConnection);

            selectCommand.Parameters.Add(new SQLiteParameter("@Id", id));
            selectCommand.Prepare();

            using SQLiteDataReader reader = selectCommand.ExecuteReader();

            if (!reader.Read())
            {
                return null;
            }

            return new Expense(
                    reader.GetInt32(IDX_ID),
                    DateTime.Parse(reader.GetString(IDX_DATE)),
                    reader.GetInt32(IDX_CATEGORY),
                    reader.GetDouble(IDX_AMOUNT),
                    reader.GetString(IDX_DESCRIPTION));
                
        }

        public List<Expense> List()
        {
            return _GetExpenses();
        }

        public List<Expense> _GetExpenses()
        {
            List<Expense> expenses = new List<Expense>();

            const int IDX_ID = 0, IDX_DATE = 1, IDX_AMOUNT = 2, IDX_DESCRIPTION = 3, IDX_CATEGORY = 4;

            const string selectCommandText = "SELECT Id, Date, Amount, Description, CategoryId FROM expenses ORDER BY Id";
            using var selectCommand = new SQLiteCommand(selectCommandText, Database.dbConnection);

            using SQLiteDataReader reader = selectCommand.ExecuteReader();

            while(reader.Read())
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

        private bool ValidateCategoryId(int catId)
        {
            Categories category = new Categories(Database.dbConnection, false);
            List<Category> categoriesList = category.List();
            return categoriesList.Exists(c => c.Id == catId);
        }
    }
}

