/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2015 51Degrees Mobile Experts Limited, 5 Charlotte Close,
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
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FiftyOne.Foundation.Mobile.Detection.Factories;
using System.IO;
using FiftyOne.Foundation.Mobile.Detection;

namespace FiftyOne.Tests.Integration.Performance.Premium
{
    [TestClass]
    public class V31File : FileTest
    {
        protected override string DataFile
        {
            get { return Utils.GetDataFile(Constants.PREMIUM_PATTERN_V31); }
        }

        [TestMethod]
        [TestCategory("Performance"), TestCategory("File"), TestCategory("Premium")]
        public void PremiumV31File_Performance_InitializeTime()
        {
            base.InitializeTime();
        }
        
        [TestMethod]
        [TestCategory("Performance"), TestCategory("File"), TestCategory("Premium")]
        public void PremiumV31File_Performance_BadUserAgentsMulti()
        {
            base.BadUserAgentsMulti(null, Asserts.AssertCacheMissesBad, 5);
        }

        [TestMethod]
        [TestCategory("Performance"), TestCategory("File"), TestCategory("Premium")]
        public void PremiumV31File_Performance_BadUserAgentsSingle()
        {
            base.BadUserAgentsSingle(null, Asserts.AssertCacheMissesBad, 10);
        }

        [TestMethod]
        [TestCategory("Performance"), TestCategory("File"), TestCategory("Premium")]
        public void PremiumV31File_Performance_UniqueUserAgentsMulti()
        {
            base.UniqueUserAgentsMulti(null, Asserts.AssertCacheMissesGood, 1);
        }

        [TestMethod]
        [TestCategory("Performance"), TestCategory("File"), TestCategory("Premium")]
        public void PremiumV31File_Performance_UniqueUserAgentsSingle()
        {
            base.UniqueUserAgentsSingle(null, Asserts.AssertCacheMissesGood, 1);
        }

        [TestMethod]
        [TestCategory("Performance"), TestCategory("File"), TestCategory("Premium")]
        public void PremiumV31File_Performance_RandomUserAgentsMulti()
        {
            base.RandomUserAgentsMulti(null, Asserts.AssertCacheMissesGood, 1);
        }

        [TestMethod]
        [TestCategory("Performance"), TestCategory("File"), TestCategory("Premium")]
        public void PremiumV31File_Performance_RandomUserAgentsSingle()
        {
            base.RandomUserAgentsSingle(null, Asserts.AssertCacheMissesGood, 1);
        }

        [TestMethod]
        [TestCategory("Performance"), TestCategory("File"), TestCategory("Premium")]
        public void PremiumV31File_Performance_BadUserAgentsMultiAll()
        {
            base.BadUserAgentsMulti(_dataSet.Properties, Asserts.AssertCacheMissesBadAll, 6);
        }

        [TestMethod]
        [TestCategory("Performance"), TestCategory("File"), TestCategory("Premium")]
        public void PremiumV31File_Performance_BadUserAgentsSingleAll()
        {
            base.BadUserAgentsSingle(_dataSet.Properties, Asserts.AssertCacheMissesBadAll, 12);
        }

        [TestMethod]
        [TestCategory("Performance"), TestCategory("File"), TestCategory("Premium")]
        public void PremiumV31File_Performance_UniqueUserAgentsMultiAll()
        {
            base.UniqueUserAgentsMulti(_dataSet.Properties, Asserts.AssertCacheMissesGoodAll, 1);
        }

        [TestMethod]
        [TestCategory("Performance"), TestCategory("File"), TestCategory("Premium")]
        public void PremiumV31File_Performance_UniqueUserAgentsSingleAll()
        {
            base.UniqueUserAgentsSingle(_dataSet.Properties, Asserts.AssertCacheMissesGoodAll, 2);
        }

        [TestMethod]
        [TestCategory("Performance"), TestCategory("File"), TestCategory("Premium")]
        public void PremiumV31File_Performance_RandomUserAgentsMultiAll()
        {
            base.RandomUserAgentsMulti(_dataSet.Properties, Asserts.AssertCacheMissesGoodAll, 1);
        }

        [TestMethod]
        [TestCategory("Performance"), TestCategory("File"), TestCategory("Premium")]
        public void PremiumV31File_Performance_RandomUserAgentsSingleAll()
        {
            base.RandomUserAgentsSingle(_dataSet.Properties, Asserts.AssertCacheMissesGoodAll, 1);
        }
    }
}
