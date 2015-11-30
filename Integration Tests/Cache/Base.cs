using FiftyOne.Foundation.Mobile.Detection;
using FiftyOne.Foundation.Mobile.Detection.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiftyOne.Tests.Integration.Cache
{
    [TestClass]
    public abstract class Base
    {
        /// <summary>
        /// The data set to be used for the tests.
        /// </summary>
        protected DataSet _dataSet;

        /// <summary>
        /// Name of the data file to use for the tests.
        /// </summary>
        protected abstract string DataFile { get; }

        private void UserAgentsSingle(IEnumerable<string> userAgents, int cacheSize, double minMisses, double maxMisses)
        {
            var provider = new Provider(_dataSet, cacheSize);
            var results = Utils.DetectLoopSingleThreaded(
                provider,
                userAgents,
                Utils.RetrievePropertyValues,
                _dataSet.Properties);
            Assert.IsTrue(provider.PercentageCacheMisses >= minMisses &&
                provider.PercentageCacheMisses <= maxMisses, String.Format(
                "Cache misses of '{0:P2}' outside expected range of '{1:P2}' to '{2:P2}'.",
                provider.PercentageCacheMisses,
                minMisses,
                maxMisses));
        }

        protected void TinyCache()
        {
            UserAgentsSingle(UserAgentGenerator.GetRepeatingUserAgents(10000, 0, 3), 1000, 0.94, 0.96);
        }

        protected void SmallCache()
        {
            UserAgentsSingle(UserAgentGenerator.GetRepeatingUserAgents(20000, 0, 6), 10000, 0.515, 0.535);
        }

        protected void LargeCache()
        {
            UserAgentsSingle(UserAgentGenerator.GetRepeatingUserAgents(20000, 0, 6), 40000, 0.1, 0.11);
        }

        protected void NoCache()
        {
            UserAgentsSingle(UserAgentGenerator.GetRepeatingUserAgents(20000, 0, 2), 0, 0, 0);
        }

        [TestCleanup]
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_dataSet != null)
            {
                _dataSet.Dispose();
            }
        }
    }
}
