/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2014 51Degrees Mobile Experts Limited, 5 Charlotte Close,
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
 * This Source Code Form is “Incompatible With Secondary Licenses”, as
 * defined by the Mozilla Public License, v. 2.0.
 * ********************************************************************* */
using FiftyOne.Foundation.Mobile.Detection.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiftyOne.UnitTests.Performance
{
    internal static class Asserts
    {
        internal static void AssertCacheMissesGoodAll(DataSet dataSet)
        {
            Assert.IsTrue(dataSet.PercentageSignatureCacheMisses < 0.4, "Signature Cache Misses");
            Assert.IsTrue(dataSet.PercentageStringsCacheMisses < 0.6, "Strings Cache Misses");
            Assert.IsTrue(dataSet.PercentageRankedSignatureCacheMisses < 0.5, "Ranked Signatures Cache Misses");
            Assert.IsTrue(dataSet.PercentageNodeCacheMisses < 0.3, "Node Cache Misses");
            Assert.IsTrue(dataSet.PercentageValuesCacheMisses < 0.3, "Value Cache Misses");
            Assert.IsTrue(dataSet.PercentageProfilesCacheMisses < 0.3, "Profile Cache Misses");
        }
        
        internal static void AssertCacheMissesGood(DataSet dataSet)
        {
            Assert.IsTrue(dataSet.PercentageSignatureCacheMisses < 0.4, "Signature Cache Misses");
            Assert.IsTrue(dataSet.PercentageStringsCacheMisses < 0.5, "Strings Cache Misses");
            Assert.IsTrue(dataSet.PercentageRankedSignatureCacheMisses < 0.5, "Ranked Signatures Cache Misses");
            Assert.IsTrue(dataSet.PercentageNodeCacheMisses < 0.3, "Node Cache Misses");
        }

        internal static void AssertCacheMissesBadAll(DataSet dataSet)
        {
            Assert.IsTrue(dataSet.PercentageSignatureCacheMisses < 0.4, "Signature Cache Misses");
            Assert.IsTrue(dataSet.PercentageStringsCacheMisses < 0.5, "Strings Cache Misses");
            Assert.IsTrue(dataSet.PercentageRankedSignatureCacheMisses < 0.5, "Ranked Signatures Cache Misses");
            Assert.IsTrue(dataSet.PercentageNodeCacheMisses < 0.5, "Node Cache Misses");
            Assert.IsTrue(dataSet.PercentageValuesCacheMisses < 0.3, "Value Cache Misses");
            Assert.IsTrue(dataSet.PercentageProfilesCacheMisses < 0.3, "Profile Cache Misses");
        }

        internal static void AssertCacheMissesBad(DataSet dataSet)
        {
            Assert.IsTrue(dataSet.PercentageSignatureCacheMisses < 0.4, "Signature Cache Misses");
            Assert.IsTrue(dataSet.PercentageStringsCacheMisses < 0.8, "Strings Cache Misses");
            Assert.IsTrue(dataSet.PercentageRankedSignatureCacheMisses < 0.5, "Ranked Signatures Cache Misses");
            Assert.IsTrue(dataSet.PercentageNodeCacheMisses < 0.5, "Node Cache Misses");
        }
    }
}
