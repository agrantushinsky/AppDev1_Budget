using System;
using Xunit;
using System.IO;
using System.Collections.Generic;
using Budget;
using System.Data.SQLite;

namespace BudgetCodeTests
{
    [Collection("Sequential")]
    public class TestCategories
    {
        public int numberOfCategoriesInFile = TestConstants.numberOfCategoriesInFile;
        public String testInputFile = TestConstants.testDBInputFile;
        public int maxIDInCategoryInFile = TestConstants.maxIDInCategoryInFile;
        Category firstCategoryInFile = TestConstants.firstCategoryInFile;
        int IDWithSaveType = TestConstants.CategoryIDWithSaveType;

        // ========================================================================

        [Fact]
        public void CategoriesObject_New()
        {
            // Arrange
            String folder = TestConstants.GetSolutionDir();
            String newDB = $"{folder}\\newDB.db";
            Database.newDatabase(newDB);
            SQLiteConnection conn = Database.dbConnection;

            // Act
            Categories categories = new Categories(conn, true);

            // Assert 
            Assert.IsType<Categories>(categories);
        }

        // ========================================================================

        [Fact]
        public void CategoriesObject_New_CreatesDefaultCategories()
        {
            // Arrange
            String folder = TestConstants.GetSolutionDir();
            String newDB = $"{folder}\\newDB.db";
            Database.newDatabase(newDB);
            SQLiteConnection conn = Database.dbConnection;

            // Act
            Categories categories = new Categories(conn, true);

            // Assert 
            Assert.False(categories.List().Count == 0, "Non zero categories");

        }

        // ========================================================================

        [Fact]
        public void CategoriesMethod_ReadFromDatabase_ValidateCorrectDataWasRead()
        {
            // Arrange
            String folder = TestConstants.GetSolutionDir();
            String existingDB = $"{folder}\\{TestConstants.testDBInputFile}";
            Database.existingDatabase(existingDB);
            SQLiteConnection conn = Database.dbConnection;

            // Act
            Categories categories = new Categories(conn, false);
            List<Category> list = categories.List();
            Category firstCategory = list[0];

            // Assert
            Assert.Equal(numberOfCategoriesInFile, list.Count);
            Assert.Equal(firstCategoryInFile.Id, firstCategory.Id);
            Assert.Equal(firstCategoryInFile.Description, firstCategory.Description);

        }

        // ========================================================================

        [Fact]
        public void CategoriesMethod_List_ReturnsListOfCategories()
        {
            // Arrange
            String folder = TestConstants.GetSolutionDir();
            String newDB = $"{folder}\\{TestConstants.testDBInputFile}";
            Database.existingDatabase(newDB);
            SQLiteConnection conn = Database.dbConnection;
            Categories categories = new Categories(conn, false);

            // Act
            List<Category> list = categories.List();

            // Assert
            Assert.Equal(numberOfCategoriesInFile, list.Count);

        }


        // ========================================================================

        [Fact]
        public void CategoriesMethod_Add()
        {
            // Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            Database.existingDatabase(messyDB);
            SQLiteConnection conn = Database.dbConnection;
            Categories categories = new Categories(conn, false);
            string descr = "New Category";
            Category.CategoryType type = Category.CategoryType.Income;

            // Act
            categories.Add(descr, type);
            List<Category> categoriesList = categories.List();
            int sizeOfList = categories.List().Count;

            // Assert
            Assert.Equal(numberOfCategoriesInFile + 1, sizeOfList);
            Assert.Equal(descr, categoriesList[sizeOfList - 1].Description);

        }

        // ========================================================================

        [Fact]
        public void CategoriesMethod_Delete()
        {
            // Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            Database.existingDatabase(messyDB);
            SQLiteConnection conn = Database.dbConnection;
            Categories categories = new Categories(conn, false);
            int IdToDelete = 3;

            // Act
            categories.Delete(IdToDelete);
            List<Category> categoriesList = categories.List();
            int sizeOfList = categoriesList.Count;

            // Assert
            Assert.Equal(numberOfCategoriesInFile - 1, sizeOfList);
            Assert.False(categoriesList.Exists(e => e.Id == IdToDelete), "correct Category item deleted");

        }

        // ========================================================================

        [Fact]
        public void CategoriesMethod_Delete_InvalidIDDoesntCrash()
        {
            // Arrange
            // Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messyDB";
            System.IO.File.Copy(goodDB, messyDB, true);
            Database.existingDatabase(messyDB);
            SQLiteConnection conn = Database.dbConnection;
            Categories categories = new Categories(conn, false);
            int IdToDelete = 9999;
            int sizeOfList = categories.List().Count;

            // Act
            try
            {
                categories.Delete(IdToDelete);
                Assert.Equal(sizeOfList, categories.List().Count);
            }

            // Assert
            catch
            {
                Assert.True(false, "Invalid ID causes Delete to break");
            }
        }

