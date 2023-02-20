using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Threading;
using System.Data.Entity.Infrastructure;

// ===================================================================
// Very important notes:
// ... To keep everything working smoothly, you should always
//     dispose of EVERY SQLiteCommand even if you recycle a 
//     SQLiteCommand variable later on.
//     EXAMPLE:
//            Database.newDatabase(GetSolutionDir() + "\\" + filename);
//            var cmd = new SQLiteCommand(Database.dbConnection);
//            cmd.CommandText = "INSERT INTO categoryTypes(Description) VALUES('Whatever')";
//            cmd.ExecuteNonQuery();
//            cmd.Dispose();
//
// ... also dispose of reader objects
//
// ... by default, SQLite does not impose Foreign Key Restraints
//     so to add these constraints, connect to SQLite something like this:
//            string cs = $"Data Source=abc.sqlite; Foreign Keys=1";
//            var con = new SQLiteConnection(cs);
//
// ===================================================================


namespace Budget
{
    public class Database
    {

        public static SQLiteConnection dbConnection { get { return _connection; } }
        private static SQLiteConnection _connection;

        // ===================================================================
        // create and open a new database
        // ===================================================================
        public static void newDatabase(string filename)
        {
            // Open connection to the database:
            _OpenConnection(filename);

            // Next, create the tables:
            const string CREATE_TABLES_COMMAND = @"
                    DROP TABLE IF EXISTS expenses;
                    DROP TABLE IF EXISTS categories;
                    DROP TABLE IF EXISTS categoryTypes;

                    CREATE TABLE expenses (
                        Id INTEGER PRIMARY KEY NOT NULL IDENTITY(0, 1),
                        Date TEXT NOT NULL,
                        Description TEXT NOT NULL,
                        Amount REAL NOT NULL,
                        CategoryId INTEGER NOT NULL,

                        FOREIGN KEY (CategoryId) REFERENCES categories(Id)
                    );

                    CREATE TABLE categories (
                        Id INTEGER PRIMARY KEY NOT NULL IDENTITY(0, 1),
                        Description TEXT NOT NULL,
                        TypeId INTEGER NOT NULL,

                        FOREIGN KEY (TypeId) REFERENCES categoryTypes(Id)
                    );

                    CREATE TABLE categoryTypes (
                        Id INTEGER PRIMARY KEY NOT NULL,
                        Description TEXT NOT NULL
                    );
                ";

            // Create, execute, and dispose of command for table creation:
            var createTablesCmd = new SQLiteCommand(dbConnection);
            createTablesCmd.CommandText = CREATE_TABLES_COMMAND;
            createTablesCmd.ExecuteNonQuery();
            createTablesCmd.Dispose();
        }

       // ===================================================================
       // open an existing database
       // ===================================================================
       public static void existingDatabase(string filename)
        {
            // If the file doesn't exist, throw an exception:
            if (!File.Exists(filename))
                throw new FileNotFoundException($"File \"{filename}\" does not exist.");

            // Open the database connection
            _OpenConnection(filename);
        }

       // ===================================================================
       // close existing database, wait for garbage collector to
       // release the lock before continuing
       // ===================================================================
        static public void CloseDatabaseAndReleaseFile()
        {
            if (Database.dbConnection != null)
            {
                // close the database connection
                Database.dbConnection.Close();
                

                // wait for the garbage collector to remove the
                // lock from the database file
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        private static void _OpenConnection(string filename)
        {
            // If there was a database open before, close it and release the lock
            CloseDatabaseAndReleaseFile();

            // Open connection to the database file with foreign keys enabled:
            string connectionSource = @$"URI=file:{filename}; Foreign Keys=1";
            _connection = new SQLiteConnection(connectionSource);
            _connection.Open();
        }
    }

}
