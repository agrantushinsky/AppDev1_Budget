using System;
using Xunit;
using System.IO;
using System.Collections.Generic;
using Budget;
using System.Data.SQLite;
using NuGet.Frameworks;

namespace BudgetCodeTests
{
    [Collection("Sequential")]
    public class TestExpenses
    {
        public int numberOfExpensesInFile = TestConstants.numberOfExpensesInFile;
        public String testInputFile = TestConstants.testDBInputFile;
        public int maxIDInExpenseInFile = TestConstants.maxIDInExpenseFile;
        public int maxIDInCategoryInFile = TestConstants.maxIDInCategoryInFile;
        public Expense firstExpenseInFile = TestConstants.firstExpenseInFile;

        [Fact]
        public void ExpensesObject_New()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String newDB = $"{folder}\\newDB.db";
            Database.newDatabase(newDB);
            SQLiteConnection conn = Database.dbConnection;

            //Act
            Expenses expenses = new Expenses();

            //Assert
            Assert.IsType<Expenses>(expenses);
        }

        [Fact]
        public void ExpensesMethod_ReadFromNewDatabase_ListIsEmpty()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String newDB = $"{folder}\\newDB.db";
            Database.newDatabase(newDB);
            SQLiteConnection conn = Database.dbConnection;

            //Act
            Expenses expenses = new Expenses();
            List<Expense> list = expenses.List();

