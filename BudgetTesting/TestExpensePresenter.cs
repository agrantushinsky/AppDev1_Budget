using Budget;
using Budget_WPF;
using Microsoft.Win32;
using NuGet.Frameworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetCodeTests
{
    public class TestBudgetView : IBudgetView
    {
        public bool calledRefresh = false;
        public bool calledShowCurrentFile = false;
        public bool calledShowError = false;
        public bool calledUpdateView = false;
        public bool calledOpenNewFile = false;
        public bool calledOpenExistingFile = false;
        //RESET ALL BOOLS TO FALSE IN EACH TEST
        public void SetToFalse()
        {
            calledRefresh = false;
            calledShowCurrentFile = false;
            calledShowError = false;
            calledUpdateView = false;
            calledOpenNewFile = false;
            calledOpenExistingFile = false;
        }

        public void Refresh()
        {
            calledRefresh = true;
        }

        public void ShowCurrentFile(string filename)
        {
            calledShowCurrentFile = true;
        }

        public void ShowError(string message)
        {
            calledShowError = true;
        }

        public void UpdateView(object items)
        {
            calledUpdateView = true;
        }

        public void OpenNewFile()
        {
            calledOpenNewFile = true;
        }

        public void OpenExistingFile()
        {
            calledOpenExistingFile = true;
        }

        public void ShowCategories(List<Category> categories)
        { 
            throw new NotImplementedException();
        }
    }
    public class TestExpenseView : IExpenseView
    {
        public bool calledAddCategory = false;
        public bool calledSaveExpense = false;
        public bool calledClearInputs = false;
        public bool calledRefresh = false;
        public bool calledShowError = false;
        public bool calledShowMessageWithConfirmation = false;
        public bool calledSetLastAction = false;
        public bool calledSetAddOrUpdateView = false;

        public void SetToFalse()
        {
            calledAddCategory = false;
            calledSaveExpense = false;
            calledClearInputs = false;
            calledRefresh = false;
            calledShowError = false;
            calledShowMessageWithConfirmation = false;
            calledSetLastAction = false;
            calledSetAddOrUpdateView = false;
        }

        public void AddCategory()
        {
            calledAddCategory = true;
        }

        public void SaveExpense()
        {
            calledSaveExpense = true;
        }

        public void ClearInputs()
        {
            calledClearInputs = true;
        }

        public void Refresh()
        {
            calledRefresh = true;
        }

        public void SetLastAction(string message)
        {
            calledSetLastAction = true;
        }

        public void ShowError(string message)
        {
            calledShowError = true;
        }

        public bool ShowMessageWithConfirmation(string message)
        {
            calledShowMessageWithConfirmation = true;
            return true;
        }

        public void SetAddOrUpdateView(AddOrUpdateExpense.Mode mode, Presenter presenter, BudgetItem budgetItem = null)
        {
            calledSetAddOrUpdateView = true;
        }
    }


    [Collection("Sequential")]
    public class TestExpensePresenter
    {
        public int maxIDInCategoryInFile = TestConstants.maxIDInCategoryInFile;
        public int numberOfExpensesInFile = TestConstants.numberOfExpensesInFile;
        public TestBudgetView budgetView = new TestBudgetView();

        // ========================================================================

        [Fact]
        public void PresenterConstructor()
        {
            //Arrange
            TestExpenseView ev = new TestExpenseView();

            //Act
            Presenter p = new Presenter(budgetView,ev);

            //Assert
            Assert.IsType<Presenter>(p);
        }

        // ========================================================================

        [Fact]
        public void ExpensePresenterMethods_ConnectToDatabase_Success()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(budgetView,ev);
            budgetView.SetToFalse();
            ev.SetToFalse();

            //Act
            p.ConnectToDatabase(messyDB, false);

            //Assert
            Assert.True(budgetView.calledShowCurrentFile);
            Assert.True(ev.calledSetLastAction);
            Assert.True(budgetView.calledRefresh);
            Assert.True(p.IsFileSelected());
        }

        // ========================================================================

        [Fact]
        public void PresenterMethods_ConnectToDatabse_EmptyFilename()
        {
            //Arrange
            TestExpenseView expenseView = new TestExpenseView();
            Presenter p = new Presenter(budgetView,expenseView);
            budgetView.SetToFalse();

            //Act
            p.ConnectToDatabase("", false);

            //Assert
            Assert.True(budgetView.calledShowError);
            Assert.False(budgetView.calledShowCurrentFile);
            Assert.False(budgetView.calledUpdateView);
            Assert.False(budgetView.calledRefresh);
            Assert.False(p.IsFileSelected());
        }

        // ========================================================================

        [Fact]
        public void ExpensePresenterMethods_AddCategory_Success()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(budgetView,ev);
            p.ConnectToDatabase(messyDB, false);
            int count = p.GetCategories().Count;
            string desc = "Game";
            Category.CategoryType type = Category.CategoryType.Expense;
            ev.calledSetLastAction = false;

            //Act
            p.AddCategory(desc, type);
            Category cat = p.GetCategories()[maxIDInCategoryInFile];

            //Assert
            Assert.True(ev.calledSetLastAction);
            Assert.Equal(count + 1, p.GetCategories().Count);
            Assert.Equal(desc, cat.Description);
            Assert.Equal(type, cat.Type);
        }

        // ========================================================================

        [Fact]
        public void ExpensePresenterMethods_AddCategory_InvalidDescription()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(budgetView,ev);
            p.ConnectToDatabase(messyDB, false);
            ev.calledSetLastAction = false;
            ev.calledShowError = false;

            //Act
            p.AddCategory("", Budget.Category.CategoryType.Expense);

            //Assert
            Assert.True(ev.calledShowError);
            Assert.False(ev.calledSetLastAction);
        }

        // ========================================================================

        [Fact]
        public void ExpensePresenterMethods_AddCategory_InvalidType()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(budgetView,ev);
            p.ConnectToDatabase(messyDB, false);
            ev.calledSetLastAction = false;
            ev.calledShowError = false;

            //Act
            p.AddCategory("Game", null);

            //Assert
            Assert.True(ev.calledShowError);
            Assert.False(ev.calledSetLastAction);
        }

        // ========================================================================

        [Fact]
        public void ExpensePresenterMethods_AddExpense_Success()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(budgetView,ev);
            p.ConnectToDatabase(messyDB, false);
            ev.calledSetLastAction = false;
            ev.calledClearInputs = false;
            budgetView.calledRefresh = false;

            //Act
            p.AddExpense(new DateTime(), 1, "10", "Lunch", false);
            Expense exp = p.GetExpenses()[numberOfExpensesInFile];

            //Assert
            Assert.True(ev.calledSetLastAction);
            Assert.True(ev.calledClearInputs);
            Assert.True(budgetView.calledRefresh);
            Assert.Equal(numberOfExpensesInFile + 1, p.GetExpenses().Count);
            Assert.Equal(new DateTime(), exp.Date);
            Assert.Equal(1, exp.Category);
            Assert.Equal(10, exp.Amount);
            Assert.Equal("Lunch", exp.Description);
        }

        // ========================================================================

        [Fact]
        public void ExpensePresenterMethods_AddExpense_Credit()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(budgetView,ev);
            p.ConnectToDatabase(messyDB, false);
            ev.calledSetLastAction = false;
            ev.calledClearInputs = false;
            budgetView.calledRefresh = false;

            //Act
            p.AddExpense(new DateTime(), 1, "10", "Lunch", true);
            int count = p.GetExpenses().Count;

            //Assert
            Assert.True(budgetView.calledRefresh);
            Assert.True(ev.calledSetLastAction);
            Assert.True(ev.calledClearInputs);
            Assert.Equal(numberOfExpensesInFile + 2, count);
        }

        // ========================================================================

        [Fact]
        public void ExpensePresenterMethods_AddExpense_InexistantCategory()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(budgetView,ev);
            p.ConnectToDatabase(messyDB, false);
            int count = p.GetCategories().Count;
            ev.calledSetLastAction = false;
            ev.calledShowError = false;

            //Act
            p.AddExpense(new DateTime(), count + 1, "10", "Lunch", false);

            //Assert
            Assert.True(ev.calledShowError);
            Assert.False(ev.calledSetLastAction);
        }

        // ========================================================================

        [Fact]
        public void ExpensePresenterMethods_AddExpense_InvalidDescription()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(budgetView,ev);
            p.ConnectToDatabase(messyDB, false);
            ev.calledSetLastAction = false;
            ev.calledShowError = false;

            //Act
            p.AddExpense(new DateTime(), 1, "10", "", false);

            //Assert
            Assert.True(ev.calledShowError);
            Assert.False(ev.calledSetLastAction);
        }

        // ========================================================================

        [Fact]
        public void ExpensePresenterMethods_AddExpense_InvalidAmount()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(budgetView,ev);
            p.ConnectToDatabase(messyDB, false);
            ev.calledSetLastAction = false;
            ev.calledShowError = false;

            //Act
            p.AddExpense(new DateTime(), 1, "abc", "Lunch", false);

            //Assert
            Assert.True(ev.calledShowError);
            Assert.False(ev.calledSetLastAction);
        }

        // ========================================================================

        [Fact]
        public void ExpensePresenterMethods_AddExpense_NoConnection()
        {
            //Arrange
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(budgetView,ev);
            ev.calledSetLastAction = false;
            ev.calledShowError = false;

            //Act
            p.AddExpense(new DateTime(), 1, "abc", "Lunch", false);

            //Assert
            Assert.True(ev.calledShowError);
            Assert.False(ev.calledSetLastAction);
        }

        // ========================================================================

        [Fact]
        public void ExpensePresenterMethods_UnsavedChangesCheck_ShowConfirmPopUp()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(budgetView,ev);
            p.ConnectToDatabase(messyDB, false);
            ev.calledShowMessageWithConfirmation = false;

            //Act
            p.UnsavedChangesCheck("Lunch", "");

            //Assert
            Assert.True(ev.calledShowMessageWithConfirmation);
        }

        // ========================================================================

        [Fact]
        public void ExpensePresenterMethods_UnsavedChangesCheck_PopUpDoesntShow()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(budgetView,ev);
            p.ConnectToDatabase(messyDB, false);
            ev.calledShowMessageWithConfirmation = false;

            //Act
            p.UnsavedChangesCheck("", "");
            
            //Assert
            Assert.False(ev.calledShowMessageWithConfirmation);

        }

        // ========================================================================

        [Fact]
        public void ExpensePresenterMethods_GetAndSetRecentFile_Success()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(budgetView,ev);
            p.ConnectToDatabase(messyDB, false);

            //Act
            p.SetRecentFile(messyDB);

            //Assert
            Assert.Equal(messyDB, p.GetRecentFile());
        }

        // ========================================================================

        //TO DO: Show first time user pop up in budget window rather than expense window

        //[Fact]
        //public void ExpensePresenterMethods_ShowFirstTimeUserSetup_FirstTimeUser()
        //{
        //    const string FULL_REGISTER_PATH = "SOFTWARE\\AppDevBudget\\";
        //    const string KEYNAME = "recentFile";

        //    //Arrange
        //    TestExpenseView ev = new TestExpenseView();
        //    Presenter p = new Presenter(budgetView,ev);

        //    // Delete the existing key if it exists
        //    try
        //    {
        //        RegistryKey? key = Registry.CurrentUser.OpenSubKey(FULL_REGISTER_PATH, true);
        //        key.DeleteValue(KEYNAME);
        //    } catch { }

        //    ev.calledShowMessageWithConfirmation = false;
        //    //ev.calledOpenNewFile = false;

        //    //Act
        //    //p.ShowFirstTimeUserSetup();

        //    //Assert
        //    Assert.True(ev.calledShowMessageWithConfirmation);
        //    //Assert.True(ev.calledOpenNewFile);
        //}

        //// ========================================================================

        //[Fact]
        //public void ExpensePresenterMethods_ShowFirstTimeUserSetup_ReturningUser()
        //{
        //    const string FULL_REGISTER_PATH = "SOFTWARE\\AppDevBudget\\";
        //    const string KEYNAME = "recentFile";

        //    //Arrange
        //    TestExpenseView ev = new TestExpenseView();
        //    Presenter p = new Presenter(budgetView,ev);
        //    // Wipe recent file register
        //    RegistryKey? key = Registry.CurrentUser.OpenSubKey(FULL_REGISTER_PATH, true);
        //    key.SetValue(KEYNAME, "C:\\");
        //    ev.calledShowMessageWithConfirmation = false;
        //    //ev.calledOpenNewFile = false;

        //    //Act
        //    //p.ShowFirstTimeUserSetup();

        //    //Assert
        //    Assert.False(ev.calledShowMessageWithConfirmation);
        //    //Assert.False(ev.calledOpenNewFile);

        //    // Cleanup (delete bad key)
        //    key.DeleteValue(KEYNAME);
        //}

        // ========================================================================

        [Fact]
        public void ExpensePresenterMethods_IsFileSelected_FalseState()
        {
            //Arrange
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(budgetView,ev);
            ev.calledShowError = false;
            bool isSelected;

            //Act
            isSelected = p.IsFileSelected();

            //Assert
            Assert.True(ev.calledShowError);
            Assert.False(isSelected);
        }

        // ========================================================================

        [Fact]
        public void ExpensePresenterMethods_IsFileSelected_TrueState()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(budgetView,ev);
            p.ConnectToDatabase(messyDB, false);
            ev.calledShowError = false;
            bool isSelected;

            //Act
            isSelected = p.IsFileSelected();

            //Assert
            Assert.False(ev.calledShowError);
            Assert.True(isSelected);
        }

        // ========================================================================

        [Fact]
        public void ExpensePresenterMethods_GetCategories_Success()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(budgetView,ev);
            p.ConnectToDatabase(messyDB, false);
            int categoryCount;
            const int EXPECTED_COUNT = 17;

            //Act
            categoryCount = p.GetCategories().Count;

            //Assert
            Assert.Equal(EXPECTED_COUNT, categoryCount);
        }

        // ========================================================================

        [Fact]
        public void ExpensePresenterMethods_GetExpenses_Success()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(budgetView, ev);
            p.ConnectToDatabase(messyDB, false);
            int expenseCount;

            //Act
            expenseCount = p.GetExpenses().Count;

            //Assert
            Assert.Equal(numberOfExpensesInFile, expenseCount);
        }

        // ========================================================================

        [Fact]
        public void ExpensePresenterMethods_UpdateExpense_Success()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(budgetView, ev);
            p.ConnectToDatabase(messyDB, false);
            ev.SetToFalse();
            budgetView.SetToFalse();
            Expense firstExpenseInFile = p.GetExpenses().First();

            //Act
            p.UpdateExpense(firstExpenseInFile.Id, firstExpenseInFile.Date, firstExpenseInFile.Category, firstExpenseInFile.Amount.ToString(), "Updated Item");
            firstExpenseInFile = p.GetExpenses().First();

            //Assert
            Assert.True(budgetView.calledRefresh);
            Assert.True(ev.calledSetLastAction);
            Assert.True(ev.calledClearInputs);
            Assert.Equal("Updated Item", firstExpenseInFile.Description);
        }

        // ========================================================================

        [Fact]
        public void ExpensePresenterMethods_UpdateExpense_InvalidAmount()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(budgetView, ev);
            p.ConnectToDatabase(messyDB, false);
            ev.SetToFalse();
            Expense firstExpenseInFile = p.GetExpenses().First();

            //Act
            p.UpdateExpense(firstExpenseInFile.Id, firstExpenseInFile.Date, firstExpenseInFile.Category, "abc", "Updated Item");

            //Assert
            Assert.True(ev.calledShowError);
            Assert.False(ev.calledSetLastAction);
        }

        // ========================================================================

        [Fact]
        public void ExpensePresenterMethods_UpdateExpense_InvalidCategory()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(budgetView, ev);
            p.ConnectToDatabase(messyDB, false);
            ev.SetToFalse();
            Expense firstExpenseInFile = p.GetExpenses().First();

            //Act
            p.UpdateExpense(firstExpenseInFile.Id, firstExpenseInFile.Date, -1, firstExpenseInFile.Amount.ToString(), "Updated Item");

            //Assert
            Assert.True(ev.calledShowError);
            Assert.False(ev.calledSetLastAction);
        }

        // ========================================================================

        [Fact]
        public void ExpensePresenterMethods_DeleteExpense_Success()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(budgetView, ev);
            p.ConnectToDatabase(messyDB, false);
            ev.SetToFalse();
            budgetView.SetToFalse();
            Expense firstExpenseInFile = p.GetExpenses().First();
            int count = p.GetExpenses().Count();

            //Act
            p.DeleteExpense(firstExpenseInFile.Id, firstExpenseInFile.Description);

            //Assert
            Assert.True(budgetView.calledRefresh);
            Assert.True(ev.calledSetLastAction);
            Assert.True(ev.calledClearInputs);
            Assert.Equal(count - 1, p.GetExpenses().Count());
        }
    }
}
