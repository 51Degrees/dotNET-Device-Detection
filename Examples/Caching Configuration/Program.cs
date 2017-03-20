/*
<tutorial>
Find profiles example of using 51Degrees device detection. 
The example shows how to:
<ol>
<li>Use the DataSetBuilder to create a 51Degrees data set that will use a
custom caching configuration.
<p><pre class="prettyprint lang-cs">
string fileName = args[0];
DataSet dataSet = DataSetBuilder.File()                
                .ConfigureCachesFromTemplate(
                    DataSetBuilder.CacheTemplate.SingleThreadLowMemory)
                .SetCacheBuilder(CacheType.SignaturesCache, new CustomCacheBuilder())
                .SetCacheBuilder(CacheType.ProfilesCache, null)
                .SetTempFile(false)
                .Build(fileName)
</pre></p>
</ol>
This tutorial assumes you are building this from within the
51Degrees Visual Studio solution. Running the executable produced
inside Visual Studio will ensure all the command line arguments
are preset correctly. If you are running outside of Visual Studio,
make sure to add the path to a 51Degrees data file as an argument.
</tutorial>
*/

/**
* This Source Code Form is copyright of 51Degrees Mobile Experts Limited.
* Copyright (c) 2017 51Degrees Mobile Experts Limited, 5 Charlotte Close,
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
using System;
using FiftyOne.Foundation.Mobile.Detection;
using FiftyOne.Foundation.Mobile.Detection.Entities;

namespace Caching_Configuration
{
    class Program
    {
        // Snippet Start
        public static void Run(string fileName)
        {
            // DataSet is the object used to interact with the data file.
            // DataSetBuilder creates a DataSet with a pool of binary 
            // readers to perform device lookup using file on disk.
            // The type is disposable and is therefore contained in using 
            // block to ensure file handles and resources are freed.
            using (DataSet dataSet = DataSetBuilder.File()
                // First, the SingleThreadLowMemory configuration template
                // is specified in order to provide initial values for the 
                // cache settings.
                .ConfigureCachesFromTemplate(
                    DataSetBuilder.CacheTemplate.SingleThreadLowMemory)
                // A custom cache implementation is specified to store signatures.
                .SetCacheBuilder(CacheType.SignaturesCache, new CustomCacheBuilder())
                // A null cache is specified for profiles in order to reduce
                // memory usage. This means that profiles will always be
                // loaded from disk when needed.
                .SetCacheBuilder(CacheType.ProfilesCache, null)
                .SetTempFile(false)
                .Build(fileName))
            {
                // Create a provider using the dataset.
                // The provider handles the process of finding a match
                // in the dataset for a given user agent.
                using (Provider provider = new Provider(dataSet))
                {
                    Console.WriteLine("Staring Caching Configuration Example.");

                    // Used to store and access detection results.
                    Match match;

                    // Contains detection result for the IsMobile property.
                    string IsMobile;

                    // User-Agent string of an iPhone mobile device.
                    string mobileUserAgent = ("Mozilla/5.0 (iPhone; CPU iPhone " +
                        "OS 7_1 like Mac OS X) AppleWebKit/537.51.2 (KHTML, like " +
                        "Gecko) 'Version/7.0 Mobile/11D167 Safari/9537.53");

                    // Get a match object and display the IsMobile property
                    match = provider.Match(mobileUserAgent);
                    IsMobile = match["IsMobile"].ToString();
                    Console.WriteLine("   IsMobile: " + IsMobile);
                }
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
