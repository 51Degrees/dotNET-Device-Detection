using FiftyOne.Foundation.Mobile.Detection.Factories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiftyOne.Tests.Integration.Cache
{
    [TestClass]
    public abstract class FileTest : Base
    {
        [TestInitialize()]
        public void CreateDataSet()
        {
            Utils.CheckFileExists(DataFile);
            _dataSet = StreamFactory.Create(DataFile);
        }
    }
}
