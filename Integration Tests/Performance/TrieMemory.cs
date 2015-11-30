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

namespace FiftyOne.Tests.Integration.Performance
{
    [TestClass]
    public abstract class TrieMemory : TrieBase
    {
        [TestInitialize()]
        [ExpectedException(typeof(OutOfMemoryException))]
        public void CreateDataSet()
        {
            var start = DateTime.UtcNow;
            _testInitializeTime = DateTime.UtcNow - start;
            Utils.CheckFileExists(DataFile);
            var file = new FileInfo(DataFile);
            try
            {
                var array = new byte[file.Length];
                using (var stream = file.OpenRead())
                {
                    stream.Read(array, 0, (int)file.Length);
                }
                _provider = TrieFactory.Create(array);
            }
            catch(OutOfMemoryException)
            {
                Assert.Inconclusive(
                    "Not enough memory to perform memory test on data file '{0}' of size '{1}'MB",
                    file.Name,
                    file.Length / (1024 * 1024));
            }
            _testInitializeTime = DateTime.UtcNow - start;
        }
    }
}
