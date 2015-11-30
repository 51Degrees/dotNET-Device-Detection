using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FiftyOne.Example.Illustration.MatchMetrics;
using FiftyOne.Tests.Integration;

namespace FiftyOne.Tests.Example
{
    [TestClass]
    public class MatchMetrics
    {
        [TestMethod]
        [TestCategory("Example"), TestCategory("Unit")]
        public void LiteExamples_Match_Metrics()
        {
            Program.Run(Utils.GetDataFile(Constants.LITE_PATTERN_V32));
        }
        [TestMethod]
        [TestCategory("Example"), TestCategory("Unit")]
        public void PremiumExamples_Match_Metrics()
        {
            Program.Run(Utils.GetDataFile(Constants.PREMIUM_PATTERN_V32));
        }
        [TestMethod]
        [TestCategory("Example"), TestCategory("Unit")]
        public void EnterpriseExamples_Match_Metrics()
        {
            Program.Run(Utils.GetDataFile(Constants.ENTERPRISE_PATTERN_V32));
        }
    }
}
