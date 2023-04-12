using Budget;
using Budget_WPF;
using NuGet.Frameworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetCodeTests
{
    public class TestView : ViewInterface
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
            calledAddExpense |= true;
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
            Presenter p = new Presenter(v);

            //Assert
            Assert.IsType<Presenter>(p);
        }

        // ========================================================================

        [Fact]
        public void PresenterMethods_ConnectToDatabse_Success()
        {
            //Assert
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestView v = new TestView();
            Presenter p = new Presenter(v);
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
            //Assert
            TestView v = new TestView();
            Presenter p = new Presenter(v);
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
            //Assert
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestView v = new TestView();
            Presenter p = new Presenter(v);
            p.ConnectToDatabase(messyDB, false);
            int count = p.GetCategories().Count;
            string desc = "Game";
            Category.CategoryType type = Category.CategoryType.Expense;
            v.calledSetLastAction = false;

            //Act
            p.AddCategory(desc, type);
            Category cat = p.GetCategories()[maxIDInCategoryInFile + 1];

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
            //Assert
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestView v = new TestView();
            Presenter p = new Presenter(v);
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
            //Assert
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestView v = new TestView();
            Presenter p = new Presenter(v);
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
            //Assert
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestView v = new TestView();
            Presenter p = new Presenter(v);
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
        public void PresenterMethods_AddExpense_InexistantCategory()
        {
            //Assert
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestView v = new TestView();
            Presenter p = new Presenter(v);
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
            //Assert
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestView v = new TestView();
            Presenter p = new Presenter(v);
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
            //Assert
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestView v = new TestView();
            Presenter p = new Presenter(v);
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
        public void PresenterMethods_UnsavedChangesCheck_ShowConfirmPopUp()
        {
            //Assert
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestView v = new TestView();
            Presenter p = new Presenter(v);
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
            //Assert
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestView v = new TestView();
            Presenter p = new Presenter(v);
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
            //Assert
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            TestView v = new TestView();
            Presenter p = new Presenter(v);
            p.ConnectToDatabase(messyDB, false);

            //Act
            p.SetRecentFile(messyDB);

            //Assert
            Assert.Equal(messyDB, p.GetRecentFile());
        }
    }
}
