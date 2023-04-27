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
        public bool calledShowCategories = false;
        public bool calledShowMessageWithConfirmation = false;
        public object? listItems;

        //RESET ALL BOOLS TO FALSE IN EACH TEST
        public void SetToFalse()
        {
            calledRefresh = false;
            calledShowCurrentFile = false;
            calledShowError = false;
            calledUpdateView = false;
            calledOpenNewFile = false;
            calledOpenExistingFile = false;
            calledShowCategories = false;
            calledShowMessageWithConfirmation = false;
            listItems = null;
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
            listItems = items;
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
            calledShowCategories = true;
        }

        public bool ShowMessageWithConfirmation(string message)
        {
            calledShowMessageWithConfirmation = true;
            return true;
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
    public class TestPresenter
    {
        public int maxIDInCategoryInFile = TestConstants.maxIDInCategoryInFile;
        public int numberOfExpensesInFile = TestConstants.numberOfExpensesInFile;
        public TestBudgetView budgetView = new TestBudgetView();

        // ========================================================================

        [Fact]
        public void PresenterConstructor()
        {
            //Arrange
            TestBudgetView bv = new TestBudgetView();

            //Act
            Presenter p = new Presenter(bv);

            //Assert
            Assert.IsType<Presenter>(p);
        }

        // ========================================================================

        [Fact]
        public void PresenterMethods_ConnectToDatabase_Success()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            Presenter p = new Presenter(budgetView);
            budgetView.SetToFalse();

            //Act
            p.ConnectToDatabase(messyDB, false);

            //Assert
            Assert.True(budgetView.calledShowCurrentFile);
            Assert.True(budgetView.calledRefresh);
            Assert.True(budgetView.calledShowCategories);
            Assert.True(p.IsFileSelected());
        }

        // ========================================================================

        [Fact]
        public void PresenterMethods_ConnectToDatabse_EmptyFilename()
        {
            //Arrange
            Presenter p = new Presenter(budgetView);
            budgetView.SetToFalse();

            //Act
            p.ConnectToDatabase("", false);

            //Assert
            Assert.True(budgetView.calledShowError);
            Assert.False(budgetView.calledShowCurrentFile);
            Assert.False(budgetView.calledUpdateView);
            Assert.False(budgetView.calledRefresh);
        }

        // ========================================================================

        [Fact]
        public void PresenterMethods_AddCategory_Success()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(budgetView);
            p.expenseView = ev;
            p.ConnectToDatabase(messyDB, false);
            int count = p.GetCategories().Count;
            string desc = "Game";
            Category.CategoryType type = Category.CategoryType.Expense;
            ev.SetToFalse();
            budgetView.calledShowCategories = false;

            //Act
            p.AddCategory(desc, type);
            Category cat = p.GetCategories()[maxIDInCategoryInFile];

            //Assert
            Assert.True(ev.calledSetLastAction);
            Assert.True(budgetView.calledShowCategories);
            Assert.Equal(count + 1, p.GetCategories().Count);
            Assert.Equal(desc, cat.Description);
            Assert.Equal(type, cat.Type);
        }

        // ========================================================================

        [Fact]
        public void PresenterMethods_AddCategory_InvalidDescription()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(budgetView);
            p.expenseView = ev;
            p.ConnectToDatabase(messyDB, false);
            ev.SetToFalse();
            budgetView.calledShowCategories = false;

            //Act
            p.AddCategory("", Budget.Category.CategoryType.Expense);

            //Assert
            Assert.True(ev.calledShowError);
            Assert.False(ev.calledSetLastAction);
            Assert.False(budgetView.calledShowCategories);
        }

        // ========================================================================

        [Fact]
        public void PresenterMethods_AddCategory_InvalidType()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(budgetView);
            p.expenseView = ev;
            p.ConnectToDatabase(messyDB, false);
            ev.calledSetLastAction = false;
            ev.calledShowError = false;
            budgetView.calledShowCategories = false;

            //Act
            p.AddCategory("Game", null);

            //Assert
            Assert.True(ev.calledShowError);
            Assert.False(ev.calledSetLastAction);
            Assert.False(budgetView.calledShowCategories);
        }

        // ========================================================================

        [Fact]
        public void PresenterMethods_AddExpense_Success()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(budgetView);
            p.expenseView = ev;
            p.ConnectToDatabase(messyDB, false);
            ev.SetToFalse();
            budgetView.SetToFalse();

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
        public void PresenterMethods_AddExpense_Credit()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(budgetView);
            p.expenseView = ev;
            p.ConnectToDatabase(messyDB, false);
            ev.SetToFalse();
            budgetView.SetToFalse();

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
        public void PresenterMethods_AddExpense_InexistentCategory()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(budgetView);
            p.expenseView = ev;
            p.ConnectToDatabase(messyDB, false);
            int count = p.GetCategories().Count;
            ev.SetToFalse();
            budgetView.SetToFalse();

            //Act
            p.AddExpense(new DateTime(), count + 1, "10", "Lunch", false);

            //Assert
            Assert.True(ev.calledShowError);
            Assert.False(ev.calledSetLastAction);
            Assert.False(budgetView.calledRefresh);
        }

        // ========================================================================

        [Fact]
        public void PresenterMethods_AddExpense_InvalidDescription()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(budgetView);
            p.expenseView = ev;
            p.ConnectToDatabase(messyDB, false);
            ev.SetToFalse();
            budgetView.SetToFalse();

            //Act
            p.AddExpense(new DateTime(), 1, "10", "", false);

            //Assert
            Assert.True(ev.calledShowError);
            Assert.False(ev.calledSetLastAction);
            Assert.False(budgetView.calledRefresh);
        }

        // ========================================================================

        [Fact]
        public void PresenterMethods_AddExpense_InvalidAmount()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(budgetView);
            p.expenseView = ev;
            p.ConnectToDatabase(messyDB, false);
            ev.SetToFalse();
            budgetView.SetToFalse();

            //Act
            p.AddExpense(new DateTime(), 1, "abc", "Lunch", false);

            //Assert
            Assert.True(ev.calledShowError);
            Assert.False(ev.calledSetLastAction);
            Assert.False(budgetView.calledRefresh);
        }

        // ========================================================================

        [Fact]
        public void PresenterMethods_AddExpense_NoConnection()
        {
            //Arrange
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(budgetView);
            p.expenseView = ev;
            ev.SetToFalse();
            budgetView.SetToFalse();

            //Act
            p.AddExpense(new DateTime(), 1, "abc", "Lunch", false);

            //Assert
            Assert.True(ev.calledShowError);
            Assert.False(ev.calledSetLastAction);
            Assert.False(budgetView.calledRefresh);
        }

        // ========================================================================

        [Fact]
        public void PresenterMethods_UnsavedChangesCheck_ShowConfirmPopUp()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(budgetView);
            p.expenseView = ev;
            p.ConnectToDatabase(messyDB, false);
            ev.SetToFalse();

            //Act
            p.UnsavedChangesCheck("Lunch", "");

            //Assert
            Assert.True(ev.calledShowMessageWithConfirmation);
        }

        // ========================================================================

        [Fact]
        public void PresenterMethods_UnsavedChangesCheck_PopUpDoesntShow()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(budgetView);
            p.expenseView = ev;
            p.ConnectToDatabase(messyDB, false);
            ev.SetToFalse();

            //Act
            p.UnsavedChangesCheck("", "");
            
            //Assert
            Assert.False(ev.calledShowMessageWithConfirmation);

        }

        // ========================================================================

        [Fact]
        public void PresenterMethods_GetAndSetRecentFile_Success()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(budgetView);
            p.expenseView = ev;
            p.ConnectToDatabase(messyDB, false);

            //Act
            p.SetRecentFile(messyDB);

            //Assert
            Assert.Equal(messyDB, p.GetRecentFile());
        }

        // ========================================================================

        [Fact]
        public void PresenterMethods_ShowFirstTimeUserSetup_FirstTimeUser()
        {
            const string FULL_REGISTER_PATH = "SOFTWARE\\AppDevBudget\\";
            const string KEYNAME = "recentFile";

            //Arrange
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(budgetView);
            p.expenseView = ev;

            // Delete the existing key if it exists
            try
            {
                RegistryKey? key = Registry.CurrentUser.OpenSubKey(FULL_REGISTER_PATH, true);
                key.DeleteValue(KEYNAME);
            }
            catch { }

            budgetView.SetToFalse();

            //Act
            p.ShowFirstTimeUserSetup();

            //Assert
            Assert.True(budgetView.calledShowMessageWithConfirmation);
            Assert.True(budgetView.calledOpenNewFile);
        }

        // ========================================================================

        [Fact]
        public void PresenterMethods_ShowFirstTimeUserSetup_ReturningUser()
        {
            const string FULL_REGISTER_PATH = "SOFTWARE\\AppDevBudget\\";
            const string KEYNAME = "recentFile";

            //Arrange
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(budgetView);
            p.expenseView = ev;
            // Wipe recent file register
            RegistryKey? key = Registry.CurrentUser.OpenSubKey(FULL_REGISTER_PATH, true);
            key.SetValue(KEYNAME, "C:\\");
            budgetView.SetToFalse();

            //Act
            p.ShowFirstTimeUserSetup();

            //Assert
            Assert.False(budgetView.calledShowMessageWithConfirmation);
            Assert.False(budgetView.calledOpenNewFile);

            // Cleanup (delete bad key)
            key.DeleteValue(KEYNAME);
        }

        // ========================================================================

        [Fact]
        public void PresenterMethods_IsFileSelected_FalseState()
        {
            //Arrange
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(budgetView);
            p.expenseView = ev;
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
        public void PresenterMethods_IsFileSelected_TrueState()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(budgetView);
            p.expenseView = ev;
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
        public void PresenterMethods_GetCategories_Success()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(budgetView);
            p.expenseView = ev;
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
        public void PresenterMethods_GetExpenses_Success()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(budgetView);
            p.expenseView = ev;
            p.ConnectToDatabase(messyDB, false);
            int expenseCount;

            //Act
            expenseCount = p.GetExpenses().Count;

            //Assert
            Assert.Equal(numberOfExpensesInFile, expenseCount);
        }

        // ========================================================================

        [Fact]
        public void PresenterMethods_UpdateExpense_Success()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(budgetView);
            p.expenseView = ev;
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
        public void PresenterMethods_UpdateExpense_InvalidAmount()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(budgetView);
            p.expenseView = ev;
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
        public void PresenterMethods_UpdateExpense_InvalidCategory()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(budgetView);
            p.expenseView = ev;
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
        public void PresenterMethods_UpdateExpense_InvalidDescription()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(budgetView);
            p.expenseView = ev;
            p.ConnectToDatabase(messyDB, false);
            ev.SetToFalse();
            Expense firstExpenseInFile = p.GetExpenses().First();

            const string BAD_DESCRIPTION = "";

            //Act
            p.UpdateExpense(firstExpenseInFile.Id, firstExpenseInFile.Date, firstExpenseInFile.Category, firstExpenseInFile.Amount.ToString(), BAD_DESCRIPTION);

            //Assert
            Assert.True(ev.calledShowError);
            Assert.False(ev.calledSetLastAction);
        }

        // ========================================================================

        [Fact]
        public void PresenterMethods_UpdateExpense_NoConnection()
        {
            //Arrange
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(budgetView);
            p.expenseView = ev;
            ev.SetToFalse();
            budgetView.SetToFalse();

            //Act
            p.UpdateExpense(1, new DateTime(), 1, "15", "Lunch");

            //Assert
            Assert.True(ev.calledShowError);
            Assert.False(ev.calledSetLastAction);
            Assert.False(budgetView.calledRefresh);
        }

        // ========================================================================

        [Fact]
        public void PresenterMethods_DeleteExpense_Success()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(budgetView);
            p.expenseView = ev;
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

        // ========================================================================

        [Fact]
        public void PresenterMethods_DeleteExpense_InexistentExpenseItem()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(budgetView);
            p.expenseView = ev;
            p.ConnectToDatabase(messyDB, false);
            ev.SetToFalse();
            budgetView.SetToFalse();
            int count = p.GetExpenses().Count();

            //Act
            p.DeleteExpense(100, "Does not exist");

            //Assert
            Assert.True(budgetView.calledRefresh);
            Assert.True(ev.calledSetLastAction);
            Assert.True(ev.calledClearInputs);
            Assert.Equal(count, p.GetExpenses().Count());
        }

        // ========================================================================

        [Fact]
        public void PresenterMethods_FiltersChange_UpdateView()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(budgetView);
            p.expenseView = ev;
            p.ConnectToDatabase(messyDB, false);
            ev.SetToFalse();
            budgetView.SetToFalse();

            //Act
            p.FiltersChange(null, null, 1, false, false, false);

            //Assert
            Assert.True(budgetView.calledUpdateView);
        }

        // ========================================================================

        [Fact]
        public void PresenterMethods_FiltersChange_NullModel()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(budgetView);
            p.expenseView = ev;
            ev.SetToFalse();
            budgetView.SetToFalse();

            //Act
            p.FiltersChange(null, null, 1, false, false, false);

            //Assert
            Assert.False(budgetView.calledUpdateView);
        }

        // ========================================================================

        [Fact]
        public void PresenterMethods_FiltersChange_GroupByMonthAndCategory()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(budgetView);
            p.ConnectToDatabase(messyDB, false);
            p.expenseView = ev;
            ev.SetToFalse();
            budgetView.SetToFalse();
            DateTime? start = null;
            DateTime? end = null;
            int catId = 1;
            bool filterCat = false;
            bool grpByMonth = true;
            bool grpByCat = true;

            HomeBudget budget = new HomeBudget(messyDB);
            List<Dictionary<string, object>> expected = budget.GetBudgetDictionaryByCategoryAndMonth(start, end, filterCat, catId);

            //Act
            p.FiltersChange(start, end, catId, filterCat, grpByMonth, grpByCat);
            List<Dictionary<string, object>> actual = (List<Dictionary<string, object>>) budgetView.listItems;

            //Assert
            Assert.True(budgetView.calledUpdateView);
            Assert.Equal(expected.Count, actual.Count);
            Assert.Equal(expected.First()["Month"], actual.First()["Month"]);
            Assert.Equal(expected.First()["Total"], actual.First()["Total"]);
        }

        // ========================================================================

        [Fact]
        public void PresenterMethods_FiltersChange_GroupByMonth()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(budgetView);
            p.ConnectToDatabase(messyDB, false);
            p.expenseView = ev;
            ev.SetToFalse();
            budgetView.SetToFalse();
            DateTime? start = null;
            DateTime? end = null;
            int catId = 1;
            bool filterCat = false;
            bool grpByMonth = true;
            bool grpByCat = false;

            HomeBudget budget = new HomeBudget(messyDB);
            List<BudgetItemsByMonth> expected = budget.GetBudgetItemsByMonth(start, end, filterCat, catId);

            //Act
            p.FiltersChange(start, end, catId, filterCat, grpByMonth, grpByCat);
            List<BudgetItemsByMonth> actual = (List<BudgetItemsByMonth>) budgetView.listItems;

            //Assert
            Assert.True(budgetView.calledUpdateView);
            Assert.Equal(expected.Count, actual.Count);
            Assert.Equal(expected.First().Month, actual.First().Month);
            Assert.Equal(expected.First().Total, actual.First().Total);
        }

        // ========================================================================

        [Fact]
        public void PresenterMethods_FiltersChange_GroupByCategory()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(budgetView);
            p.ConnectToDatabase(messyDB, false);
            p.expenseView = ev;
            ev.SetToFalse();
            budgetView.SetToFalse();
            DateTime? start = null;
            DateTime? end = null;
            int catId = 1;
            bool filterCat = false;
            bool grpByMonth = false;
            bool grpByCat = true;

            HomeBudget budget = new HomeBudget(messyDB);
            List<BudgetItemsByCategory> expected = budget.GetBudgetItemsByCategory(start, end, filterCat, catId);

            //Act
            p.FiltersChange(start, end, catId, filterCat, grpByMonth, grpByCat);
            List<BudgetItemsByCategory> actual = (List<BudgetItemsByCategory>)budgetView.listItems;

            //Assert
            Assert.True(budgetView.calledUpdateView);
            Assert.Equal(expected.Count, actual.Count);
            Assert.Equal(expected.First().Category, actual.First().Category);
            Assert.Equal(expected.First().Total, actual.First().Total);
        }

        // ========================================================================

        [Fact]
        public void PresenterMethods_FiltersChange_AllItems()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(budgetView);
            p.ConnectToDatabase(messyDB, false);
            p.expenseView = ev;
            ev.SetToFalse();
            budgetView.SetToFalse();
            DateTime? start = null;
            DateTime? end = null;
            int catId = 1;
            bool filterCat = false;
            bool grpByMonth = false;
            bool grpByCat = false;

            HomeBudget budget = new HomeBudget(messyDB);
            List<BudgetItem> expected = budget.GetBudgetItems(start, end, filterCat, catId);

            //Act
            p.FiltersChange(start, end, catId, filterCat, grpByMonth, grpByCat);
            List<BudgetItem> actual = (List<BudgetItem>)budgetView.listItems;

            //Assert
            Assert.True(budgetView.calledUpdateView);
            Assert.Equal(expected.Count, actual.Count);
            Assert.Equal(expected.First().Category, actual.First().Category);
            Assert.Equal(expected.First().CategoryID, actual.First().CategoryID);
            Assert.Equal(expected.First().ExpenseID, actual.First().ExpenseID);
            Assert.Equal(expected.First().Date, actual.First().Date);
            Assert.Equal(expected.First().ShortDescription, actual.First().ShortDescription);
            Assert.Equal(expected.First().Amount, actual.First().Amount);
            Assert.Equal(expected.First().Balance, actual.First().Balance);

        }
    }
}
