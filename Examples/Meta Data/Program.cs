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
Example of listing properties and possible values from a Dataset<br>
The example illustrates:
<ol>
<li>Initialize the data set
<p><code>
DataSet dataSet = StreamFactory.Create(fileName, false);
</code></p>
<li>Fetch and print the name and description of all 
properties contained in the data set
<p><code>
for ( i = 0; i < dataSet.Properties.Count; i++ )<br>
{<br>
    Console.WriteLine( dataSet.Properties[i].Name<br>
        + " - "<br>
        + dataSet.Properties[i].Description)<br>
}
</code></p>
<li>fetch and print the possible values and description 
(if available) for a given property
<p><code>
for ( j = 0; j < dataSet.Properties[i].Values.Count; j++ )<br>
{<br>
    if (dataSet.Properties[i].Values[j].Description != null)<br>
        Console.WriteLine("   "<br>
            + dataSet.Properties[i].Values[j].Name<br>
            + " - "<br>
            + dataSet.Properties[i].Values[j].Description);<br>
    else<br>
        Console.WriteLine("   "<br>
            + dataSet.Properties[i].Values[j].Name);<br>
}
</code></p>
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

namespace FiftyOne.Example.Illustration.MetaData
{
    public class Program
    {
        // Snippet Start
        public static void Run(string fileName)
        {
            int i, j;

            // Initializes the data set.
            DataSet dataSet = StreamFactory.Create(fileName, false);

            Console.WriteLine("Starting Mata Data Example");

            // Loops over all properties.
            for (i = 0; i < dataSet.Properties.Count; i++ )
            {
                // Prints the property name and description.
                Console.WriteLine("\n" + dataSet.Properties[i].Name + " - " +
                    dataSet.Properties[i].Description);

                // Prints the possible values of the property
                // and a description if available.
                for (j = 0; j < dataSet.Properties[i].Values.Count; j++ )
                {
                    if (dataSet.Properties[i].Values[j].Description != null)
                        Console.WriteLine("   "
                            + dataSet.Properties[i].Values[j].Name
                            + " - "
                            + dataSet.Properties[i].Values[j].Description);
                    else
                        Console.WriteLine("   "
                            + dataSet.Properties[i].Values[j].Name);
                }
            }
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
