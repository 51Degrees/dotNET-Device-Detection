/**
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited.
 * Copyright (c) 2017 51Degrees Mobile Experts Limited, 5 Charlotte Close,
 * Caversham, Reading, Berkshire, United Kingdom RG4 7BY
 * 
 * This Source Code Form is the subject of the following patents and patent 
 * applications, owned by 51Degrees Mobile Experts Limited of 5 Charlotte
 * Close, Caversham, Reading, Berkshire, United Kingdom RG4 7BY:
 * European Patent No. 2871816;
 * European Patent Application No. 17184134.9;
 * United States Patent Nos. 9,332,086 and 9,350,823; and
 * United States Patent Application No. 15/686,066.
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0.
 * 
 * If a copy of the MPL was not distributed with this file, You can obtain
 * one at http://mozilla.org/MPL/2.0/.
 * 
 * This Source Code Form is "Incompatible With Secondary Licenses", as
 * defined by the Mozilla Public License, v. 2.0.
 */
 using FiftyOne.Foundation.Mobile.Detection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using static FiftyOne.Foundation.Mobile.Detection.DataSetBuilder;
using File = System.IO.File;

namespace FiftyOne.Tests.Integration.DataSetBuilderTests
{
    [TestClass]
    public abstract class FileBase : Base<BuildFromFile>
    {
        protected override BuildFromFile InitBuilder()
        {
            return Foundation.Mobile.Detection.DataSetBuilder.File();
        }

        protected override IndirectDataSet BuildDataset(DataSetBuilder builder)
        {
            return (builder as BuildFromFile).Build(DataFile);
        }

        /// <summary>
        /// Check that the builder deletes the data file after disposing when SetTempFile is true 
        /// </summary>
        protected void DataSetBuilder_TempFile()
        {
            // Arange
            string tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".tmp");
            File.Copy(DataFile, tempFile);

            // Act
            using (IndirectDataSet dataset = InitBuilder()
                    .SetTempFile(true)
                    .Build(tempFile))
            {
            }

            // Assert
            Assert.IsFalse(File.Exists(tempFile), "Temp file was not deleted");
        }
    }
}
