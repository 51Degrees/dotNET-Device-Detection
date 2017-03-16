/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2017 51Degrees Mobile Experts Limited, 5 Charlotte Close,
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

namespace FiftyOne.Tests.Integration
{
    /// <summary>
    /// Used to return User-Agents for various test cases.
    /// </summary>
    public static class UserAgentGenerator
    {
        /// <summary>
        /// Array of User-Agents stored in memory from the source data
        /// file.
        /// </summary>
        private static readonly string[] _userAgents;

        /// <summary>
        /// Used to generate random User-Agents.
        /// </summary>
        private static Random _random = new Random();

        /// <summary>
        /// Initialises the User-Agents used by the generator.
        /// </summary>
        static UserAgentGenerator()
        {
            _userAgents = File.ReadAllLines(Utils.GetDataFile(Constants.GOOD_USERAGENTS_FILE));
        }

        /// <summary>
        /// Returns a random User-Agent which may also have been randomised.
        /// </summary>
        /// <param name="randomness"></param>
        /// <returns></returns>
        public static string GetRandomUserAgent(int randomness)
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
        /// Returns the number of random User-Agents requested in the count paramemer.
        /// The User-Agents are also altered with the randomness provided.
        /// </summary>
        /// <param name="count">Number of User-Agents to return</param>
        /// <param name="randomness">Number of characters to alter in the User-Agents</param>
        /// <returns>Enumerable of User-Agents</returns>
        public static IEnumerable<string> GetEnumerable(int count, int randomness)
        {
            for(int i = 0; i < count; i++)
            {
                yield return GetRandomUserAgent(randomness);
            }
        }

        /// <summary>
        /// Returns an enumerable of User-Agent strings which match the regex. The
        /// results can not return more than the count specified.
        /// </summary>
        /// <param name="count">Nmber of User-Agents to return.</param>
        /// <param name="pattern">Regular expression for the User-Agents.</param>
        /// <returns>An enumerable of User-Agents</returns>
        public static IEnumerable<string> GetEnumerable(int count, string pattern)
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
        /// A selection of random User-Agents which have not been randomly 
        /// altered.
        /// </summary>
        /// <returns>An enumerable of User-Agents</returns>
        public static IEnumerable<string> GetRandomUserAgents()
        {
            return UserAgentGenerator.GetEnumerable(_userAgents.Length, 0);
        }

        /// <summary>
        /// A selection of unique User-Agents in a defined order.
        /// </summary>
        /// <returns>An enumerable of User-Agents</returns>
        public static IEnumerable<string> GetUniqueUserAgents()
        {
            return _userAgents;
        }

        /// <summary>
        /// A selected of unique user agenets in a defined order
        /// repeated a set number of times.
        /// </summary>
        /// <param name="count">Number of User-Agents to return</param>
        /// <param name="randomness">Number of characters to alter in the User-Agents</param>
        /// <param name="repeat">Number of times to repeat the input</param>
        /// <returns></returns>
        public static IEnumerable<string> GetRepeatingUserAgents(int count, int randomness, int repeat)
        {
            var userAgents = GetEnumerable(count, randomness).ToArray();
            for (int i = 0; i < repeat; i++)
            {
                foreach (var userAgent in userAgents)
                {
                    yield return userAgent;
                }
            }
        }

        /// <summary>
        /// A selection of randomly invalid User-Agents. 
        /// </summary>
        /// <returns>An enumerable of User-Agents</returns>
        public static IEnumerable<string> GetBadUserAgents()
        {
            return UserAgentGenerator.GetEnumerable(_userAgents.Length, 10);
        }
    }
}