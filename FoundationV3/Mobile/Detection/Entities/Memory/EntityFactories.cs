using FiftyOne.Foundation.Mobile.Detection.Factories;
using FiftyOne.Foundation.Mobile.Detection.Readers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FiftyOne.Foundation.Mobile.Detection.Entities.Memory
{
    internal class NodeMemoryFactory : NodeFactory
    {
        protected override Entities.Node Construct(DataSet dataSet, int offset, Reader reader)
        {
            return new Node(dataSet, offset, reader);
        }
    }

    internal class ProfileMemoryFactory : ProfileFactory
    {
        protected override Entities.Profile Construct(DataSet dataSet, int offset, Reader reader)
        {
            return new Entities.Memory.Profile(dataSet, offset, reader);
        }
    }
}
