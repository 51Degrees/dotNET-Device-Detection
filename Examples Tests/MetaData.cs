using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FiftyOne.Example.Illustration.MetaData;
using FiftyOne.Tests.Integration;

namespace FiftyOne.Tests.Example
{
    [TestClass]
    public class MetaData
    {
        [TestMethod]
        [TestCategory("Example"), TestCategory("Unit")]
        public void LiteExamples_Meta_Data()
        {
            Program.Run(Utils.GetDataFile(Constants.LITE_PATTERN_V32));
        }
        [TestMethod]
        [TestCategory("Example"), TestCategory("Unit")]
        public void PremiumExamples_Meta_Data()
        {
            Program.Run(Utils.GetDataFile(Constants.PREMIUM_PATTERN_V32));
        }
        [TestMethod]
        [TestCategory("Example"), TestCategory("Unit")]
        public void EnterpriseExamples_Meta_Data()
        {
            Program.Run(Utils.GetDataFile(Constants.ENTERPRISE_PATTERN_V32));
        }
    }
}
