using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FiftyOne.Example.Illustration.OfflineProcessing;
using FiftyOne.Tests.Integration;

namespace FiftyOne.Tests.Example
{
    [TestClass]
    public class OfflineProcessing
    {
        [TestMethod]
        [TestCategory("Example"), TestCategory("Unit")]
        public void LiteExamples_Offline_Processing()
        {
            Program.Run(
                Utils.GetDataFile(Constants.LITE_PATTERN_V32),
                Utils.GetDataFile(Constants.GOOD_USERAGENTS_FILE));
        }
        [TestMethod]
        [TestCategory("Example"), TestCategory("Unit")]
        public void PremiumExamples_Offline_Processing()
        {
            Program.Run(
                Utils.GetDataFile(Constants.PREMIUM_PATTERN_V32),
                Utils.GetDataFile(Constants.GOOD_USERAGENTS_FILE));
        }
        [TestMethod]
        [TestCategory("Example"), TestCategory("Unit")]
        public void EnterpriseExamples_Offline_Processing()
        {
            Program.Run(
                Utils.GetDataFile(Constants.ENTERPRISE_PATTERN_V32),
                Utils.GetDataFile(Constants.GOOD_USERAGENTS_FILE));
        }
    }
}
