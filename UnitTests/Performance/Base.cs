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
using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using FiftyOne.Foundation.Mobile.Detection.Entities;
using FiftyOne.Foundation.Mobile.Detection.Factories;

namespace FiftyOne.UnitTests.Performance
{
    [TestClass]
    public abstract class Base
    {
        /// <summary>
        /// The data set to be used for the tests.
        /// </summary>
        protected DataSet _dataSet;

        protected abstract string DataFile { get; }

        protected TimeSpan _testInitializeTime;

        protected virtual Utils.Results UniqueUserAgentsSingle()
        {
            return Utils.DetectLoopSingleThreaded(
                _dataSet,
                File.ReadAllLines(Constants.GOOD_USERAGENTS_FILE),
                Utils.DoNothing,
                null);
        }

        protected virtual Utils.Results DuplicatedUserAgentsSingle()
        {
            return Utils.DetectLoopSingleThreaded(
                _dataSet,
                UserAgentGenerator.GetEnumerable(Constants.USERAGENT_COUNT, 0),
                Utils.DoNothing,
                null);
        }

        protected virtual Utils.Results BadUserAgentsSingle()
        {
            return Utils.DetectLoopSingleThreaded(
                _dataSet,
                UserAgentGenerator.GetEnumerable(Constants.USERAGENT_COUNT, 10),
                Utils.DoNothing,
                null);
        }

        protected virtual Utils.Results UniqueUserAgentsMulti()
        {
            return Utils.DetectLoopMultiThreaded(
                _dataSet,
                File.ReadAllLines(Constants.GOOD_USERAGENTS_FILE),
                Utils.DoNothing,
                null);
        }

        protected virtual Utils.Results DuplicatedUserAgentsMulti()
        {
            return Utils.DetectLoopMultiThreaded(
                _dataSet,
                UserAgentGenerator.GetEnumerable(Constants.USERAGENT_COUNT, 0),
                Utils.DoNothing,
                null);
        }

        protected virtual Utils.Results BadUserAgentsMulti()
        {
            return Utils.DetectLoopMultiThreaded(
                _dataSet,
                UserAgentGenerator.GetEnumerable(Constants.USERAGENT_COUNT, 10),
                Utils.DoNothing,
                null);
        }


        protected virtual Utils.Results RandomUserAgentsMulti()
        {
            return Utils.DetectLoopMultiThreaded(
                _dataSet,
                UserAgentGenerator.GetEnumerable(20000, 0),
                Utils.DoNothing,
                null);
        }

        protected virtual Utils.Results RandomUserAgentsSingle()
        {
            return Utils.DetectLoopSingleThreaded(
                _dataSet,
                UserAgentGenerator.GetEnumerable(20000, 0),
                Utils.DoNothing,
                null);
        }

        [TestCleanup]
        public void Dispose()
        {
            if (_dataSet != null)
            {
                _dataSet.Dispose();
            }
        }
    }
}
