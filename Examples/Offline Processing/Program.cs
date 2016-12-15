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
Offline processing example of using 51Degrees device detection. 
The example shows how to:
<ol>
<li>Set the various settings for the 51Degrees detector
<p><pre class="prettyprint lang-cs">
string fileName = args[0];
DataSet dataSet = StreamFactory.Create(fileName, false);
string[] properties = {"IsMobile","PlatformName","PlatformVersion"};
</pre></p>
<li>Instantiate the 51Degrees device detection provider
with these settings
<p><pre class="prettyprint lang-cs">
Provider provider = new Provider(dataSet);
</pre></p>
<li>Open an input file with a list of User-Agents, and an output file,
<p><pre class="prettyprint lang-cs">
StreamReader fin = new StreamReader(inputFile);
StreamWriter fout = new StreamWriter(outputFile);
</pre></p>
<li>Write a header to the output file with the property names in '|'
separated CSV format ('|' separated because some User-Agents contain
commas)
<p><pre class="prettyprint lang-cs">
fout.Write("User-Agent");
for (i = 0; i &lt; properties.Count(); i++ )
{
    fout.Write("|" + properties[i]);
}
fout.Write("\n");
</pre></p>
<li>For the first 20 User-Agents in the input file, perform a match then
write the User-Agent along with the values for chosen properties to
the CSV.
<p><pre class="prettyprint lang-cs">
while ((userAgent = fin.ReadLine()) != null &&
            currentLine &lt; linesToRead)
{
    provider.Match(userAgent, match);
    fout.Write(userAgent);
    foreach (var property in properties)
    {
        fout.Write("|" + match[property]);
    }
    fout.Write("\n");
    currentLine++;
}
</pre></p>
</ol>
This tutorial assumes you are building this from within the
51Degrees Visual Studio solution. Running the executable produced
inside Visual Studio will ensure all the command line arguments
are preset correctly. If you are running outside of Visual Studio,
make sure to add the path to a 51Degrees data file and 
"20000 User Agents.csv" as arguments.
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
using System.IO;

namespace FiftyOne.Example.Illustration.OfflineProcessing
{
    public class Program
    {
        // Snippet Start
        public static void Run(string fileName, string inputFile)
        {
            // DataSet is the object used to interact with the data file.
            // StreamFactory creates Dataset with pool of binary readers to 
            // perform device lookup using file on disk.
            DataSet dataSet = StreamFactory.Create(fileName, false);

            // Provides access to device detection functions.
            Provider provider = new Provider(dataSet);

            // Used to store and access detection results.
            // Same match will be reused multiple times to avoid unnecessary 
            // object creation.
            Match match = provider.CreateMatch();;

            // Name of the output file results will be saved to.
            string outputFile = "OfflineProcessingOutput.csv";

            // How many lines of the input CSV file to read.
            const int linesToRead = 20;

            // Line currently being read.
            int currentLine = 0;

            // HTTP User-Agent string to match, taken from input the CSV file.
            string userAgent;

            // 51Degrees properties to append the CSV file with.
            // For the full list of properties see:
            // https://51degrees.com/resources/property-dictionary
            string[] properties = { "IsMobile", "PlatformName", "PlatformVersion" };

            // Opens input and output files.
            StreamReader fin = new StreamReader(inputFile);
            StreamWriter fout = new StreamWriter(outputFile);

            Console.WriteLine("Starting Offline Processing Example.");

            // Print CSV headers to output file.
            fout.Write("User-Agent");
            for (int i = 0; i < properties.Count(); i++)
            {
                fout.Write("|" + properties[i]);
            }
            fout.Write("\n");

            while ((userAgent = fin.ReadLine()) != null &&
                     currentLine < linesToRead)
            {
                // Match current User-Agent. Match object reused.
                provider.Match(userAgent, match);
                // Write the User-Agent.
                fout.Write(userAgent);
                // Append properties.
                foreach (var property in properties)
                {
                    fout.Write("|" + match[property]);
                }
                fout.Write("\n");
                currentLine++;
            }

            fin.Close();
            fout.Close();

            Console.WriteLine("Output Written to " + outputFile);

            // Finally close the dataset, releasing resources and file locks.
            dataSet.Dispose();
        }
        // Snippet End

        static void Main(string[] args)
        {
            string fileName = args.Length > 0 ? args[0] : "../../../../data/51Degrees-LiteV3.2.dat";
            string userAgents = args.Length > 1 ? args[1] : "../../../../data/20000 User Agents.csv";
            Run(fileName, userAgents);

            // Waits for a character to be pressed.
            Console.ReadKey();
        }
    }
}