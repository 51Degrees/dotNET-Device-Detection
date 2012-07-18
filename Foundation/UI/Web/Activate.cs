/* *********************************************************************
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0.
 * 
 * If a copy of the MPL was not distributed with this file, You can obtain
 * one at http://mozilla.org/MPL/2.0/.
 * 
 * This Source Code Form is “Incompatible With Secondary Licenses”, as
 * defined by the Mozilla Public License, v. 2.0.
 * ********************************************************************* */

using System;

namespace FiftyOne.Foundation.UI.Web
{
    /// <summary>
    /// Display a user interface to enable the user to enter a Premium license key and upgrade Foundation
    /// to the premium product. If the fiftyOne/detection element does not exist in the configuration
    /// this element is added and the binaryFilePath set to "51Degrees-Premium.dat". The licence key will 
    /// be written to a file called 51Degrees.mobi.lic in the bin folder.
    /// If the site is running in medium trust mode the operation will fail and a message will be displayed
    /// to the user.
    /// The control also contains a check box to enable / disable the sharing of usage information with
    /// 51Degrees.mobi.
    /// </summary>
    [Obsolete("Use the Detection user control. " +
    "This control now includes functionality not entirely related to activating premium data.")]
    public class Activate : Detection
    {
    }
}
