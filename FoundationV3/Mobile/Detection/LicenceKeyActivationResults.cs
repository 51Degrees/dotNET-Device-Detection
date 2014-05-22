/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2014 51Degrees Mobile Experts Limited, 5 Charlotte Close,
 * Caversham, Reading, Berkshire, United Kingdom RG4 7BY
 * 
 * This Source Code Form is the subject of the following patent 
 * applications, owned by 51Degrees Mobile Experts Limited of 5 Charlotte
 * Close, Caversham, Reading, Berkshire, United Kingdom RG4 7BY: 
 * European Patent Application No. 13192291.6; and
 * United States Patent Application Nos. 14/085,223 and 14/085,301.
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0.
 * 
 * If a copy of the MPL was not distributed with this file, You can obtain
 * one at http://mozilla.org/MPL/2.0/.
 * 
 * This Source Code Form is “Incompatible With Secondary Licenses”, as
 * defined by the Mozilla Public License, v. 2.0.
 * ********************************************************************* */

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// Enumeration of reasons why activation could fail.
    /// </summary>
    public enum LicenceKeyResults
    {
        /// <summary>
        /// The licence key was activated successfully for this web site.
        /// </summary>
        Success = 1,
        /// <summary>
        /// An HTTPS connection could not be established with the validation service.
        /// </summary>
        Https = 2,
        /// <summary>
        /// The licence key is invalid.
        /// </summary>
        Invalid = 3,
        /// <summary>
        /// The configuration file could not be altered.
        /// </summary>
        Config = 4,
        /// <summary>
        /// The licence file could not be written to the bin folder.
        /// </summary>
        WriteLicenceFile = 5,
        /// <summary>
        /// The data file could not be written to the folder.
        /// </summary>
        WriteDataFile = 6,
        /// <summary>
        /// The licence key could not be activated for an unknown reason.
        /// </summary>
        GenericFailure = 7,
        /// <summary>
        /// The source stream could not be read from.
        /// </summary>
        StreamFailure = 8,
        /// <summary>
        /// The stream does not contain valid data.
        /// </summary>
        DataInvalid = 9
    }
}
