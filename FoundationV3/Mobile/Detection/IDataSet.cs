using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// Interface that defines a dataset.
    /// </summary>
    public interface IDataSet
    {
    }

    /// <summary>
    /// Interface that defines a dataset used by device 
    /// detection
    /// </summary>
    public interface IDeviceDetectionDataSet : IDataSet
    {
        /// <summary>
        /// The number of signature profiles
        /// </summary>
        int SignatureProfilesCount { get; }

        /// <summary>
        /// The number of signature nodes
        /// </summary>
        int SignatureNodesCount { get; }
    }
}
