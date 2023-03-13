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

        // ====================================================================
        // Constructor
        // ====================================================================
        /// <summary>
        /// Creates an instance of Categories
        /// </summary>
        public Categories()
        {

        }

        /// <summary>
        /// Creates an instance of Categories and sets the categories to default is newDatabase is true.
        /// This constructor is mainly used for testing.
        /// </summary>
        /// <param name="conn">Connection to database.</param>
        /// <param name="newDatabase">Boolean whether to reset categories to defaults or not.</param>
        /// <exception cref="SQLiteException">Thrown when the connection has not been initialized.</exception>
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
        /// <summary>
        /// Gets the Category object with the provided ID
        /// </summary>
        /// <param name="id">ID of the Category item to get</param>
        /// <returns>Category object</returns>
        /// <exception cref="Exception">If the ID does not exist</exception>
        /// <exception cref="SQLiteException">Thrown when an SQLite error occurs.</exception>
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
        /// <summary>
        /// Clears the stored categories and categoryTypes, then, adds the defaults. Refer to examples for defaults.
        /// </summary>
        /// <exception cref="SQLiteException">Thrown when the database connection has not been initialized yet or an SQLite error occured.</exception>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Categories categories = new Categories();
        /// 
        /// // Set defaults (it is worth noting that the default constructor calls this method)
        /// categories.SetCategoriesToDefaults();
        ///
        /// foreach (Category category in categories.List())
        ///     Console.WriteLine($"{category.Description}: {category.Type}");
        /// // Output:
        /// // Utilities: Expense
        /// // Rent: Expense
        /// // Food: Expense
        /// // Entertainment: Expense
        /// // Education: Expense
        /// // Miscellaneous: Expense
        /// // Medical Expenses: Expense
        /// // Vacation: Expense
        /// // Credit Card: Credit
        /// // Clothes: Expense
        /// // Gifts: Expense
        /// // Insurance: Expense
        /// // Transportation: Expense
        /// // Eating Out: Expense
        /// // Savings: Savings
        /// // Income: Income
        /// ]]>
        /// </code>
        /// </example>
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
        /// <summary>
        /// Stores the category using the information provided by the user.
        /// </summary>
        /// <param name="desc">The new Category's description.</param>
        /// <param name="type">The new Category's type.</param>
        /// <exception cref="SQLiteException">Thrown when an SQLite error occurs.</exception>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// // New instance of Categories. Defaults will be loaded as well.
        /// Categories categories = new Categories();
        ///
        /// // Add new a categories
        /// categories.Add("Streaming Services", Category.CategoryType.Expense);
        /// categories.Add("Fitness", Category.CategoryType.Expense);
        ///
        /// // View new categories
        /// foreach (Category category in categories.List())
        ///     Console.WriteLine(category.Description);
        /// ]]>
        /// </code>
        /// </example>
        public void Add(String desc, Category.CategoryType type)
        {
            _InsertCategory(desc, type);
        }

        // ====================================================================
        // Delete category
        // ====================================================================
        /// <summary>
        /// Deletes a Category using the Id provided.
        /// </summary>
        /// <param name="Id">The Id of the category to remove from the database.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the provided Id is negative or is greater than or equal to the count.</exception>
        /// <exception cref="SQLiteException">Thrown when an SQLite error occurs.</exception>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// // New instance of Categories. Defaults will be loaded as well.
        /// Categories categories = new Categories();
        ///
        /// // Add new a category
        /// categories.Add("Streaming Services", Category.CategoryType.Expense);
        ///
        /// // Remove at Id 1
        /// categories.Delete(1);
        ///
        /// // Attempt to remove again:
        /// categories.Delete(1);
        /// // Exception ArgumentOutOfRangeException is thrown.
        /// ]]>
        /// </code>
        /// </example>
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
        /// Gets a list of Categories from the database.
        /// </summary>
        /// <returns>A new List containing the stored categories.</returns>
        /// <exception cref="SQLiteException">Thrown when an SQLite error occurs.</exception>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Categories categories = new Categories();
        ///
        /// // Add new a category
        /// categories.Add("Online Subscription", Category.CategoryType.Expense);
        ///
        /// // Print categories using List()
        /// List<Expense> list = categories.List();
        /// foreach (Category category in list)
        ///     Console.WriteLine(category.Description);
        /// ]]>
        /// </code>
        /// </example>
        public List<Category> List()
        {
            return _GetCategories();
        }

        /// <summary>
        /// Finds the category in the database and replaces it with the passed category data.
        /// </summary>
        /// <param name="id">The id of the category to replace.</param>
        /// <param name="newDescr">The description of the new category.</param>
        /// <param name="type">The CategoryType of the new category.</param>
        /// <exception cref="SQLiteException">Thrown when an SQLite error occurs.</exception>
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
            const string insertCommandText = "INSERT INTO categories(Description, TypeId) VALUES(@Description, @TypeId)";

            // Initialize the insert command with the command text and connection.
            using var insertCommand = new SQLiteCommand(insertCommandText, Database.dbConnection);

            // Setup parameters:
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
            _TruncateTable("categories");
            _TruncateTable("categoryTypes");
        }

        private void _TruncateTable(string table)
        {
            // When using DELETE FROM <table>, sqlite will actually truncate the table.
            // There is no truncate table in sqlite3.
            // Source: https://www.tutorialspoint.com/sqlite/sqlite_truncate_table.htm

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
    }
}

