/*
* Copyright © 2010 - 2017 51 Degrees Mobile Experts Limited. All rights reserved.
*/

using System;
using System.Text;

namespace FiftyOne.Foundation.Properties
{
    internal class LicenceConstants
    {
        /// <summary>
        /// The size of the key to use for signatures.
        /// </summary>
        internal const int KeySize = 384;
        
        /// <summary>
        /// Valid device data products Ids. Premium, Enterprise and Basic.
        /// </summary>
        internal static readonly byte[] ProductIDs = new byte[] { 2, 4, 7, 8, 9 };

        /// <summary>
        /// Key to verify signature of licence.
        /// </summary>
        internal const string PublicKey = "<DSAKeyValue><P>kwGfa9rgUT8XBPu3rWZzT/pTaBWQ+ls9ijGlv9bE52mf72AnI27zm15CnnwB/M6da27m2O1T1mjDBfovGnfyPsk4JpPFcQ2ny9CDyDn2rFL4E3ABQce1ogD3UfHYYSlfnbGo/mZJ1k3iFrk/FOWnMHErq8B68BL19BJua7DoQks=</P><Q>0g1iAE6Y5g6ynE14T6VVWfkCXIM=</Q><G>VyT7/BvtfYB6P4LTkOG9quI0inKt7khBKpkgMRvP+ZtMwMrKOuib9Cwb3hNo7vY1of8gk0JKetsg6luIH3R5xPBi75LujLYUiQzECM65w+xqfAi5osgunbXi19DAtAaCs379CZ4I3fNKneDflHclj1onCrz3tbP1MYVUDBVx9X8=</G><Y>HgOB8RPFg/s5Uh50joZFEktsiDcSdo0yklR2wMl5F5kZWX3TXpmFL6JY/ZxADNKHeT8kzEjEmnsWJzsNEeuCOx/93GM8bmQnSDFAaOXU0FnH+Rt/CrcbFP2u4ElFOCOG+2+qCFdmJ+z17fVJNU+EVCgOtcVHwjK1JxaGzszqqXs=</Y><J>synCsupc4lzKH5sZiqc4CIsueCAnx5BH8fK3/hOlpe89uonnW4YwpFcs46QRmqDNRGKc1oRUkXOioEcgGKmcAYIlgfeMhvw+KedVp6qyxZSZmCU0wPMFx5fmfJgiqp8DbksaxgWVT/VOBdZu</J><Seed>IcsejGCLVZK6T4+WBNlMSsrLOQw=</Seed><PgenCounter>JA==</PgenCounter></DSAKeyValue>";
        
        /// <summary>
        /// Header name to expect for interogation process
        /// </summary>
        internal const string InterogationHeader = "51D";

        /// <summary>
        /// Base date to count days Licence is valid for
        /// It represent 1st Nov 2010
        /// </summary>
        internal const int LicenceBaseDateOffset = 734076;

        /// <summary>
        /// Text used with the mobile page if the Licence is a trial.
        /// </summary>
        internal const string TrialLicenceText = "Trial Licence: {0} Days Remaining";

    }
}