            //Assert
            Assert.Empty(list);
        }

        [Fact]
        public void ExpensesMethod_ReadFromExistingDatabase_ValidateCorrectDataWasRead()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            Database.existingDatabase(messyDB);
            SQLiteConnection conn = Database.dbConnection;

            //Act
            Expenses expenses = new Expenses();
            List<Expense> list = expenses.List();
            Expense firstExpense = list[0];

            //Assert
            Assert.Equal(numberOfExpensesInFile, list.Count);
            Assert.Equal(firstExpenseInFile.Id, firstExpense.Id);
            Assert.Equal(firstExpenseInFile.Date, firstExpense.Date);
            Assert.Equal(firstExpenseInFile.Category, firstExpense.Category);
            Assert.Equal(firstExpenseInFile.Amount, firstExpense.Amount);
            Assert.Equal(firstExpenseInFile.Description, firstExpense.Description);
        }


        [Fact]
        public void ExpensesMethod_Add()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            Database.existingDatabase(messyDB);
            SQLiteConnection conn = Database.dbConnection;
            Expenses expenses = new Expenses();
            DateTime date = new DateTime(2023,2,28);
            int catId = 3;
            Double amount = 10;
            string description = "Lunch";

            //Act
            expenses.Add(date, catId, amount, description);
            List<Expense> expensesList = expenses.List();
            int sizeOfList = expensesList.Count;

            //Assert
            Assert.Equal(numberOfExpensesInFile + 1, sizeOfList);
            Assert.Equal(date, expensesList[sizeOfList - 1].Date);
            Assert.Equal(catId, expensesList[sizeOfList - 1].Category);
            Assert.Equal(amount, expensesList[sizeOfList - 1].Amount);
            Assert.Equal(description, expensesList[sizeOfList - 1].Description);
        }

        [Fact]
        public void ExpensesMethod_Add_InvalidCategoryIDNotAccepted()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            Database.existingDatabase(messyDB);
            SQLiteConnection conn = Database.dbConnection;
            Expenses expenses = new Expenses();
            DateTime date = new DateTime(2023, 2, 28);
            int catId = maxIDInCategoryInFile + 1;
            Double amount = 10;
            string description = "Lunch";

            //Act + Assert
            Assert.Throws<ArgumentException>(() =>
            {
                expenses.Add(date, catId, amount, description);
            });
        }

        [Fact]
        public void ExpensesMethod_GetExpenseFromId()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            Database.existingDatabase(messyDB);
            SQLiteConnection conn = Database.dbConnection;
            Expenses expenses = new Expenses();
            int expId = 1;

            //Act
            Expense exp = expenses.GetExpenseFromId(expId);
            
            //Assert
            Assert.Equal(expId, exp.Id);
        }

        [Fact]
        public void ExpensesMethod_GetExpenseFromId_IDDoesNotExist()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            Database.existingDatabase(messyDB);
            SQLiteConnection conn = Database.dbConnection;
            Expenses expenses = new Expenses();
            int expId = maxIDInExpenseInFile + 1;

            Assert.Throws<Exception>(() =>
            {
                Expense exp = expenses.GetExpenseFromId(expId);
            });
        }

        [Fact]
        public void ExpensesMethod_List_ReturnListOfExpenses()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            Database.existingDatabase(messyDB);
            SQLiteConnection conn = Database.dbConnection;
            Expenses expenses = new Expenses();

            //Act
            List<Expense> list = expenses.List();

            //Assert
            Assert.Equal(numberOfExpensesInFile, list.Count);
        }

        [Fact]
        public void ExpensesMethod_List_OrderByID()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            Database.existingDatabase(messyDB);
            SQLiteConnection conn = Database.dbConnection;
            Expenses expenses = new Expenses();
            bool success = true;

            //Act
            List<Expense> list = expenses.List();
            
            for(int count = 0; count < list.Count - 1; count++)
            {
                if (list[count].Id > list[count + 1].Id)
                {
                    success = false; 
                    break;
                }
            }

            //Assert
            Assert.True(success);
        }

        [Fact]
        public void ExpensesMethod_Update()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            Database.existingDatabase(messyDB);
            SQLiteConnection conn = Database.dbConnection;
            Expenses expenses = new Expenses();

            DateTime newDate = new DateTime(2023, 2, 28);
            int newCatId = 3;
            Double newAmount = 10;
            string newDescription = "Lunch";

            //Act
            expenses.Update(maxIDInExpenseInFile, newDate, newCatId, newAmount, newDescription);
            Expense exp = expenses.GetExpenseFromId(maxIDInExpenseInFile);

            //Assert
            Assert.Equal(newDate, exp.Date);
            Assert.Equal(newCatId, exp.Category);
            Assert.Equal(newAmount, exp.Amount);
            Assert.Equal(newDescription, exp.Description);

        }

        [Fact]
        public void ExpensesMethod_Update_IDDoesNotExist()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            Database.existingDatabase(messyDB);
            SQLiteConnection conn = Database.dbConnection;
            Expenses expenses = new Expenses();

            DateTime newDate = new DateTime(2023, 2, 28);
            int newCatId = 3;
            Double newAmount = 10;
            string newDescription = "Lunch";

            //Act + Assert
            Assert.Throws<ArgumentException>(() =>
            {
                expenses.Update(maxIDInExpenseInFile + 1, newDate, newCatId, newAmount, newDescription);
            });
        }

        [Fact]
        public void ExpensesMethod_Delete()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            Database.existingDatabase(messyDB);
            SQLiteConnection conn = Database.dbConnection;
            Expenses expenses = new Expenses();
            int idToDelete = maxIDInExpenseInFile;

            //Act
            expenses.Delete(idToDelete);
            List<Expense> list = expenses.List();
            int sizeList = list.Count();

            //Assert
            Assert.Equal(sizeList, numberOfExpensesInFile - 1);
            Assert.False(list.Exists(e => e.Id == idToDelete), "Correct expense item deleted");
        }

        [Fact]
        public void ExpensesMethod_Delete_InvalidIDDoesntCrash()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            Database.existingDatabase(messyDB);
            SQLiteConnection conn = Database.dbConnection;
            Expenses expenses = new Expenses();
            int idToDelete = maxIDInExpenseInFile + 1;

            //Act
            try
            {
                expenses.Delete(idToDelete);
                List<Expense> list = expenses.List();
                int sizeList = list.Count();
                Assert.Equal(sizeList, numberOfExpensesInFile);
            }
            //Assert
            catch
            {
                Assert.True(false, "Invalid ID causes Delete to break");

            }
        }

        [Fact]
        public void ExpensesMethod_DeleteAllExpenses()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            Database.existingDatabase(messyDB);
            SQLiteConnection conn = Database.dbConnection;
            Expenses expenses = new Expenses();

            //Act
            expenses.DeleteAllExpenses();
            List<Expense> list = expenses.List();

            //Assert
            Assert.Empty(list);
        }

    }
}
