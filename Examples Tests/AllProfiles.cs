using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FiftyOne.Example.Illustration.AllProfiles;
using FiftyOne.Tests.Integration;

namespace FiftyOne.Tests.Example
{
    [TestClass]
    public class AllProfiles
    {
        [TestMethod]
        [TestCategory("Example"), TestCategory("Unit")]
        public void LiteExamples_All_Profiles()
        {
            Program.Run(
                Utils.GetDataFile(Constants.LITE_PATTERN_V32));
        }
        [TestMethod]
        [TestCategory("Example"), TestCategory("Unit")]
        public void PremiumExamples_All_Profiles()
        {
            Program.Run(
                Utils.GetDataFile(Constants.PREMIUM_PATTERN_V32));
        }
        [TestMethod]
        [TestCategory("Example"), TestCategory("Unit")]
        public void EnterpriseExamples_All_Profiles()
        {
            Program.Run(
                Utils.GetDataFile(Constants.ENTERPRISE_PATTERN_V32));
        }
    }
}
