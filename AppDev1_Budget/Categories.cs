using System.Data.SQLite;

// ============================================================================
// (c) Sandy Bultena 2018
// * Released under the GNU General Public License
// ============================================================================

namespace Budget
{
    // ====================================================================
    // CLASS: categories
    //        - A collection of category items,
    //        - Read / write to file
    //        - etc
    // ====================================================================
    public class Categories
    {
        private string _FileName;
        private string _DirName;

        // ====================================================================
        // Properties
        // ====================================================================
        public String FileName { get { return _FileName; } }
        public String DirName { get { return _DirName; } }

        // ====================================================================
        // Constructor
        // ====================================================================
        public Categories()
        {
            SetCategoriesToDefaults();
        }

        //Used for testing
        public Categories(SQLiteConnection conn, bool newDatabase)
        {
            if (newDatabase)
            {
                SetCategoriesToDefaults();
            }
        }
        // ====================================================================
        // get a specific category from the list where the id is the one specified
        // ====================================================================
        public Category GetCategoryFromId(int i)
        {
            Category? c = _SelectCategory(i);
            if (c == null)
            {
                throw new Exception("Cannot find category with id " + i.ToString());
            }
            return c;
        }

        // ====================================================================
        // set categories to default
        // ====================================================================
        public void SetCategoriesToDefaults()
        {
            _RemoveAll();

            // Add default categoryTypes
            _InsertCategoryType((int)Category.CategoryType.Income, "Income");
            _InsertCategoryType((int)Category.CategoryType.Expense, "Expense");
            _InsertCategoryType((int)Category.CategoryType.Credit, "Credit");
            _InsertCategoryType((int)Category.CategoryType.Savings, "Savings");

            // ---------------------------------------------------------------
            // Add Defaults
            // ---------------------------------------------------------------
            _InsertCategory("Utilities", Category.CategoryType.Expense);
            _InsertCategory("Rent", Category.CategoryType.Expense);
            _InsertCategory("Food", Category.CategoryType.Expense);
            _InsertCategory("Entertainment", Category.CategoryType.Expense);
            _InsertCategory("Education", Category.CategoryType.Expense);
            _InsertCategory("Miscellaneous", Category.CategoryType.Expense);
            _InsertCategory("Medical Expenses", Category.CategoryType.Expense);
            _InsertCategory("Vacation", Category.CategoryType.Expense);
            _InsertCategory("Credit Card", Category.CategoryType.Credit);
            _InsertCategory("Clothes", Category.CategoryType.Expense);
            _InsertCategory("Gifts", Category.CategoryType.Expense);
            _InsertCategory("Insurance", Category.CategoryType.Expense);
            _InsertCategory("Transportation", Category.CategoryType.Expense);
            _InsertCategory("Eating Out", Category.CategoryType.Expense);
            _InsertCategory("Savings", Category.CategoryType.Savings);
            _InsertCategory("Income", Category.CategoryType.Income);

        }

        // ====================================================================
        // Add category
        // ====================================================================
        public void Add(String desc, Category.CategoryType type)
        {
            _InsertCategory(desc, type);
        }

        // ====================================================================
        // Delete category
        // ====================================================================
        /// <summary>
        /// Deletes the category with the specified id from the database.
        /// </summary>
        /// <param name="Id">The id of the category you want to delete</param>
        public void Delete(int Id)
        {
            // Delete from database
            _DeleteCategory(Id);
        }

        // ====================================================================
        // Return list of categories
        // Note:  make new copy of list, so user cannot modify what is part of
        //        this instance
        // ====================================================================

        /// <summary>
        /// Gets all the categories from the database.
        /// </summary>
        /// <returns>A list of all of the categories in the database table.</returns>
        public List<Category> List()
        {
            return _GetCategories();
        }

        /// <summary>
        /// Finds the category in the list and replaces it with the passed category data.
        /// </summary>
        /// <param name="id">The id of the category to replace.</param>
        /// <param name="newDescr">The description of the new category.</param>
        /// <param name="type">The CategoryType of the new category.</param>
        public void UpdateProperties(int id, string newDescr, Category.CategoryType type)
        {
            _UpdateCategory(new Category(id, newDescr, type));
        }

        private List<Category> _GetCategories()
        {
            List<Category> categories = new List<Category>();
            // Constants for reader indices
            const int IDX_ID = 0, IDX_DESCRIPTION = 1, IDX_CATEGORY = 2;

            // Create the select command
            const string selectCommandText = "SELECT Id, Description, TypeId FROM categories";
            using var selectCommand = new SQLiteCommand(selectCommandText, Database.dbConnection);

            // Execute the reader
            using SQLiteDataReader reader = selectCommand.ExecuteReader();

            // Loop through all the reader information
            while (reader.Read())
            {
                // Add the category to the list from the database
                categories.Add(new Category(
                    reader.GetInt32(IDX_ID),
                    reader.GetString(IDX_DESCRIPTION),
                    (Category.CategoryType)reader.GetInt32(IDX_CATEGORY)));
            }
            return categories;
        }

