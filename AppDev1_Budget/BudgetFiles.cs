using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

// ============================================================================
// (c) Sandy Bultena 2018
// * Released under the GNU General Public License
// ============================================================================

namespace Budget
{

    /// <summary>
    /// BudgetFiles class is used to manage the file for the Budget project
    /// </summary>
    public class BudgetFiles
    {
        /// <summary>
        /// Checks if the provided filepath is not null and exists
        /// </summary>
        /// <param name="FilePath">The filepath to validate</param>
        /// <returns>The path to the file to be read</returns>
        /// <exception cref="FileNotFoundException">If the filepath is null or does not exists</exception>
        public static String VerifyReadFromFileName(String FilePath)
        {
            //Null filepath is not accepted
            if (FilePath == null || FilePath == String.Empty)
            {
                throw new FileNotFoundException("File path must be specified");
            }

            //If the file path doesn't exists, reading is impossible
            if (!File.Exists(FilePath))
            {
                throw new FileNotFoundException("ReadFromFileException: FilePath (" + FilePath + ") does not exist");
            }

            return FilePath;
        }

        /// <summary>
        /// Checks if the provided filepath is not null and is not read only. Creates file if it does not exists.
        /// </summary>
        /// <param name="FilePath">The filepath to validate</param>
        /// <returns>The path of the file where data will be written on</returns>
        /// <exception cref="FileNotFoundException">If the file path is null</exception>
        /// <exception cref="ArgumentException">If the file path to be created is invalid</exception>
        /// <exception cref="Exception">If the file is read only</exception>
        public static String VerifyWriteToFileName(String FilePath)
        {
            //FilePath must always be specficied
            if (FilePath == null || FilePath == String.Empty)
            {
                throw new FileNotFoundException("File path must be specified");
            }

            //If file does not exist, create file
            if (!File.Exists(FilePath))
            {
                try
                {
                    File.Create(FilePath).Close();
                }
                catch(Exception e)
                {
                    throw new ArgumentException("Can't create file: invalid file path");
                }

            }

            //Checks if it's possible to write on the file
            FileAttributes fileAttr = File.GetAttributes(FilePath);
            if ((fileAttr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
            {
                throw new Exception("SaveToFileException:  FilePath(" + FilePath + ") is read only");
            }

            return FilePath;
        }
    }
}
