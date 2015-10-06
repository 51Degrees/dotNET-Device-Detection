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

namespace FiftyOne.UnitTests.Performance.Lite
{
    [TestClass]
    public class V31Array : Array
    {
        protected override int MaxInitializeTime
        {
            get { return 1000; }
        }

        protected override string DataFile
        {
            get { return Constants.LITE_PATTERN_V31; }
        }

        [TestMethod]
        public void LiteV31Array_Performance_InitializeTime() 
        {
            base.InitializeTime(); 
        }


        [TestMethod]
        public void LiteV31Array_Performance_BadUserAgentsMulti()
        {
            base.BadUserAgentsMulti(null, 1);
        }

        [TestMethod]
        public void LiteV31Array_Performance_BadUserAgentsSingle()
        {
            base.BadUserAgentsSingle(null, 4);
        }

        [TestMethod]
        public void LiteV31Array_Performance_UniqueUserAgentsMulti()
        {
            base.UniqueUserAgentsMulti(null, 1);
        }

        [TestMethod]
        public void LiteV31Array_Performance_UniqueUserAgentsSingle()
        {
            base.UniqueUserAgentsSingle(null, 1);
        }

        [TestMethod]
        public void LiteV31Array_Performance_RandomUserAgentsMulti()
        {
            base.RandomUserAgentsMulti(null, 1);
        }

        [TestMethod]
        public void LiteV31Array_Performance_RandomUserAgentsSingle()
        {
            base.RandomUserAgentsSingle(null, 1);
        }

        [TestMethod]
        public void LiteV31Array_Performance_BadUserAgentsMultiAll()
        {
            base.BadUserAgentsMulti(_dataSet.Properties, 2);
        }

        [TestMethod]
        public void LiteV31Array_Performance_BadUserAgentsSingleAll()
        {
            base.BadUserAgentsSingle(_dataSet.Properties, 3);
        }

        [TestMethod]
        public void LiteV31Array_Performance_UniqueUserAgentsMultiAll()
        {
            base.UniqueUserAgentsMulti(_dataSet.Properties, 1);
        }

        [TestMethod]
        public void LiteV31Array_Performance_UniqueUserAgentsSingleAll()
        {
            base.UniqueUserAgentsSingle(_dataSet.Properties, 1);
        }

        [TestMethod]
        public void LiteV31Array_Performance_RandomUserAgentsMultiAll()
        {
            base.RandomUserAgentsMulti(_dataSet.Properties, 1);
        }

        [TestMethod]
        public void LiteV31Array_Performance_RandomUserAgentsSingleAll()
        {
            base.RandomUserAgentsSingle(_dataSet.Properties, 1);
        }
    }
}
