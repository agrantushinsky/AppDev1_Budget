using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Data.SQLite;
using System.Configuration.Internal;

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
        private List<Expense> _Expenses = new List<Expense>();
        private string _FileName;
        private string _DirName;

        // ====================================================================
        // Properties
        // ====================================================================
        public String FileName { get { return _FileName; } }
        public String DirName { get { return _DirName; } }

        // ====================================================================
        // populate categories from a file
        // if filepath is not specified, read/save in AppData file
        // Throws System.IO.FileNotFoundException if file does not exist
        // Throws System.Exception if cannot read the file correctly (parsing XML)
        // ====================================================================
        public void ReadFromFile(String filepath = null)
        {

            // ---------------------------------------------------------------
            // reading from file resets all the current expenses,
            // so clear out any old definitions
            // ---------------------------------------------------------------
            _Expenses.Clear();

            // ---------------------------------------------------------------
            // reset default dir/filename to null 
            // ... filepath may not be valid, 
            // ---------------------------------------------------------------
            _DirName = null;
            _FileName = null;

            // ---------------------------------------------------------------
            // get filepath name (throws exception if it doesn't exist)
            // ---------------------------------------------------------------
            filepath = BudgetFiles.VerifyReadFromFileName(filepath);

            // ---------------------------------------------------------------
            // read the expenses from the xml file
            // ---------------------------------------------------------------
            _ReadXMLFile(filepath);

            // ----------------------------------------------------------------
            // save filename info for later use?
            // ----------------------------------------------------------------
            _DirName = Path.GetDirectoryName(filepath);
            _FileName = Path.GetFileName(filepath);


        }

        // ====================================================================
        // save to a file
        // if filepath is not specified, throws an exception
        // ====================================================================
        public void SaveToFile(String filepath = null)
        {
            // ---------------------------------------------------------------
            // if file path not specified, set to last read file
            // ---------------------------------------------------------------
            if (filepath == null && DirName != null && FileName != null)
            {
                filepath = DirName + "\\" + FileName;
            }

            // ---------------------------------------------------------------
            // just in case filepath doesn't exist, reset path info
            // ---------------------------------------------------------------
            _DirName = null;
            _FileName = null;

            // ---------------------------------------------------------------
            // get filepath name (throws exception if it's null and read only)
            // ---------------------------------------------------------------
            filepath = BudgetFiles.VerifyWriteToFileName(filepath);

            // ---------------------------------------------------------------
            // save as XML
            // ---------------------------------------------------------------
            _WriteXMLFile(filepath);

            // ----------------------------------------------------------------
            // save filename info for later use
            // ----------------------------------------------------------------
            _DirName = Path.GetDirectoryName(filepath);
            _FileName = Path.GetFileName(filepath);
        }


        public void Add(DateTime date, int category, Double amount, String description)
        {
            _InsertExpense(date, category, amount, description);

        }

        private void _InsertExpense(DateTime date, int category, Double amount, String description)
        {
            const string insertCommandText = "INSERT INTO expenses(Date, CategoryId, Amount, Description) VALUE" +
                "(@Date, @CategoryId, @Amount, @Description)";

            using var insertCommand = new SQLiteCommand(insertCommandText, Database.dbConnection);

            insertCommand.Parameters.Add(new SQLiteParameter("@Date", date));
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

        private void _DeleteAllExpenses()
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
            const string updateCommandText = "UPDATE expenses SET date = @Date, category = @Category, Amount = @Amount, Description = @Description WHERE Id = @ID";

            using var updateCommand = new SQLiteCommand(updateCommandText, Database.dbConnection);

            updateCommand.Parameters.Add(new SQLiteParameter("@Id", id));
            updateCommand.Parameters.Add(new SQLiteParameter("@Date", newDate));
            updateCommand.Parameters.Add(new SQLiteParameter("@Category", newCategory));
            updateCommand.Parameters.Add(new SQLiteParameter("@Amount", newAmount));
            updateCommand.Parameters.Add(new SQLiteParameter("@Description", newDesptiption));
            updateCommand.Prepare();

            updateCommand.ExecuteNonQuery();
        }

        public List<Expense> List()
        {
            List<Expense> newList = new List<Expense>();
            foreach (Expense expense in _Expenses)
            {
                newList.Add(new Expense(expense));
            }
            return newList;
        }

        public List<Expense> _GetExpenses()
        {
            List<Expense> expenses = new List<Expense>();

            const int IDX_ID = 0, IDX_DATE = 1, IDX_AMOUNT = 2, IDX_DESCRIPTION = 3, IDX_CATEGORY = 4;

            const string selectCommandText = "SELECT Id, Date, Amount, Description, Category FROM expenses ORDERBY Id";
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


        // ====================================================================
        // read from an XML file and add categories to our categories list
        // ====================================================================
        private void _ReadXMLFile(String filepath)
        {


            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(filepath);

                // Loop over each Expense
                foreach (XmlNode expense in doc.DocumentElement.ChildNodes)
                {
                    // set default expense parameters
                    int id = int.Parse((((XmlElement)expense).GetAttributeNode("ID")).InnerText);
                    String description = "";
                    DateTime date = DateTime.Parse("2000-01-01");
                    int category = 0;
                    Double amount = 0.0;

                    // get expense parameters
                    foreach (XmlNode info in expense.ChildNodes)
                    {
                        switch (info.Name)
                        {
                            case "Date":
                                date = DateTime.Parse(info.InnerText);
                                break;
                            case "Amount":
                                amount = Double.Parse(info.InnerText);
                                break;
                            case "Description":
                                description = info.InnerText;
                                break;
                            case "Category":
                                category = int.Parse(info.InnerText);
                                break;
                        }
                    }

                    // have all info for expense, so create new one
                    this.Add(new Expense(id, date, category, amount, description));

                }

            }
            catch (Exception e)
            {
                throw new Exception("ReadFromFileException: Reading XML " + e.Message);
            }
        }


        // ====================================================================
        // write to an XML file
        // if filepath is not specified, read/save in AppData file
        // ====================================================================
        private void _WriteXMLFile(String filepath)
        {
            // ---------------------------------------------------------------
            // loop over all categories and write them out as XML
            // ---------------------------------------------------------------
            try
            {
                // create top level element of expenses
                XmlDocument doc = new XmlDocument();
                doc.LoadXml("<Expenses></Expenses>");

                // foreach Category, create an new xml element
                foreach (Expense exp in _Expenses)
                {
                    // main element 'Expense' with attribute ID
                    XmlElement ele = doc.CreateElement("Expense");
                    XmlAttribute attr = doc.CreateAttribute("ID");
                    attr.Value = exp.Id.ToString();
                    ele.SetAttributeNode(attr);
                    doc.DocumentElement.AppendChild(ele);

                    // child attributes (date, description, amount, category)
                    XmlElement d = doc.CreateElement("Date");
                    XmlText dText = doc.CreateTextNode(exp.Date.ToString("M/dd/yyyy hh:mm:ss tt"));
                    ele.AppendChild(d);
                    d.AppendChild(dText);

                    XmlElement de = doc.CreateElement("Description");
                    XmlText deText = doc.CreateTextNode(exp.Description);
                    ele.AppendChild(de);
                    de.AppendChild(deText);

                    XmlElement a = doc.CreateElement("Amount");
                    XmlText aText = doc.CreateTextNode(exp.Amount.ToString());
                    ele.AppendChild(a);
                    a.AppendChild(aText);

                    XmlElement c = doc.CreateElement("Category");
                    XmlText cText = doc.CreateTextNode(exp.Category.ToString());
                    ele.AppendChild(c);
                    c.AppendChild(cText);

                }

                // write the xml to FilePath
                doc.Save(filepath);

            }
            catch (Exception e)
            {
                throw new Exception("SaveToFileException: Reading XML " + e.Message);
            }
        }

     

    }
}

