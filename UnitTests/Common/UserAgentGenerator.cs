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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FiftyOne.UnitTests
{
    /// <summary>
    /// Used to return user agents for various test cases.
    /// </summary>
    internal static class UserAgentGenerator
    {
        /// <summary>
        /// Array of user agents stored in memory from the source data
        /// file.
        /// </summary>
        private static readonly string[] _userAgents;

        /// <summary>
        /// Used to generate random user agents.
        /// </summary>
        private static Random _random = new Random();

        /// <summary>
        /// Initialises the user agents used by the generator.
        /// </summary>
        static UserAgentGenerator()
        {
            _userAgents = File.ReadAllLines(Constants.GOOD_USERAGENTS_FILE);
        }

        /// <summary>
        /// Returns a random user agent which may also have been randomised.
        /// </summary>
        /// <param name="randomness"></param>
        /// <returns></returns>
        internal static string GetRandomUserAgent(int randomness)
        {
            var value = _userAgents[_random.Next(_userAgents.Length)];
            if (randomness > 0)
            {
                var bytes = ASCIIEncoding.ASCII.GetBytes(value);
                for (int i = 0; i < randomness; i++ )
                {
                    var indexA = _random.Next(value.Length);
                    var indexB = _random.Next(value.Length);
                    byte temp = bytes[indexA];
                    bytes[indexA] = bytes[indexB];
                    bytes[indexB] = temp;
                }
                value = ASCIIEncoding.ASCII.GetString(bytes);
            }
            return value;
        }

        /// <summary>
        /// Returns the number of random user agents requested in the count paramemer.
        /// The user agents are also altered with the randomness provided.
        /// </summary>
        /// <param name="count">Number of user agents to return</param>
        /// <param name="randomness">Number of characters to alter in the user agents</param>
        /// <returns>Enumerable of user agents</returns>
        internal static IEnumerable<string> GetEnumerable(int count, int randomness)
        {
            for(int i = 0; i < count; i++)
            {
                yield return GetRandomUserAgent(randomness);
            }
        }

        /// <summary>
        /// Returns an enumerable of user agent strings which match the regex. The
        /// results can not return more than the count specified.
        /// </summary>
        /// <param name="count">Nmber of user agents to return.</param>
        /// <param name="pattern">Regular expression for the user agents.</param>
        /// <returns>An enumerable of user agents</returns>
        internal static IEnumerable<string> GetEnumerable(int count, string pattern)
        {
            var counter = 0;
            var regex = new Regex(pattern, RegexOptions.Compiled);
            while (counter < count)
            {
                var iterator = _userAgents.Select(i => i).GetEnumerator();
                while (counter < count && iterator.MoveNext())
                {
                    if (regex.IsMatch(iterator.Current))
                    {
                        yield return iterator.Current;
                        counter++;
                    }
                }
            }
        }

        /// <summary>
        /// A selection of random user agents which have not been randomly 
        /// altered.
        /// </summary>
        /// <returns>An enumerable of user agents</returns>
        internal static IEnumerable<string> GetRandomUserAgents()
        {
            return UserAgentGenerator.GetEnumerable(_userAgents.Length, 0);
        }

        /// <summary>
        /// A selection of unique user agents in a defined order.
        /// </summary>
        /// <returns>An enumerable of user agents</returns>
        internal static IEnumerable<string> GetUniqueUserAgents()
        {
            return _userAgents;
        }

        /// <summary>
        /// A selection of randomly invalid user agents. 
        /// </summary>
        /// <returns>An enumerable of user agents</returns>
        internal static IEnumerable<string> GetBadUserAgents()
        {
            return UserAgentGenerator.GetEnumerable(_userAgents.Length, 10);
        }
    }
}