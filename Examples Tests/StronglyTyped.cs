using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FiftyOne.Example.Illustration.StronglyTyped;
using FiftyOne.Tests.Integration;

namespace FiftyOne.Tests.Example
{
    [TestClass]
    public class StronglyTyped
    {
        [TestMethod]
        [TestCategory("Example"), TestCategory("Unit")]
        public void LiteExamples_Strongly_Typed()
        {
            Program.Run(Utils.GetDataFile(Constants.LITE_PATTERN_V32));
        }
        [TestMethod]
        [TestCategory("Example"), TestCategory("Unit")]
        public void PremiumExamples_Strongly_Typed()
        {
            Program.Run(Utils.GetDataFile(Constants.PREMIUM_PATTERN_V32));
        }
        [TestMethod]
        [TestCategory("Example"), TestCategory("Unit")]
        public void EnterpriseExamples_Strongly_Typed()
        {
            Program.Run(Utils.GetDataFile(Constants.ENTERPRISE_PATTERN_V32));
        }
    }
}
