/**
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited.
 * Copyright (c) 2017 51Degrees Mobile Experts Limited, 5 Charlotte Close,
 * Caversham, Reading, Berkshire, United Kingdom RG4 7BY
 * 
 * This Source Code Form is the subject of the following patent
 * applications, owned by 51Degrees Mobile Experts Limited of 5 Charlotte
 * Close, Caversham, Reading, Berkshire, United Kingdom RG4 7BY:
 * European Patent Application No. 13192291.6; and
 * United States Patent Application Nos. 14/085,223 and 14/085,301.
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
 
 using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FiftyOne.Tests.Integration.DataSetBuilderTests.Lite
{
    [TestClass]
    public class V31Buffer : BufferBase
    {
        protected override string DataFile { get { return Utils.GetDataFile(Constants.LITE_PATTERN_V31); } }

        [TestMethod(), TestCategory("Integration"), TestCategory("Lite"), TestCategory("DataSetBuilder")]
        /// <summary>
        /// Check that the un-configured builder returns a dataset that does 
        /// not use caches and does not delete the data file
        /// </summary>
        public void DataSetBuilder_BufferV31_UnconfiguredCaches()
        {
            DataSetBuilder_UnconfiguredCaches();
        }

        [TestMethod(), TestCategory("Integration"), TestCategory("Lite"), TestCategory("DataSetBuilder")]
        /// <summary>
        /// Check that the builder using default caches returns a dataset that 
        /// uses LruCaches
        /// </summary>
        public void DataSetBuilder_BufferV31_DefaultCaches()
        {
            DataSetBuilder_DefaultCaches();
        }

        [TestMethod(), TestCategory("Integration"), TestCategory("Lite"), TestCategory("DataSetBuilder")]
        /// <summary>
        /// Check that the dataset built using the DataSetBuilder with default caches
        /// contains the same data as that built using the MemoryFactory
        /// </summary>
        public void DataSetBuilder_BufferV31_Compare()
        {
            DataSetBuilder_Compare();
        }

        [TestMethod(), TestCategory("Integration"), TestCategory("Lite"), TestCategory("DataSetBuilder")]
        /// <summary>
        /// Check that the dataset built using the DataSetBuilder with custom caches
        /// contains the same data as that built using the MemoryFactory
        /// </summary>
        public void DataSetBuilder_BufferV31_CompareCustomCaches()
        {
            DataSetBuilder_CompareCustomCaches();
        }

    }
}