        // ========================================================================

        [Fact]
        public void CategoriesMethod_WriteToFile()
        {

            // NOTE: Currently failing.  Added new test to try to track down source of 
            //       problem
            // CategoryTypeSavingsReadCorrectlyFromFile()
            //  ... which also fails, so that is why the WriteToFile is not accurate...
            //  ... fix above test, and then this one should pass as well.

            // Arrange
            String dir = GetSolutionDir();
            Categories categories = new Categories();
            categories.ReadFromFile(dir + "\\" + testInputFile);
            string fileName = TestConstants.CategoriesOutputTestFile;
            String outputFile = dir + "\\" + fileName;
            File.Delete(outputFile);

            // Act
            categories.SaveToFile(outputFile);

            // Assert
            Assert.True(File.Exists(outputFile), "output file created");
            Assert.True(FileEquals(dir + "\\" + testInputFile, outputFile), "Input /output files are the same");
            String fileDir = Path.GetFullPath(Path.Combine(categories.DirName, ".\\"));
            Assert.Equal(dir, fileDir);
            Assert.Equal(fileName, categories.FileName);

            // Cleanup
            if (FileEquals(dir + "\\" + testInputFile, outputFile)) {
                File.Delete(outputFile);
            }

        }

        // ========================================================================

        [Fact]
        public void CategoriesMethod_WriteToFile_WriteToLastFileWrittenToByDefault()
        {
            // Arrange
            String dir = GetSolutionDir();
            Categories categories = new Categories();

            //saving the categories data from file to property
            categories.ReadFromFile(dir + "\\" + testInputFile);
            string fileName = TestConstants.CategoriesOutputTestFile;
            String outputFile = dir + "\\" + fileName;
            File.Delete(outputFile);
            categories.SaveToFile(outputFile); // output file is now last file that was written to.
            File.Delete(outputFile);  // Delete the file

            // Act
            categories.SaveToFile(); // should write to same file as before

            // Assert
            Assert.True(File.Exists(outputFile), "output file created");
            String fileDir = Path.GetFullPath(Path.Combine(categories.DirName, ".\\"));
            Assert.Equal(dir, fileDir);
            Assert.Equal(fileName, categories.FileName);

            // Cleanup
            if (FileEquals(dir + "\\" + testInputFile, outputFile))
            {
                File.Delete(outputFile);
            }

        }


        // ========================================================================

        [Fact]
        public void CategoriesMethod_GetCategoryFromId()
        {
            // Arrange
            String folder = TestConstants.GetSolutionDir();
            String newDB = $"{folder}\\{TestConstants.testDBInputFile}";
            Database.existingDatabase(newDB);
            SQLiteConnection conn = Database.dbConnection;
            Categories categories = new Categories(conn, false);
            int catID = 15;

            // Act
            Category category = categories.GetCategoryFromId(catID);

            // Assert
            Assert.Equal(catID, category.Id);

        }

        // ========================================================================

        [Fact]
        public void CategoriesMethod_SetCategoriesToDefaults()
        {

            // Arrange
            String folder = TestConstants.GetSolutionDir();
            String newDB = $"{folder}\\newDB.db";
            Database.newDatabase(newDB);
            SQLiteConnection conn = Database.dbConnection;

            // Act
            Categories categories = new Categories(conn, true);
            List<Category> originalList = categories.List();

            // modify list of categories
            categories.Delete(1);
            categories.Delete(2);
            categories.Delete(3);
            categories.Add("Another one ", Category.CategoryType.Credit);

            //"just double check that initial conditions are correct");
            Assert.NotEqual(originalList.Count, categories.List().Count);

            // Act
            categories.SetCategoriesToDefaults();

            // Assert
            Assert.Equal(originalList.Count, categories.List().Count);
            foreach (Category defaultCat in originalList)
            {
                Assert.True(categories.List().Exists(c => c.Description == defaultCat.Description && c.Type == defaultCat.Type));
            }

        }

        // ========================================================================

        [Fact]
        public void CategoriesMethod_UpdateCategory()
        {
            // Arrange
            String folder = TestConstants.GetSolutionDir();
            String newDB = $"{folder}\\newDB.db";
            Database.newDatabase(newDB);
            SQLiteConnection conn = Database.dbConnection;
            Categories categories = new Categories(conn, true);
            String newDescr = "Presents";
            int id = 11;

            // Act
            categories.UpdateProperties(id, newDescr, Category.CategoryType.Income);
            Category category = categories.GetCategoryFromId(id);

            // Assert 
            Assert.Equal(newDescr, category.Description);
            Assert.Equal(Category.CategoryType.Income, category.Type);

        }
    }
}

