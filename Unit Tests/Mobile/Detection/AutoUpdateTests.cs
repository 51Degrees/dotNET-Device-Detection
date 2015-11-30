using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FiftyOne.Foundation.Mobile.Detection;
using FiftyOne.Foundation.Mobile.Detection.Factories;
using FiftyOne.Foundation.Mobile.Detection.Entities;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace FiftyOne.Tests.Unit.Mobile.Detection
{
    /// <summary>
    /// Common tests for the automatic update process.
    /// </summary>
    [TestClass]
    public class AutoUpdateTests
    {
        private static TestContext _context;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            _context = context;
        }

        [TestInitialize]
        public void SetUp() 
        {
            CleanFiles();
        }
    
        [TestCleanup]
        public void TearDown() 
        {
            CleanFiles();
        }

        [TestMethod(), TestCategory("API"), TestCategory("Premium"), TestCategory("Update")]
        public void TestUpgradeLite() 
        {
            SetLiteDataFile();
            ValidateDownload(Update());
        }

        [TestMethod(), TestCategory("API"), TestCategory("Premium"), TestCategory("Update")]
        public void TestUpgradeLiteOpen()
        {
            SetLiteDataFile();
            using (var fileHandle = TestDataFile.OpenRead())
            {
                Assert.IsTrue(Update() == 
                    AutoUpdate.AutoUpdateStatus.AUTO_UPDATE_MASTER_FILE_CANT_RENAME);
            }
        }

        [TestMethod(), TestCategory("API"), TestCategory("Premium"), TestCategory("Update")]
        public void TestDownloadNew() 
        {
            if (TestDataFile.Exists)
            {
                TestDataFile.Delete();
            }
            ValidateDownload(Update());
        }

        [TestMethod(), TestCategory("API"), TestCategory("Premium"), TestCategory("Update")]
        [ExpectedException(typeof(ArgumentException))]
        public void BadServerLicenceKey()
        {
            AutoUpdate.Update("BADSERVERKEY", TestDataFile);
        }

        [TestMethod(), TestCategory("API"), TestCategory("Premium"), TestCategory("Update")]
        [ExpectedException(typeof(ArgumentException))]
        public void BadClientLicenceKey()
        {
            AutoUpdate.Update("BAD CLIENT KEY", TestDataFile);
        }

        /// <summary>
        /// Performs an update of the data file with any licence keys 
        /// available from the root solution folder.
        /// </summary>
        /// <returns></returns>
        private AutoUpdate.AutoUpdateStatus Update()
        {
            var licenceKeys = GetLicenceKeys();
            return AutoUpdate.Update(
                    licenceKeys, 
                    TestDataFile);
        }

        /// <summary>
        /// Validates the download for success and checks the data set can
        /// be loaded. Uses the memory factory as this validates more elements
        /// of the data file.
        /// </summary>
        /// <param name="result">Result of the download process.</param>
        private void ValidateDownload(AutoUpdate.AutoUpdateStatus result)
        {
            if (result != AutoUpdate.AutoUpdateStatus.AUTO_UPDATE_SUCCESS)
            {
                Assert.Fail(
                    "Data file update process failed with status '{0}'.",
                    result.ToString());
            }
            using (var dataSet = MemoryFactory.Create(TestDataFile.FullName))
            {
                if (dataSet.Name.Equals("Lite"))
                {
                    Console.WriteLine("Data set name was: " + dataSet.Name);
                    Assert.Fail("Data set name was 'Lite'.");
                }
            }
        }

        /// <summary>
        /// The name of the data file to use during testing.
        /// </summary>
        private const string TEST_DATA_FILE = "51Degrees.dat";    
    
        /// <summary>
        /// Make sure the data file gets replaced with a Lite one to emulate an
        /// existing Lite data file.
        /// </summary>
        protected void SetLiteDataFile() {
            String templateFile = AppDomain.CurrentDomain.BaseDirectory
                    + "\\..\\..\\..\\data\\51Degrees-LiteV3.2.dat";
            // Delete existing file in case it's already of the latest version.
            if (TestDataFile.Exists) 
            {
                TestDataFile.Delete();
            }
            File.Copy(templateFile, TestDataFile.FullName);
            Console.WriteLine("Test Data File: {0}", TestDataFile);
        }
    
        /// <summary>
        /// The test data file location.
        /// </summary>
        protected FileInfo TestDataFile 
        {
            get 
            {
                return new FileInfo(Path.Combine(
                    _context.TestDir,
                    TEST_DATA_FILE));
            }
        }
    
        /// <summary>
        /// Removes all test files from the test directory.
        /// </summary>
        protected void CleanFiles() {
            foreach(var file in TestDataFile.Directory.GetFiles().Where(i =>
                i.Name.StartsWith(TestDataFile.Name)))
            {
                file.Delete();
            }
        }
    
        /// <summary>
        /// Gets an array of all the .lic files in the working directory and
        /// its children.
        /// </summary>
        private FileInfo[] GetLicenceKeyFiles() 
        {
            var rootDirectory = new DirectoryInfo(_context.TestDir).Parent.Parent;
            return rootDirectory.GetFiles(
                "*.lic", SearchOption.AllDirectories).ToArray();
        }

        /// <summary>
        /// All of the licence key files found need to be extracted into an 
        /// array of keys.
        /// </summary>
        /// <returns>
        /// Array of Strings representing licence keys from available 
        /// test files.
        /// </returns>
        protected List<string> GetLicenceKeys() 
        {
            var result = new List<string>();
            foreach (FileInfo licenceKeyFile in GetLicenceKeyFiles()) 
            {
                using (var reader = licenceKeyFile.OpenText())
                {
                    var line = reader.ReadLine();
                    while (line != null)
                    {
                        result.Add(line.Trim());
                        line = reader.ReadLine();
                    }
                }
            }
        
            // Check that there are licence keys. Without these no update test can
            // run.
            if (result.Count == 0) 
            {
                Assert.Inconclusive("No licence keys were available in folder '" + 
                    TestDataFile.Directory.FullName + "'. See " +
                    "https://51degrees.com/compare-data-options to acquire valid " +
                    "licence keys.");
            }
        
            // Return the keys as an array.
            foreach (var licenceKey in result) 
            {
                Console.WriteLine("Licence Key: " + licenceKey);
            }

            return result;
        }    
    }
}
