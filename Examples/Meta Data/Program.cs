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
Example of listing properties and possible values from a Dataset
The example illustrates:
<ol>
<li>Initialize the data set
<p><pre class="prettyprint lang-cs">
DataSet dataSet = StreamFactory.Create(fileName, false);
</pre></p>
<li>Fetch and print the name and description of all 
properties contained in the data set
<p><pre class="prettyprint lang-cs">
foreach (var property in dataSet.Properties) 
{
    Console.WriteLine(property.Name + " - " + 
        property.Description);
}
</pre></p>
<li>Fetch and print the possible values and description 
(if available) for a given property
<p><pre class="prettyprint lang-cs">
foreach (var property in dataSet.Properties) 
{
    Console.WriteLine(property.Name + " - " + 
        property.Description);

    foreach (var value in property.Values)
    {
        sb.Append(" - ");
        sb.Append(value.Name);
        if (value.Description != null)
        {
            sb.Append(" - ");
            sb.Append(value.Description);
        }
        sb.Append("\n");
        Console.WriteLine(sb.ToString());
        sb.Clear();
    }
}
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

namespace FiftyOne.Example.Illustration.MetaData
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
            StringBuilder sb = new StringBuilder();

            Console.WriteLine("Starting Mata Data Example");

            // Loops over all properties.
            foreach (var property in dataSet.Properties) 
            {
                // Print property name and description.
                Console.WriteLine(property.Name + " - " + 
                    property.Description);

                // For each of the values of the current property.
                foreach (var value in property.Values)
                {
                    // Print value name.
                    sb.Append(" - ");
                    sb.Append(value.Name);
                    // If value has a description add it.
                    if (value.Description != null)
                    {
                        sb.Append(" - ");
                        sb.Append(value.Description);
                    }
                    sb.Append("\n");
                    // Print value and reset string builder.
                    Console.WriteLine(sb.ToString());
                    sb.Clear();
                }
            }

            // Finally close the dataset, releasing resources and file locks.
            dataSet.Dispose();
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
