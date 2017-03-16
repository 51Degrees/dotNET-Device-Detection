/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2017 51Degrees Mobile Experts Limited, 5 Charlotte Close,
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

namespace FiftyOne.Tests.Integration.MetaData.Enterprise
{
    [TestClass]
    public class V31File : Base
    {
        protected override string DataFile
        {
            get { return Utils.GetDataFile(Constants.ENTERPRISE_PATTERN_V31); }
        }

        [TestInitialize()]
        public void CreateDataSet()
        {
            Utils.CheckFileExists(DataFile);
            _dataSet = StreamFactory.Create(DataFile);
        }

        [TestMethod]
        public void EnterpriseV31File_RetrieveComponents() { base.RetrieveComponents(); }
        
        [TestMethod]
        public void EnterpriseV31File_RetrieveProperties() { base.RetrieveProperties(); }

        [TestMethod]
        public void EnterpriseV31File_RetrieveValues() { base.RetrieveValues(); }

        [TestMethod]
        public void EnterpriseV31File_CheckPropertyCount() { base.CheckPropertyCount(160);  }

        [TestMethod]
        public void EnterpriseV31File_ValidatePropertiesHaveDescription() { base.ValidatePropertiesHaveDescription();  }
    }
}
