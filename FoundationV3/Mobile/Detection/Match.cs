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
using System.Linq;
using System.Text;
using FiftyOne.Foundation.Mobile.Detection.Entities;
using System.Diagnostics;

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// Contains all the information associated with the device detection
    /// and matched result.
    /// </summary>
    /// <para>
    /// The match property can be used to request results from the match using the
    /// accessor provided with a <see cref="Property"/> or the string
    /// name of the property.
    /// </para>
    /// <para>
    /// The <see cref="Signature"/> the target device match against can be returned 
    /// along with the associated profiles.
    /// </para>
    /// <para>
    /// Statistics associated with the match can also be returned. For example; the 
    /// Elapsed property returns the time taken to perform the match. The Confidence
    /// property provides a value to indicate the differences between the match
    /// result and the target user agent.
    /// </para>
    /// <para>
    /// For more information see https://51degrees.com/Support/Documentation/Net
    /// </para>
    public class Match : IMatch
    {
        #region Classes

        /// <summary>
        /// A custom linked list used to identify the most frequently
        /// occurring signature indexes.
        /// </summary>
        internal class PossibleSignatures : IEnumerable<int>
        {
            #region Fields

            internal PossibleSignature First;

            internal PossibleSignature Last;

            internal int Count { get; private set; }

            #endregion

            #region Edit Methods

            internal void AddBefore(PossibleSignature existing, PossibleSignature newItem)
            {
                newItem.Next = existing;
                newItem.Previous = existing.Previous;
                if (existing.Previous != null)
                    existing.Previous.Next = newItem;
                existing.Previous = newItem;
                if (existing == First)
                    First = newItem;
                Count++;
            }

            internal void AddAfter(PossibleSignature existing, PossibleSignature newItem)
            {
                newItem.Next = existing.Next;
                newItem.Previous = existing;
                if (existing.Next != null)
                    existing.Next.Previous = newItem;
                existing.Next = newItem;
                if (existing == Last)
                    Last = newItem;
                Count++;
            }

            /// <summary>
            /// Adds the item to the end of the linked list.
            /// </summary>
            /// <param name="newItem"></param>
            internal void Add(PossibleSignature newItem)
            {
                if (Last != null)
                {
                    AddAfter(Last, newItem);
                }
                else
                {
                    First = newItem;
                    Last = newItem;
                    Count++;
                }
            }

            /// <summary>
            /// Removes any reference to this element from the linked list.
            /// </summary>
            internal void Remove(PossibleSignature existing)
            {
                if (First == existing)
                    First = existing.Next;
                if (Last == existing)
                    Last = existing.Previous;
                if (existing.Previous != null)
                    existing.Previous.Next = existing.Next;
                if (existing.Next != null)
                    existing.Next.Previous = existing.Previous;
                Count--;
            }

            #endregion

            #region Enumeration Methods

            public IEnumerator<int> GetEnumerator()
            {
                var current = First;
                while (current != null)
                {
                    // Return the signature associated with the ranked signature index.
                    yield return current.RankedSignatureIndex;
                    current = current.Next;
                }
            }
            
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            #endregion
        }

        /// <summary>
        /// Used to represent a signature index and the number of times
	    /// it occurs in the matched nodes.
        /// </summary>
        internal class PossibleSignature 
        {
            /// <summary>
            /// The ranked signature index. It's value is the index of the signature
            /// in the main list of signatures.
            /// </summary>
            internal readonly int RankedSignatureIndex;

            /// <summary>
            /// The number of times the signature index occurs.
            /// </summary>
		    internal int Frequency;

            /// <summary>
            /// The next signature index in the linked list.
            /// </summary>
            internal PossibleSignature Next;

            /// <summary>
            /// The previous signature index in the linked list.
            /// </summary>
            internal PossibleSignature Previous;
		
		    internal PossibleSignature(int signatureIndex, int frequency) 
            {
			    RankedSignatureIndex = signatureIndex;
			    Frequency = frequency;
		    }

            public override string ToString()
            {
                return String.Format("{0},{1}", RankedSignatureIndex, Frequency);
            }
        }

        #endregion

        #region Internal Fields

        /// <summary>
        /// Used to convert user-agents to arrays for matching.
        /// </summary>
        private static readonly Encoding ASCIIEncoder = Encoding.GetEncoding("us-ascii");

        /// <summary>
        /// List of signatures found for the match. Will reduce over time as
        /// the matching process progresses.
        /// </summary>
        private readonly List<Signature> _signatures = new List<Signature>();

        /// <summary>
        /// Used to time the detections.
        /// </summary>
        internal readonly Stopwatch Timer = new Stopwatch();

        /// <summary>
        /// List of node indexes found for the match.
        /// </summary>
        internal readonly List<Node> Nodes = new List<Node>();

        /// <summary>
        /// The data set used for the detection.
        /// </summary>
        public readonly DataSet DataSet;

        /// <summary>
        /// The next character position to be checked.
        /// </summary>
        internal int NextCharacterPositionIndex;

        /// <summary>
        /// The user agent string as an ASCII byte array.
        /// </summary>
        internal byte[] TargetUserAgentArray;

        #endregion

        #region External Fields

        /// <summary>
        /// The target user agent string used for the detection.
        /// </summary>
        public string TargetUserAgent
        {
            get { return _targetUserAgent; }
        }
        private string _targetUserAgent;
        
        /// <summary>
        /// The Http header that provided the user agent.
        /// </summary>
        public string HttpHeader
        {
            get { return _httpHeader; }
        }
        private string _httpHeader = "User-Agent";

        #endregion

        #region Public Properties

        /// <summary>
        /// The elapsed time for the match.
        /// </summary>
        public TimeSpan Elapsed
        {
            get { return new TimeSpan(_elapsed); }
            internal set { _elapsed = value.Ticks; }
        }
        internal long _elapsed;

        /// <summary>
        /// The signature with the closest match to the user sgent provided.
        /// </summary>
        public Signature Signature
        {
            get { return _signature; }
        }
        internal Signature _signature;

        /// <summary>
        /// Returns the results of the match as a sorted list of property
        /// names and values.
        /// </summary>
        [Obsolete("This method is not memory efficient and should be avoided as the Match " +
                  "class now exposes an accessor keyed on property name.")]
        public SortedList<string, string[]> Results
        {
            get
            {
                if (_results == null)
                {
                    lock (this)
                    {
                        if (_results == null)
                        {
                            var results = new SortedList<string, string[]>();
                            
                            // Add the properties and values first.
                            foreach (var profile in Profiles.Where(i => i != null))
                            {
                                foreach (var property in profile.Properties)
                                {
                                    results.Add(
                                        property.Name, 
                                        profile[property].ToStringArray());
                                }
                            }

                            // Add the statistics to make available.
                            results.Add(Constants.DetectionTimeProperty, 
                                new string[] { Elapsed.TotalMilliseconds.ToString("0.00000") });
                            results.Add(Constants.DifferenceProperty, 
                                new string[] { Difference.ToString() });
                            results.Add(Constants.Nodes,
                                new string[] { ToString() });

                            // Add any other derived values.
                            results.Add(Constants.DeviceId,
                                new string[] { DeviceId });

                            _results = results;
                        }
                    }
                }
                return _results;
            }
        }
        internal SortedList<string, string[]> _results;

        /// <summary>
        /// Gets the values associated with the property name using the profiles
        /// found by the match. If matched profiles don't contain a value then
        /// the default profiles for each of the components are also checked.
        /// </summary>
        /// <param name="property">The property whose values are required</param>
        /// <returns>Array of the values associated with the property, or null if the property does not exist</returns>
        public Values this[Property property]
        {
            get
            {
                Values value = null;
                
                if (property != null)
                {
                    // Get the property value from the profile returned
                    // from the match.
                    Profile profile;
                    if (ComponentProfiles.TryGetValue(property.Component.ComponentId, out profile) &&
                        profile != null)
                    {
                        value = profile[property];
                    }

                    // If the value has not been found use the default profile.
                    if (value == null)
                    {
                        value = property.Component.DefaultProfile[property];
                    }
                }
                return value;

            }
        }
        
        /// <summary>
        /// Gets the values associated with the property name using the profiles
        /// found by the match. If matched profiles don't contain a value then
        /// the default profiles for each of the components are also checked.
        /// </summary>
        /// <param name="propertyName">The property name whose values are required</param>
        /// <returns>Array of the values associated with the property name, or null if the property does not exist</returns>
        public Values this[string propertyName]
        {
            get
            {
                return this[DataSet.GetProperty(propertyName)];
            }
        }

        /// <summary>
        /// The numeric difference between the target user agent and the 
        /// match. Numeric sub strings of the same length are compared 
        /// based on the numeric value. Other character differences are 
        /// compared based on the difference in ASCII values of the two
        /// characters at the same positions.
        /// </summary>
        public int Difference
        {
            get 
            {
                return LowestScore >= 0 ? LowestScore : 0; 
            }
        }

        /// <summary>
        /// The method used to obtain the match. <see cref="MatchMethods"/>
        /// provides descriptions of the possible return values.
        /// </summary>
        public MatchMethods Method
        {
            get { return _method; }
        }
        internal MatchMethods _method;

        /// <summary>
        /// The number of closest signatures returned for evaluation.
        /// </summary>
        public int ClosestSignatures
        {
            get
            {
                return _closestSignatures;
            }
        }
        internal int _closestSignatures;

        /// <summary>
        /// The number of signatures that were compared against the target
        /// user agent if the Closest match method was used.
        /// </summary>
        public int SignaturesCompared
        {
            get { return _signaturesCompared; }
        }
        internal int _signaturesCompared;

        /// <summary>
        /// The number of signatures read during the detection.
        /// </summary>
        public int SignaturesRead
        {
            get { return _signaturesRead; }
        }
        internal int _signaturesRead;

        /// <summary>
        /// The number of root nodes checked against the target user agent.
        /// </summary>
        public int RootNodesEvaluated
        {
            get { return _rootNodesEvaluated; }
        }
        internal int _rootNodesEvaluated;

        /// <summary>
        /// The number of nodes checked.
        /// </summary>
        public int NodesEvaluated
        {
            get { return _nodesEvaluated; }
        }
        internal int _nodesEvaluated;

        /// <summary>
        /// The number of strings that were read from the data structure for 
        /// the match.
        /// </summary>
        public int StringsRead
        {
            get { return _stringsRead; }
        }
        internal int _stringsRead;

        /// <summary>
        /// Returns the profile Ids or device Id as a byte array.
        /// </summary>
        public byte[] DeviceIdAsByteArray
        {
            get
            {
                return ProfileIds.OrderBy(i =>
                        i.Key).SelectMany(i =>
                            BitConverter.GetBytes(i.Value)).ToArray();
            }
        }

        /// <summary>
        /// The unique id of the Device based on the profiles.
        /// </summary>
        public string DeviceId
        {
            get 
            {
                if (_signature != null)
                    return _signature.DeviceId;
                return String.Join(
                    Constants.ProfileSeperator,
                    Profiles.OrderBy(i => 
                        i.Component.ComponentId).Select(i => 
                            i.ProfileId.ToString()).ToArray());
            }
        }

        /// <summary>
        /// Dictionary keyed on the component id returning related
        /// profiles.
        /// </summary>
        internal IDictionary<int, Profile> ComponentProfiles
        {
            get
            {
                if (_componentProfiles == null)
                {
                    lock(this)
                    {
                        if (_componentProfiles == null)
                        {
                            if (Profiles == null)
                            {
                                int t = 1; ;
                            }
                            _componentProfiles = Profiles.ToDictionary(i =>
                                i.Component.ComponentId);
                        }
                    }
                }
                return _componentProfiles;
            }
        }
        internal IDictionary<int, Profile> _componentProfiles;

        /// <summary>
        /// Array of profiles associated with the device that was found.
        /// </summary>
        public Profile[] Profiles
        {
            get 
            {
                if (_profiles == null &&
                    _signature != null)
                {
                    lock (this)
                    {
                        if (_profiles == null)
                        {
                            var profiles = new Profile[_signature.Profiles.Length];
                            Array.Copy(_signature.Profiles, profiles, profiles.Length);
                            _profiles = profiles;
                        }
                    }
                }
                return _profiles;
            }
        }
        internal Profile[] _profiles;

        /// <summary>
        /// Array of profile ids associated with the device that was found, order by component id
        /// which forms the key field.
        /// </summary>
        public Dictionary<int, int> ProfileIds
        {
            get 
            {
                if (_profileIds == null)
                {
                    lock(this)
                    {
                        if (_profileIds == null)
                        {
                            _profileIds = Profiles.ToDictionary(
                                key => key.Component.ComponentId,
                                value => value.ProfileId); 
                        }
                    }
                }
                return _profileIds;
            }
        }
        private Dictionary<int, int> _profileIds;

        /// <summary>
        /// The user agent of the matching device with irrelevant 
        /// characters removed.
        /// </summary>
        public string UserAgent
        {
            get { return _signature != null ? _signature.ToString() : null; }
        }

        #endregion

        #region Internal Fields

        /// <summary>
        /// The current lowest score for the target user agent. Initialised
        /// to the largest possible result.
        /// </summary>
        internal int LowestScore;

        #endregion

        #region Constructor

        /// <summary>
        /// Contructs a new detection match ready to used to identify
        /// profiles from user agents.
        /// </summary>
        /// <param name="dataSet"></param>
        internal Match(DataSet dataSet)
        {
            DataSet = dataSet;
        }

        /// <summary>
        /// Constructs a new detection match ready to be used to identify
        /// the profiles associated with the target user agent.
        /// </summary>
        /// <param name="targetUserAgent"></param>
        /// <param name="dataSet"></param>
        internal Match(
            DataSet dataSet,
            string targetUserAgent)
            : this (dataSet)
        {
            Init(targetUserAgent);
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Sets the match properties to those provided in the state.
        /// </summary>
        /// <param name="state"></param>
        internal void SetState(MatchState state)
        {
            _elapsed = state._elapsed;
            _method = state._method;
            _nodesEvaluated = state._nodesEvaluated;
            _profiles = new Profile[state._profiles.Length];
            Array.Copy(state._profiles, _profiles, _profiles.Length);
            _rootNodesEvaluated = state._rootNodesEvaluated;
            _signature = state._signature;
            _signaturesCompared = state._signaturesCompared;
            _signaturesRead = state._signaturesRead;
            _stringsRead = state._stringsRead;
            _closestSignatures = state._closestSignatures;
            LowestScore = state._lowestScore;
            _targetUserAgent = state._targetUserAgent;
            TargetUserAgentArray = state._targetUserAgentArray;
            Nodes.Clear();
            Nodes.AddRange(state._nodes);
        }

        /// <summary>
        /// Returns the start character position of the node within the target
        /// user agent, or -1 if the node does not exist.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        internal int IndexOf(Node node)
        {
            var characters = node.Characters;
            var finalIndex = characters.Length - 1;
            for (int index = 0; index < TargetUserAgentArray.Length - characters.Length; index++)
            {
                for (int nodeIndex = 0, targetIndex = index; 
                    nodeIndex < characters.Length && targetIndex < TargetUserAgentArray.Length; 
                    nodeIndex++, targetIndex++)
                {
                    if (characters[nodeIndex] != TargetUserAgentArray[targetIndex])
                        break;
                    else if (nodeIndex == finalIndex)
                        return index;
                }
            }
            return -1;
        }

        /// <summary>
        /// Initialises the match object ready for detection.
        /// </summary>
        /// <param name="targetUserAgent"></param>
        private void Init(string targetUserAgent)
        {
            if (String.IsNullOrEmpty(targetUserAgent) == false)
            {
                TargetUserAgentArray = ASCIIEncoder.GetBytes(targetUserAgent);
            }
            else
            {
                TargetUserAgentArray = new byte[0];
            }
            _targetUserAgent = targetUserAgent;

            ResetNextCharacterPositionIndex();
        }

        /// <summary>
        /// Reset the next character position index based on the length
        /// of the target user agent and the root nodes.
        /// </summary>
        internal void ResetNextCharacterPositionIndex()
        {
            // Start checking on the far right of the user agent.
            NextCharacterPositionIndex = Math.Min(
                TargetUserAgentArray.Length - 1,
                DataSet.RootNodes.Count - 1);
        }

        /// <summary>
        /// Resets the match for the user agent returning all the fields
        /// to the values they would have when the match was first
        /// constructed. Used to avoid having to reallocate memory for 
        /// data structures when a lot of detections are being performed.
        /// </summary>
        /// <param name="targetUserAgent"></param>
        internal void Reset(string targetUserAgent)
        {
            _nodesEvaluated = 0;
            _rootNodesEvaluated = 0;
            _signaturesCompared = 0;
            _signaturesRead = 0;
            _stringsRead = 0;
            
            _signature = null;
            _signatures.Clear();
            Nodes.Clear();
            _profiles = null;
            _profileIds = null;
            _componentProfiles = null;

            Init(targetUserAgent);
        }

        /// <summary>
        /// Inserts the node into the list checking to find it's correct
        /// position in the list first.
        /// </summary>
        /// <param name="node">The node to be added to the match list</param>
        /// <returns>The index of the node inserted into the list</returns>
        internal int InsertNode(Node node)
        {
            var index = ~Nodes.BinarySearch(node);

            // Insert this node checking it's position relative to others
            // already populated in the list.
            Nodes.Insert(
                index,
                node);

            return index;
        }

        /// <summary>
        /// Override the profiles found by the match with the profileId provided.
        /// </summary>
        /// <param name="profileId">The ID of the profile to replace the existing component</param>
        internal void UpdateProfile(int profileId)
        {
            // Find the new profile from the data set.
            var newProfile = DataSet.FindProfile(profileId);
            if (newProfile != null)
            {
                // Loop through the profiles found so far and replace the
                // profile for the same component with the new one.
                for (int i = 0; i < Profiles.Length; i++)
                {
                    // Compare by component Id incase the stream data source is
                    // used and we have different instances of the same component
                    // being used.
                    if (Profiles[i].Component.ComponentId ==
                        newProfile.Component.ComponentId)
                    {
                        Profiles[i] = newProfile;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// If the nodes of the match correspond exactly to a signature then
        /// return the index of the signature found. Other wise -1.
        /// </summary>
        /// <returns></returns>
        internal int GetExactSignatureIndex()
        {
            var lower = 0;
            var upper = DataSet._signatures.Count - 1;

            while (lower <= upper)
            {
                _signaturesRead++;
                var middle = lower + (upper - lower) / 2;
                var comparisonResult = DataSet._signatures[middle].CompareTo(Nodes);
                if (comparisonResult == 0)
                    return middle;
                else if (comparisonResult > 0)
                    upper = middle - 1;
                else
                    lower = middle + 1;
            }

            return -1;
        }

        /// <summary>
        /// Returns an enumeration of the closest signatures which most closely 
        /// match the target user agent string. Where a single signature
        /// is not present across all the nodes the signatures which match
        /// the most nodes from the target user agent string are returned.
        /// </summary>
        /// <returns>An enumeration of the closest signatures</returns>
        internal IEnumerable<int> GetClosestSignatures()
        {
            if (Nodes.Count == 1)
            {
                // There is only 1 list so return an enumeration for that
                // single list.
                _closestSignatures = Nodes[0].RankedSignatureCount;
                return Nodes[0].RankedSignatureIndexes;
            }
            else
            {
                // There are multiple lists which will need to be combined to form
                // the result.
                var iteration = 2;
                var maxCount = 1;

                // Get the nodes in ascending order of signature index length.
                var iterator = Nodes.OrderBy(i => i.RankedSignatureCount).GetEnumerator();

                // Get the first node and add all the signature indexes.
                iterator.MoveNext();
                var linkedList = BuildInitialList(iterator.Current.RankedSignatureIndexes);

                // Count the number of times each signature index occurs.
                while (iterator.MoveNext())
                {
                    maxCount = GetClosestSignaturesForNode(
                        iterator.Current.RankedSignatureIndexes, linkedList, maxCount, iteration);
                    iteration++;
                }

                // Get the first item in the linked list.
                var current = linkedList.First;

                // First iteration to remove any items from the linked list that are
                // lower than the minimum count and set the signatures property
                // of each item that will remain.
                while (current != null)
                {
                    if (current.Frequency < maxCount)
                    {
                        // Remove the item as it's not needed.
                        linkedList.Remove(current);
                    }
                    current = current.Next;
                }
                _closestSignatures = linkedList.Count;
                return linkedList;
            }
        }

        private int GetClosestSignaturesForNode(
            int[] signatureIndexList,
            PossibleSignatures linkedList,
            int maxCount, int iteration)
        {
            // If there is point adding any new signature indexes set the
            // threshold reached indicator. New signatures won't be added
            // and ones with counts lower than maxcount will be removed.
            var thresholdReached = Nodes.Count - iteration < maxCount;
            var current = linkedList.First;
            int signatureIndex = 0;
            while (signatureIndex < signatureIndexList.Length &&
                current != null)
            {
                if (current.RankedSignatureIndex > signatureIndexList[signatureIndex])
                {
                    // The base list is higher than the target list. Add the element
                    // from the target list and move to the next element in each.
                    if (thresholdReached == false)
                        linkedList.AddBefore(
                            current,
                            new PossibleSignature(signatureIndexList[signatureIndex], 1));
                    signatureIndex++;
                }
                else if (current.RankedSignatureIndex <
                    signatureIndexList[signatureIndex])
                {
                    if (thresholdReached)
                    {
                        // Threshold reached so we can removed this item
                        // from the list as it's not relevant.
                        var nextItem = current.Next;
                        if (current.Frequency < maxCount)
                            linkedList.Remove(current);
                        current = nextItem;
                    }
                    else
                    {
                        current = current.Next;
                    }
                }
                else
                {
                    // They're the same so increase the frequency and move to the next
                    // element in each.
                    current.Frequency++;
                    if (current.Frequency > maxCount)
                        maxCount = current.Frequency;
                    signatureIndex++;
                    current = current.Next;
                }
            }
            if (thresholdReached == false)
            {
                // Add any signature indexes higher than the base list to the base list.
                while (signatureIndex < signatureIndexList.Length)
                {
                    linkedList.Add(new PossibleSignature(
                        signatureIndexList[signatureIndex], 1));
                    signatureIndex++;
                }
            }
            return maxCount;
        }

        /// <summary>
        /// Builds the initial list from the nodes signature indexes.
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private static PossibleSignatures BuildInitialList(IEnumerable<int> list)
        {
            var linkedList = new PossibleSignatures();
            foreach (var index in list)
            {
                linkedList.Add(new PossibleSignature(index, 1));
            }
            return linkedList;
        }

        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// A string representation of the nodes found from the 
        /// target user agent.
        /// </summary>
        public override string ToString()
        {
            if (Nodes != null && Nodes.Count > 0)
            {
                var value = new byte[TargetUserAgent.Length];
                foreach (var node in Nodes)
                    node.AddCharacters(value);
                for (int i = 0; i < value.Length; i++)
                    if (value[i] == 0)
                        value[i] = (byte)'_';
                return Encoding.ASCII.GetString(value);
            }
            return null;
        }

        #endregion
    }

    /// <summary>
    /// Used to persist the match results to the cache. Used with the 
    /// SetState method of the match class to retrieve the state.
    /// </summary>
    internal struct MatchState
    {
        internal readonly long _elapsed;
        internal readonly MatchMethods _method;
        internal readonly int _nodesEvaluated;
        internal readonly Profile[] _profiles;
        internal readonly int _rootNodesEvaluated;
        internal readonly Signature _signature;
        internal readonly int _signaturesCompared;
        internal readonly int _signaturesRead;
        internal readonly int _stringsRead;
        internal readonly int _closestSignatures;
        internal readonly int _lowestScore;
        internal readonly string _targetUserAgent;
        internal readonly byte[] _targetUserAgentArray;
        internal readonly Node[] _nodes;

        /// <summary>
        /// Creates the state based on the match provided.
        /// </summary>
        /// <param name="match"></param>
        internal MatchState(Match match)
        {
            _elapsed = match._elapsed;
            _method = match._method;
            _nodesEvaluated = match._nodesEvaluated;
            _profiles = new Profile[match.Profiles.Length];
            Array.Copy(match.Profiles, _profiles, _profiles.Length);
            _rootNodesEvaluated = match._rootNodesEvaluated;
            _signature = match._signature;
            _signaturesCompared = match._signaturesCompared;
            _signaturesRead = match._signaturesRead;
            _stringsRead = match._stringsRead;
            _closestSignatures = match._closestSignatures;
            _lowestScore = match.LowestScore;
            _targetUserAgent = match.TargetUserAgent;
            _targetUserAgentArray = match.TargetUserAgentArray;
            _nodes = match.Nodes.ToArray();
        }
    }
}