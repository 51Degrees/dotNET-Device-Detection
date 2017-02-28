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
Find profiles example of using 51Degrees device detection. 
The example shows how to:
<ol>
<li>Configure the 51Degrees data set to give optimal performance 
when calling FindProfiles.
<p><pre class="prettyprint lang-cs">
string fileName = args[0];
DataSet dataSet = DataSetBuilder.File()
                .ConfigureDefaultCaches()
                .SetCacheSize(CacheType.ValuesCache, 200000)
                .SetTempFile(false)
                .Build(fileName)
</pre></p>
<li>Retrive all the profiles for a specified property value pair.
<p><pre class="prettyprint lang-cs">
profiles = dataSet.FindProfiles("IsMobile", "True");
</pre></p>
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
using FiftyOne.Foundation.Mobile.Detection.Caching;

namespace FiftyOne.Example.Illustration.FindProfiles
{
    public class Program
    {
        // Snippet Start
        public static void Run(string fileName)
        {
            // DataSet is the object used to interact with the data file.
            // DataSetBuilder creates Dataset with pool of binary readers to 
            // perform device lookup using file on disk. The type is 
            // disposable and is therefore contained in using block to 
            // ensure file handles and resources are freed.
            using (DataSet dataSet = DataSetBuilder.File()
                .ConfigureDefaultCaches()
                // Set the cache size for the Values cache to 200,000
                // This is done because FindProfiles performs significantly
                // faster when all Value objects can be held in memory.
                // The number of Value objects varies by data file type:
                // Lite < 5000
                // Premium < 180,000
                // Enterprise < 200,000
                .SetCacheSize(CacheType.ValuesCache, 200000)
                .SetTempFile(false)
                .Build(fileName))
            {
                Console.WriteLine("Staring Find Profiles Example.");

                // Retrieve all mobile profiles from the data set.
                Profile[] profiles = dataSet.FindProfiles("IsMobile", "True");
                Console.WriteLine("There are " + profiles.Count()
                    + " mobile profiles in the " + dataSet.Name
                    + " data set.");

                // Retrieve all non-mobile profiles from the data set.
                profiles = dataSet.FindProfiles("IsMobile", "False");
                Console.WriteLine("There are " + profiles.Count()
                    + " non-mobile profiles in the " + dataSet.Name
                    + " data set.");
            }
        }
        // Snippet End

        static void Main(string[] args)
        {
            string fileName = args.Length > 0 ? args[0] : "../../../../data/51Degrees-LiteV3.2.dat";
            Run(fileName);

            // Waits for a character to be pressed.
            Console.ReadKey();
        }
    }
}