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
    public class TestView : IExpenseView
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

        public void AddExpense()
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
    }


    [Collection("Sequential")]
    public class TestPresenter
    {
        public int maxIDInCategoryInFile = TestConstants.maxIDInCategoryInFile;

        // ========================================================================

        [Fact]
        public void PresenterConstructor()
        {
            //Arrange
            TestView v = new TestView();

            //Act
            ExpensePresenter p = new ExpensePresenter(v);

            //Assert
            Assert.IsType<ExpensePresenter>(p);
        }

        // ========================================================================

        [Fact]
        public void PresenterMethods_ConnectToDatabse_Success()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestView v = new TestView();
            ExpensePresenter p = new ExpensePresenter(v);
            v.calledShowCurrentFile = false;
            v.calledSetLastAction = false;
            v.calledRefresh = false;

            //Act
            p.ConnectToDatabase(messyDB, false);

            //Assert
            Assert.True(v.calledShowCurrentFile);
            Assert.True(v.calledSetLastAction);
            Assert.True(v.calledRefresh);
            Assert.True(p.IsFileSelected());
        }

        // ========================================================================

        [Fact]
        public void PresenterMethods_ConnectToDatabse_EmptyFilename()
        {
            //Arrange
            TestView v = new TestView();
            ExpensePresenter p = new ExpensePresenter(v);
            v.calledShowCurrentFile = false;
            v.calledSetLastAction = false;
            v.calledRefresh = false;
            v.calledShowError = false;

            //Act
            p.ConnectToDatabase("", false);

            //Assert
            Assert.True(v.calledShowError);
            Assert.False(v.calledShowCurrentFile);
            Assert.False(v.calledSetLastAction);
            Assert.False(v.calledRefresh);
            Assert.False(p.IsFileSelected());
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
            TestView v = new TestView();
            ExpensePresenter p = new ExpensePresenter(v);
            p.ConnectToDatabase(messyDB, false);
            int count = p.GetCategories().Count;
            string desc = "Game";
            Category.CategoryType type = Category.CategoryType.Expense;
            v.calledSetLastAction = false;

            //Act
            p.AddCategory(desc, type);
            Category cat = p.GetCategories()[maxIDInCategoryInFile];

            //Assert
            Assert.True(v.calledSetLastAction);
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
            TestView v = new TestView();
            ExpensePresenter p = new ExpensePresenter(v);
            p.ConnectToDatabase(messyDB, false);
            v.calledSetLastAction = false;
            v.calledShowError = false;

            //Act
            p.AddCategory("", Budget.Category.CategoryType.Expense);

            //Assert
            Assert.True(v.calledShowError);
            Assert.False(v.calledSetLastAction);
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
            TestView v = new TestView();
            ExpensePresenter p = new ExpensePresenter(v);
            p.ConnectToDatabase(messyDB, false);
            v.calledSetLastAction = false;
            v.calledShowError = false;

            //Act
            p.AddCategory("Game", null);

            //Assert
            Assert.True(v.calledShowError);
            Assert.False(v.calledSetLastAction);
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
            TestView v = new TestView();
            ExpensePresenter p = new ExpensePresenter(v);
            p.ConnectToDatabase(messyDB, false);
            v.calledSetLastAction = false;
            v.calledClearInputs = false;

            //Act
            p.AddExpense(new DateTime(), 1, "10", "Lunch", false);

            //Assert
            Assert.True(v.calledSetLastAction);
            Assert.True(v.calledClearInputs);
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
            TestView v = new TestView();
            ExpensePresenter p = new ExpensePresenter(v);
            p.ConnectToDatabase(messyDB, false);
            v.calledSetLastAction = false;
            v.calledClearInputs = false;

            //Act
            p.AddExpense(new DateTime(), 1, "10", "Lunch", true);

            //Assert
            Assert.True(v.calledSetLastAction);
            Assert.True(v.calledClearInputs);

            // TODO: Currently there is no way of checking that the credit expense was added.
            // It will be possible once we have the code to display expenses to the user.
        }

        // ========================================================================

        [Fact]
        public void PresenterMethods_AddExpense_InexistantCategory()
        {
            //Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestView v = new TestView();
            ExpensePresenter p = new ExpensePresenter(v);
            p.ConnectToDatabase(messyDB, false);
            int count = p.GetCategories().Count;
            v.calledSetLastAction = false;
            v.calledShowError = false;

            //Act
            p.AddExpense(new DateTime(), count + 1, "10", "Lunch", false);

            //Assert
            Assert.True(v.calledShowError);
            Assert.False(v.calledSetLastAction);
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
            TestView v = new TestView();
            ExpensePresenter p = new ExpensePresenter(v);
            p.ConnectToDatabase(messyDB, false);
            v.calledSetLastAction = false;
            v.calledShowError = false;

            //Act
            p.AddExpense(new DateTime(), 1, "10", "", false);

            //Assert
            Assert.True(v.calledShowError);
            Assert.False(v.calledSetLastAction);
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
            TestView v = new TestView();
            ExpensePresenter p = new ExpensePresenter(v);
            p.ConnectToDatabase(messyDB, false);
            v.calledSetLastAction = false;
            v.calledShowError = false;

            //Act
            p.AddExpense(new DateTime(), 1, "abc", "Lunch", false);

            //Assert
            Assert.True(v.calledShowError);
            Assert.False(v.calledSetLastAction);
        }

        // ========================================================================

        [Fact]
        public void PresenterMethods_AddExpense_NoConnection()
        {
            //Arrange
            TestView v = new TestView();
            ExpensePresenter p = new ExpensePresenter(v);
            v.calledSetLastAction = false;
            v.calledShowError = false;

            //Act
            p.AddExpense(new DateTime(), 1, "abc", "Lunch", false);

            //Assert
            Assert.True(v.calledShowError);
            Assert.False(v.calledSetLastAction);
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
            TestView v = new TestView();
            ExpensePresenter p = new ExpensePresenter(v);
            p.ConnectToDatabase(messyDB, false);
            v.calledShowMessageWithConfirmation = false;

            //Act
            p.UnsavedChangesCheck("Lunch", "");

            //Assert
            Assert.True(v.calledShowMessageWithConfirmation);
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
            TestView v = new TestView();
            ExpensePresenter p = new ExpensePresenter(v);
            p.ConnectToDatabase(messyDB, false);
            v.calledShowMessageWithConfirmation = false;

            //Act
            p.UnsavedChangesCheck("", "");
            
            //Assert
            Assert.False(v.calledShowMessageWithConfirmation);

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
            TestView v = new TestView();
            ExpensePresenter p = new ExpensePresenter(v);
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
            TestView v = new TestView();
            ExpensePresenter p = new ExpensePresenter(v);

            // Delete the existing key if it exists
            try
            {
                RegistryKey? key = Registry.CurrentUser.OpenSubKey(FULL_REGISTER_PATH, true);
                key.DeleteValue(KEYNAME);
            } catch { }

            v.calledShowMessageWithConfirmation = false;
            v.calledOpenNewFile = false;

            //Act
            p.ShowFirstTimeUserSetup();

            //Assert
            Assert.True(v.calledShowMessageWithConfirmation);
            Assert.True(v.calledOpenNewFile);
        }

        // ========================================================================

        [Fact]
        public void PresenterMethods_ShowFirstTimeUserSetup_ReturningUser()
        {
            const string FULL_REGISTER_PATH = "SOFTWARE\\AppDevBudget\\";
            const string KEYNAME = "recentFile";

            //Arrange
            TestView v = new TestView();
            ExpensePresenter p = new ExpensePresenter(v);
            // Wipe recent file register
            RegistryKey? key = Registry.CurrentUser.OpenSubKey(FULL_REGISTER_PATH, true);
            key.SetValue(KEYNAME, "C:\\");
            v.calledShowMessageWithConfirmation = false;
            v.calledOpenNewFile = false;

            //Act
            p.ShowFirstTimeUserSetup();

            //Assert
            Assert.False(v.calledShowMessageWithConfirmation);
            Assert.False(v.calledOpenNewFile);

            // Cleanup (delete bad key)
            key.DeleteValue(KEYNAME);
        }

        // ========================================================================

        [Fact]
        public void PresenterMethods_IsFileSelected_FalseState()
        {
            //Arrange
            TestView v = new TestView();
            ExpensePresenter p = new ExpensePresenter(v);
            v.calledShowError = false;
            bool isSelected;

            //Act
            isSelected = p.IsFileSelected();

            //Assert
            Assert.True(v.calledShowError);
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
            TestView v = new TestView();
            ExpensePresenter p = new ExpensePresenter(v);
            p.ConnectToDatabase(messyDB, false);
            v.calledShowError = false;
            bool isSelected;

            //Act
            isSelected = p.IsFileSelected();

            //Assert
            Assert.False(v.calledShowError);
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
            TestView v = new TestView();
            ExpensePresenter p = new ExpensePresenter(v);
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