        private void _InsertCategory(string description, Category.CategoryType type)
        {
            // Create the insert command text
            const string insertCommandText = "INSERT INTO categories(Id, Description, TypeId) VALUES(@Id, @Description, @TypeId)";

            // Initialize the insert command with the command text and connection.
            using var insertCommand = new SQLiteCommand(insertCommandText, Database.dbConnection);

            // Setup parameters:
            int nextId = _GetNextID();
            insertCommand.Parameters.Add(new SQLiteParameter("@Id", nextId));
            insertCommand.Parameters.Add(new SQLiteParameter("@Description", description));
            insertCommand.Parameters.Add(new SQLiteParameter("@TypeId", type));
            insertCommand.Prepare();

            // Finally, execute the command
            insertCommand.ExecuteNonQuery();
        }

        private void _InsertCategoryType(int typeId, string description)
        {
            // Create the insert command text
            const string insertCommandText = "INSERT INTO categoryTypes(Id, Description) VALUES(@Id, @Description)";

            // Initialize the insert command with the command text and connection.
            using var insertCommand = new SQLiteCommand(insertCommandText, Database.dbConnection);

            // Setup parameters:
            insertCommand.Parameters.Add(new SQLiteParameter("@Id", typeId));
            insertCommand.Parameters.Add(new SQLiteParameter("@Description", description));
            insertCommand.Prepare();

            // Finally, execute the command
            insertCommand.ExecuteNonQuery();
        }

        private void _DeleteCategory(int id)
        {
            // Create the delete command text
            const string deleteCommandText = "DELETE FROM categories WHERE Id=@Id";

            // Initialize the delete command with the command text and connection
            using var deleteCommand = new SQLiteCommand(deleteCommandText, Database.dbConnection);

            // Setup the ID parameter
            deleteCommand.Parameters.Add(new SQLiteParameter("@Id", id));
            deleteCommand.Prepare();

            // Execute the delete operation

            //Throw excetion if not allowed to delete in database
            try
            {
                deleteCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new SQLiteException($"Error while deleting category of id: {id} from database: {ex.Message}");
            }
        }

        private Category? _SelectCategory(int id)
        {
            const int IDX_ID = 0, IDX_DESCRIPTION = 1, IDX_CATEGORY = 2;

            // Create the select command text
            const string selectCommandText = "SELECT Id, Description, TypeId FROM categories WHERE Id=@Id";

            // Initialize the select command with the command text and connection
            using var selectCommand = new SQLiteCommand(selectCommandText, Database.dbConnection);

            // Setup the ID parameter
            selectCommand.Parameters.Add(new SQLiteParameter("@Id", id));
            selectCommand.Prepare();

            // Execute the reader
            using SQLiteDataReader reader = selectCommand.ExecuteReader();

            // If the category was not found, just return null
            if (!reader.Read())
                return null;

            // Return the Category object
            return new Category(
                reader.GetInt32(IDX_ID),
                reader.GetString(IDX_DESCRIPTION),
                (Category.CategoryType)reader.GetInt32(IDX_CATEGORY));
        }

        private void _UpdateCategory(Category newCategory)
        {
            // Create the insert command text
            const string updateCommandText = "UPDATE categories SET Description=@Description, TypeId=@TypeId WHERE Id=@Id";

            // Initialize the update command with the command text and connection.
            using var updateCommand = new SQLiteCommand(updateCommandText, Database.dbConnection);

            // Setup parameters:
            updateCommand.Parameters.Add(new SQLiteParameter("@Id", newCategory.Id));
            updateCommand.Parameters.Add(new SQLiteParameter("@Description", newCategory.Description));
            updateCommand.Parameters.Add(new SQLiteParameter("@TypeId", newCategory.Type));
            updateCommand.Prepare();

            // Finally, execute the command
            updateCommand.ExecuteNonQuery();
        }

        private void _RemoveAll()
        {
            _DeleteAllFromTable("categories");
            _DeleteAllFromTable("categoryTypes");
        }

        private void _DeleteAllFromTable(string table)
        {
            // Create the delete command text
            string deleteCommandText = $"DELETE FROM {table}";

            // Initialize the delete command with the command text and connection
            using var deleteCommand = new SQLiteCommand(deleteCommandText, Database.dbConnection);

            // Execute the delete operation
            try
            {
                deleteCommand.ExecuteNonQuery();
            } catch (Exception ex)
            {
                throw new SQLiteException($"Error while deleting all values from table: {table} Message: {ex.Message}");
            }
        }

        private int _GetNextID()
        {
            // Create the select command
            const string selectCommandText = "SELECT MAX(Id) FROM categories";
            using var selectCommand = new SQLiteCommand(selectCommandText, Database.dbConnection);

            // Execute the reader
            using SQLiteDataReader reader = selectCommand.ExecuteReader();

            // Loop through all the reader information
            if (!reader.Read())
                return -1;

            // If MAX aggregate function returned null, return a 0 ID.
            if (reader[0].GetType() == typeof(DBNull))
                return 0;

            return reader.GetInt32(0) + 1;
        }
    }
}

