/**
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited.
 * Copyright (c) 2017 51Degrees Mobile Experts Limited, 5 Charlotte Close,
 * Caversham, Reading, Berkshire, United Kingdom RG4 7BY
 * 
 * This Source Code Form is the subject of the following patents and patent 
 * applications, owned by 51Degrees Mobile Experts Limited of 5 Charlotte
 * Close, Caversham, Reading, Berkshire, United Kingdom RG4 7BY:
 * European Patent No. 2871816;
 * European Patent Application No. 17184134.9;
 * United States Patent Nos. 9,332,086 and 9,350,823; and
 * United States Patent Application No. 15/686,066.
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
All profiles example of using 51Degrees device detection. 
The example shows how to:
<ol>
<li>Initialise the 51Degrees data set
<p><pre class="prettyprint lang-cs">
string fileName = args[0];
DataSet dataSet = StreamFactory.Create(fileName, false);
</pre></p>
<li>Open an output file to write the results to
<p><pre class="prettyprint lang-cs">
StreamWriter fout = new StreamWriter(outputFile);
</pre></p>
<li>Write a header to the output file with the property names in 
<p><pre class="prettyprint lang-cs">
fout.Write("Id");
foreach (Property property in hardwareProperties)
{
    fout.Write("," + property.Name);
}
fout.Write("\n");
</pre></p>
<li>Get all hardware profiles as an array
<p><pre class="prettyprint lang-cs">
Profile[] hardwareProfiles = dataSet.Hardware.Profiles;
</pre></p>
<li>Get the values of a required property from a profile to write
to the CSV file
<p><pre class="prettyprint lang-cs">
foreach (Value value in profile.Values)
{
    if (value.Property == property)
    {
        // Append this value to the string.
        values += value.ToString();
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
using System.Linq;
using FiftyOne.Foundation.Mobile.Detection.Entities;
using FiftyOne.Foundation.Mobile.Detection.Factories;
using System.IO;

namespace FiftyOne.Example.Illustration.AllProfiles
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
            
            // Name of the output file results will be saved to.
            string outputFile = "AllProfilesOutput.csv";

            // Get all the hardware profiles from the hardware component.
            Profile[] hardwareProfiles = dataSet.Hardware.Profiles;

            // Get all the hardware properties from the hardware component.
            // For the full list of properties see:
            // https://51degrees.com/resources/property-dictionary
            Property[] hardwareProperties = dataSet.Hardware.Properties;

            // Opens and output file.
            StreamWriter fout = new StreamWriter(outputFile);

            Console.WriteLine("Starting All Profiles Example.");

            // Print CSV headers to output file.
            fout.Write("Id");
            foreach (Property property in hardwareProperties)
            {
                fout.Write("," + property.Name);
            }
            fout.Write("\n");

            // Loop over all devices.
            foreach (Profile profile in hardwareProfiles)
            {
                // Write the device's profile id.
                fout.Write(profile.ProfileId);

                foreach (Property property in hardwareProperties)
                {
                    // Print the profile's values for the property.
                    fout.Write(",");
                    fout.Write(getProfileValues(profile, property));
                }
                fout.Write("\n");
            }

            fout.Close();

            Console.WriteLine("Output Written to " + outputFile);

            // Finally close the dataset, releasing resources and file locks.
            dataSet.Dispose();
        }

        /// <summary>
        /// Constructs a "|" separated string of all the values the profile has
        /// which relate to the supplied property.
        /// </summary>
        /// <param name="profile">Profile to get the values from.</param>
        /// <param name="property">Property to get the values for.</param>
        /// <returns>string of properties for the requested profile
        /// and property.</returns>
        static string getProfileValues(Profile profile, Property property)
        {
            string values = "";

            if (property.Category == "Javascript")
            {
                // Prevents big chunks of javascript overrides from
                // being writen.
                return "N/A";
            }

            bool firstValue = true;

            // Look though all the values in the profile and check if
            // they relate to the requested property.
            foreach (Value value in profile.Values)
            {
                if (value.Property == property)
                {
                    if (!firstValue)
                    {
                        // There are multiple values, so separate them.
                        values += "|";
                    }
                    else
                    {
                        firstValue = false;
                    }
                    // Append this value to the string.
                    values += value.ToString();
                }
            }
            
            // Return the values string.
            return values;
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