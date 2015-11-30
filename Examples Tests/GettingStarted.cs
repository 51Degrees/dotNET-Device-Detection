using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FiftyOne.Example.Illustration.GettingStarted;
using FiftyOne.Tests.Integration;

namespace FiftyOne.Tests.Example
{
    [TestClass]
    public class GettingStarted
    {
        [TestMethod]
        [TestCategory("Example"), TestCategory("Unit")]
        public void LiteExamples_Getting_Started()
        {
            Program.Run(Utils.GetDataFile(Constants.LITE_PATTERN_V32));
        }
        [TestMethod]
        [TestCategory("Example"), TestCategory("Unit")]
        public void PremiumExamples_Getting_Started()
        {
            Program.Run(Utils.GetDataFile(Constants.PREMIUM_PATTERN_V32));
        }
        [TestMethod]
        [TestCategory("Example"), TestCategory("Unit")]
        public void EnterpriseExamples_Getting_Started()
        {
            Program.Run(Utils.GetDataFile(Constants.ENTERPRISE_PATTERN_V32));
        }
    }
}
