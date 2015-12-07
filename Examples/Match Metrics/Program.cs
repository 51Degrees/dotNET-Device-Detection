/**
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited.
 * Copyright (c) 2015 51Degrees Mobile Experts Limited, 5 Charlotte Close,
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
 * This Source Code Form is "Incompatible With Secondary Licenses", as
 * defined by the Mozilla Public License, v. 2.0.
 */

/*
<tutorial>
Match metrics example of using 51Degrees device detection. 
The example shows how to:
<ol>
<li>Set the data set for the 51Degrees detector
<p><pre class="prettyprint lang-cs">
string fileName = args[0];
DataSet dataSet = StreamFactory.Create(fileName, false);
</pre></p>
<li>Instantiate the 51Degrees device detection provider
with these settings
<p><pre class="prettyprint lang-cs">
Provider provider = new Provider(dataSet);
</pre></p>
<li>Produce a match for a single HTTP User-Agent
<p><pre class="prettyprint lang-cs">
match = provider.Match(userAgent);
</pre></p>
<li>Obtain device Id: consists of four components separated by a hyphen 
symbol: Hardware-Platform-Browser-IsCrawler where each Component 
represents an ID of the corresponding Profile.
<p><pre class="prettyprint lang-cs">
deviceId = match.DeviceId;
</pre></p>
<li>Obtain match method: provides information about the 
algorithm that was used to perform detection for a particular User-Agent. 
For more information on what each method means please see: 
<a href="https://51degrees.com/support/documentation/pattern">
How device detection works</a>
<p><pre class="prettyprint lang-cs">method = match.Method;</pre></p>
<li>Obtain difference:  used when detection method is not Exact or None. 
This is an integer value and the larger the value the less confident the 
detector is in this result.
<p><pre class="prettyprint lang-cs">difference = match.Difference;</pre></p>
<li>Obtain signature rank: an integer value that indicates how popular 
the device is. The lower the rank the more popular the signature.
<p><pre class="prettyprint lang-cs">rank =  match.Signature.Rank;</pre></p>
</ol>
This tutorial assumes you are building this from within the
51Degrees Visual Studio solution. Running the executable produced
inside Visual Studio will ensure all the command line arguments
are preset correctly. If you are running outside of Visual Studio,
make sure to add the path to a 51Degrees data file as an argument.
</tutorial>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FiftyOne.Foundation.Mobile.Detection;
using FiftyOne.Foundation.Mobile.Detection.Entities;
using FiftyOne.Foundation.Mobile.Detection.Factories;

namespace FiftyOne.Example.Illustration.MatchMetrics
{
    public class Program
    {
        // Snippet Start
        public static void Run(string fileName)
        {
            // DataSet is the object used to interact with the data file.
            // StreamFactory creates Dataset with pool of binary readers to 
            // perform device lookup using file on disk.
            DataSet dataSet = StreamFactory.Create(fileName, false);

            // Provides access to device detection functions.
            Provider provider = new Provider(dataSet);

            // Used to store and access detection results.
            Match match;

            // User-Agent string of an iPhone mobile device.
            string mobileUserAgent = ("Mozilla/5.0 (iPhone; CPU iPhone " +
                "OS 7_1 like Mac OS X) AppleWebKit/537.51.2 (KHTML, like " +
                "Gecko) 'Version/7.0 Mobile/11D167 Safari/9537.53");

            // User-Agent string of Firefox Web browser version 41 on desktop.
            string desktopUserAgent = ("Mozilla/5.0 (Windows NT 6.3; " +
                "WOW64; rv:41.0) Gecko/20100101 Firefox/41.0");

            // User-Agent string of a MediaHub device.
            string mediaHubUserAgent = ("Mozilla/5.0 (Linux; Android " +
                "4.4.2; X7 Quad Core Build/KOT49H) AppleWebKit/537.36 " +
                "(KHTML, like Gecko) Version/4.0 Chrome/30.0.0.0 " +
                "Safari/537.36");

            Console.WriteLine("Starting Match Metrics Example.");

            // Carries out a match for a mobile User-Agent.
            Console.WriteLine("*** Mobile ***");
            match = provider.Match(mobileUserAgent);
            OutputMatchMetrics(match);

            // Carries out a match for a desktop User-Agent.
            Console.WriteLine("*** Desktop ***");
            match = provider.Match(desktopUserAgent);
            OutputMatchMetrics(match);

            // Carries out a match for a MediaHub User-Agent.
            Console.WriteLine("*** MediaHub ***");
            match = provider.Match(mediaHubUserAgent);
            OutputMatchMetrics(match);

            // Finally close the dataset, releasing resources and file locks.
            dataSet.Dispose();
        }

        /// <summary>
        /// Prints match metrics associated with a given match object.
        /// </summary>
        /// <param name="match">a Match object</param>
        public static void OutputMatchMetrics(Match match)
        {
            Console.WriteLine();
            Console.WriteLine("Target User-Agent:");
            Console.WriteLine(match.TargetUserAgent);
            Console.WriteLine();
            Console.WriteLine("Relevant Sub Strings:");
            Console.WriteLine(match.UserAgent);
            Console.WriteLine();
            Console.WriteLine("Metrics:");
            Console.WriteLine("   Device Id: " + match.DeviceId);
            Console.WriteLine("   Method: " + match.Method);
            Console.WriteLine("   Difference: " + match.Difference);
            Console.WriteLine("   Rank: " + match.Signature.Rank);
            Console.WriteLine();
        }
        // Snippet End

        static void Main(string[] args)
        {
            string fileName = args.Length > 0 ? args[0] : "../../../../../data/51Degrees-LiteV3.2.dat";
            Run(fileName);

            // Waits for a character to be pressed.
            Console.ReadKey();
        }
    }
}
