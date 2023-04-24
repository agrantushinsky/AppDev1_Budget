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
    }
    public class TestExpenseView : IExpenseView
    {
        public bool calledAddCategory = false;
        public bool calledAddExpense = false;
        public bool calledClearInputs = false;
        public bool calledOpenExistingFile = false;
        public bool calledCloseExistingFile = false;
        public bool calledOpenNewFile = false;
        public bool calledRefresh = false;
        public bool calledShowCurrentFile = false;
        public bool calledShowError = false;
        public bool calledShowMessageWithConfirmation = false;
        public bool calledSetLastAction = false;


        public void AddCategory()
        {
            calledAddCategory = true;
        }

        public void SaveExpense()
        {
            calledAddExpense = true;
        }

        public void ClearInputs()
        {
            calledClearInputs = true;
        }

        public void OpenExistingFile()
        {
            calledOpenExistingFile = true;
        }

        public void OpenNewFile()
        {
            calledOpenNewFile = true;
        }

        public void Refresh()
        {
            calledRefresh = true;
        }

        public void SetLastAction(string message)
        {
            calledSetLastAction = true;
        }

        public void ShowCurrentFile(string filename)
        {
            calledShowCurrentFile = true;
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
            throw new NotImplementedException();
        }
    }


    [Collection("Sequential")]
    public class TestExpensePresenter
    {
        public int maxIDInCategoryInFile = TestConstants.maxIDInCategoryInFile;
        public TestBudgetView bv = new TestBudgetView();

        // ========================================================================

        [Fact]
        public void PresenterConstructor()
        {
            //Arrange
            TestExpenseView ev = new TestExpenseView();

            //Act
            Presenter p = new Presenter(bv,ev);

            //Assert
            Assert.IsType<Presenter>(p);
        }

        // ========================================================================

        [Fact]
        public void ExpensePresenterMethods_ConnectToDatabse_Success()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(bv,ev);
            ev.calledShowCurrentFile = false;
            ev.calledSetLastAction = false;
            ev.calledRefresh = false;

            //Act
            p.ConnectToDatabase(messyDB, false);

            //Assert
            Assert.True(ev.calledShowCurrentFile);
            Assert.True(ev.calledSetLastAction);
            Assert.True(ev.calledRefresh);
            Assert.True(p.IsFileSelected());
        }

        // ========================================================================

        [Fact]
        public void ExpensePresenterMethods_ConnectToDatabse_EmptyFilename()
        {
            //Arrange
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(bv,ev);
            ev.calledShowCurrentFile = false;
            ev.calledSetLastAction = false;
            ev.calledRefresh = false;
            ev.calledShowError = false;

            //Act
            p.ConnectToDatabase("", false);

            //Assert
            Assert.True(ev.calledShowError);
            Assert.False(ev.calledShowCurrentFile);
            Assert.False(ev.calledSetLastAction);
            Assert.False(ev.calledRefresh);
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
            Presenter p = new Presenter(bv,ev);
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
            Presenter p = new Presenter(bv,ev);
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
            Presenter p = new Presenter(bv,ev);
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
            Presenter p = new Presenter(bv,ev);
            p.ConnectToDatabase(messyDB, false);
            ev.calledSetLastAction = false;
            ev.calledClearInputs = false;

            //Act
            p.AddExpense(new DateTime(), 1, "10", "Lunch", false);

            //Assert
            Assert.True(ev.calledSetLastAction);
            Assert.True(ev.calledClearInputs);
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
            Presenter p = new Presenter(bv,ev);
            p.ConnectToDatabase(messyDB, false);
            ev.calledSetLastAction = false;
            ev.calledClearInputs = false;

            //Act
            p.AddExpense(new DateTime(), 1, "10", "Lunch", true);

            //Assert
            Assert.True(ev.calledSetLastAction);
            Assert.True(ev.calledClearInputs);

            // TODO: Currently there is no way of checking that the credit expense was added.
            // It will be possible once we have the code to display expenses to the user.
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
            Presenter p = new Presenter(bv,ev);
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
            Presenter p = new Presenter(bv,ev);
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
            Presenter p = new Presenter(bv,ev);
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
            Presenter p = new Presenter(bv,ev);
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
            Presenter p = new Presenter(bv,ev);
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
            Presenter p = new Presenter(bv,ev);
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
            Presenter p = new Presenter(bv,ev);
            p.ConnectToDatabase(messyDB, false);

            //Act
            p.SetRecentFile(messyDB);

            //Assert
            Assert.Equal(messyDB, p.GetRecentFile());
        }

        // ========================================================================

        [Fact]
        public void ExpensePresenterMethods_ShowFirstTimeUserSetup_FirstTimeUser()
        {
            const string FULL_REGISTER_PATH = "SOFTWARE\\AppDevBudget\\";
            const string KEYNAME = "recentFile";

            //Arrange
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(bv,ev);

            // Delete the existing key if it exists
            try
            {
                RegistryKey? key = Registry.CurrentUser.OpenSubKey(FULL_REGISTER_PATH, true);
                key.DeleteValue(KEYNAME);
            } catch { }

            ev.calledShowMessageWithConfirmation = false;
            ev.calledOpenNewFile = false;

            //Act
            p.ShowFirstTimeUserSetup();

            //Assert
            Assert.True(ev.calledShowMessageWithConfirmation);
            Assert.True(ev.calledOpenNewFile);
        }

        // ========================================================================

        [Fact]
        public void ExpensePresenterMethods_ShowFirstTimeUserSetup_ReturningUser()
        {
            const string FULL_REGISTER_PATH = "SOFTWARE\\AppDevBudget\\";
            const string KEYNAME = "recentFile";

            //Arrange
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(bv,ev);
            // Wipe recent file register
            RegistryKey? key = Registry.CurrentUser.OpenSubKey(FULL_REGISTER_PATH, true);
            key.SetValue(KEYNAME, "C:\\");
            ev.calledShowMessageWithConfirmation = false;
            ev.calledOpenNewFile = false;

            //Act
            p.ShowFirstTimeUserSetup();

            //Assert
            Assert.False(ev.calledShowMessageWithConfirmation);
            Assert.False(ev.calledOpenNewFile);

            // Cleanup (delete bad key)
            key.DeleteValue(KEYNAME);
        }

        // ========================================================================

        [Fact]
        public void ExpensePresenterMethods_IsFileSelected_FalseState()
        {
            //Arrange
            TestExpenseView ev = new TestExpenseView();
            Presenter p = new Presenter(bv,ev);
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
            Presenter p = new Presenter(bv,ev);
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
            Presenter p = new Presenter(bv,ev);
            p.ConnectToDatabase(messyDB, false);
            int categoryCount;
            const int EXPECTED_COUNT = 17;

            //Act
            categoryCount = p.GetCategories().Count;

            //Assert
            Assert.Equal(EXPECTED_COUNT, categoryCount);
        }
    }
}
