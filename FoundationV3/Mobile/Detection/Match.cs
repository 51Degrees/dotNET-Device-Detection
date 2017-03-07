/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2015 51Degrees Mobile Experts Limited, 5 Charlotte Close,
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
using FiftyOne.Foundation.Mobile.Detection.Caching;

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// Contains all the information associated with the device detection
    /// and matched result.
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
    /// result and the target User-Agent.
    /// </para>
    /// <para>
    /// For more information see https://51degrees.com/Support/Documentation/Net
    /// </para>
    /// </summary>
    public class Match : IMatch
    {
        #region Private Fields

        /// <summary>
        /// The result of a completed match.
        /// </summary>
        internal MatchResult Result { private get; set; }

        /// <summary>
        /// State of an active device detection process.
        /// </summary>
        internal readonly MatchState State;

        /// <summary>
        /// Instance of the provider used to create the match.
        /// </summary>
        internal readonly Provider Provider;

        #endregion

        #region Public Fields

        /// <summary>
        /// The data set used for the detection.
        /// </summary>
        public DataSet DataSet { get { return Provider.DataSet; } }

        #endregion

        #region Public Properties

        /// <summary>
        /// The target User-Agent string used for the detection where a single
        /// User-Agent was provided. If mutli HTTP headers were provided then
        /// this value will be null.
        /// </summary>
        public string TargetUserAgent
        {
            get { return Result.TargetUserAgent; }
        }

        /// <summary>
        /// The elapsed time for the match.
        /// </summary>
        public TimeSpan Elapsed
        {
            get { return new TimeSpan(Result.Elapsed); }
        }

        /// <summary>
        /// The signature with the closest match to the User-Agent provided.
        /// </summary>
        public Signature Signature
        {
            get { return Result.Signature; }
        }

        /// <summary>
        /// The method used to obtain the match. <see cref="MatchMethods"/>
        /// provides descriptions of the possible return values. When used
        /// with multi HTTP headers the worst method used for all the HTTP
        /// headers.
        /// </summary>
        public MatchMethods Method
        {
            get { return Result.Method; }
        }

        /// <summary>
        /// The number of closest signatures returned for evaluation.
        /// </summary>
        public int ClosestSignatures
        {
            get { return Result.ClosestSignatures; }
        }

        /// <summary>
        /// The number of signatures that were compared against the target
        /// User-Agent if the Closest match method was used.
        /// </summary>
        public int SignaturesCompared
        {
            get { return Result.SignaturesCompared; }
        }

        /// <summary>
        /// The number of signatures read during the detection.
        /// </summary>
        public int SignaturesRead
        {
            get { return Result.SignaturesRead; }
        }

        /// <summary>
        /// The number of root nodes checked against the target User-Agent.
        /// </summary>
        public int RootNodesEvaluated
        {
            get { return Result.RootNodesEvaluated; }
        }

        /// <summary>
        /// The number of nodes checked.
        /// </summary>
        public int NodesEvaluated
        {
            get { return Result.NodesEvaluated; }
        }

        /// <summary>
        /// The number of nodes found by the match.
        /// </summary>
        public int NodesFound
        {
            get { return Result.Nodes.Count; }
        }

        /// <summary>
        /// The number of strings that were read from the data structure for 
        /// the match.
        /// </summary>
        public int StringsRead
        {
            get { return Result.StringsRead; }
        }

        /// <summary>
        /// Array of profiles associated with the device that was found.
        /// </summary>
        public Profile[] Profiles
        {
            get { return _overriddenProfiles == null ? Result.Profiles : _overriddenProfiles; }
        }

        /// <summary>
        /// Array of profiles associated with the device that may have been
        /// overridden for this instance of match.
        /// </summary>
        /// <remarks>
        /// This property is needed to ensure that another references to the
        /// instance of MatchResult are not altered when overriding profiles.
        /// </remarks>
        internal Profile[] OverriddenProfiles
        {
            get
            {
                if (_overriddenProfiles == null)
                {
                    lock (this)
                    {
                        if (_overriddenProfiles == null)
                        {
                            var overriddenProfiles = new Profile[Result.Profiles.Length];
                            Result.Profiles.CopyTo(overriddenProfiles, 0);
                            _overriddenProfiles = overriddenProfiles;
                        }
                    }
                }
                return _overriddenProfiles;
            }
        }
        private Profile[] _overriddenProfiles;

        /// <summary>
        /// Dictionary of profile ids associated with the device that was found, order by component id
        /// which forms the key field.
        /// </summary>
        public Dictionary<int, int> ProfileIds
        {
            get
            {
                return Profiles.ToDictionary(
                    key => key.Component.ComponentId,
                    value => value.ProfileId);
            }
        }

        /// <summary>
        /// The numeric difference between the target User-Agent and the 
        /// match. Numeric sub strings of the same length are compared 
        /// based on the numeric value. Other character differences are 
        /// compared based on the difference in ASCII values of the two
        /// characters at the same positions.
        /// </summary>
        public int Difference
        {
            get
            {
                return Result.LowestScore >= 0 ? Result.LowestScore : 0;
            }
        }

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
                if (Signature != null)
                    return Signature.DeviceId;
                return String.Join(
                    Constants.ProfileSeperator,
                    Profiles.OrderBy(i =>
                        i.Component.ComponentId).Select(i =>
                            i.ProfileId.ToString()).ToArray());
            }
        }

        /// <summary>
        /// The User-Agent of the matching device with irrelevant 
        /// characters removed.
        /// </summary>
        public string UserAgent
        {
            get { return Signature != null ? Signature.ToString() : null; }
        }

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

                return results;
            }
        }

        #endregion

        #region Public Accessors

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
                    Profile profile = Profiles.FirstOrDefault(i =>
                        property.Component.Equals(i.Component));
                    if (profile != null)
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
                return this[DataSet.Properties[propertyName]];
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Contructs a new detection match ready to used to identify
        /// profiles from User-Agents.
        /// </summary>
        /// <param name="provider"></param>
        internal Match(Provider provider)
        {
            Provider = provider;
            State = new MatchState(this);
            Result = State;
        }

        /// <summary>
        /// Constructs a new detection match ready to be used to identify
        /// the profiles associated with the target User-Agent.
        /// </summary>
        /// <param name="targetUserAgent"></param>
        /// <param name="provider"></param>
        internal Match(
            Provider provider,
            string targetUserAgent)
            : this(provider)
        {
            State.Init(targetUserAgent);
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Resets the match instance ready for further matching.
        /// </summary>
        internal void Reset()
        {
            State.Reset();
            _overriddenProfiles = null;
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
                for (int i = 0; i < OverriddenProfiles.Length; i++)
                {
                    // Compare by component Id incase the stream data source is
                    // used and we have different instances of the same component
                    // being used.
                    if (OverriddenProfiles[i].Component.ComponentId ==
                        newProfile.Component.ComponentId)
                    {
                        OverriddenProfiles[i] = newProfile;
                        break;
                    }
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// A string representation of the nodes found from the 
        /// target User-Agent.
        /// </summary>
        public override string ToString()
        {
            if (Result.Nodes != null && Result.Nodes.Count > 0)
            {
                var value = new byte[TargetUserAgent.Length];
                foreach (var node in Result.Nodes)
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
    /// Working state used during the device detection matching process.
    /// </summary>
    internal class MatchState : MatchResult, IValueLoader<string, MatchResult>
    {
        #region Constants

        /// <summary>
        /// Used to convert user-agents to arrays for matching.
        /// </summary>
        private static readonly Encoding ASCIIEncoder = Encoding.GetEncoding("us-ascii");

        #endregion

        #region MatchResult Setters

        internal new long Elapsed { get { return base._elapsed; } set { base._elapsed = value; } }

        internal new MatchMethods Method { get { return base._method; } set { base._method = value; } }

        internal new int NodesEvaluated { get { return base._nodesEvaluated; } set { base._nodesEvaluated = value; } }

        internal new int RootNodesEvaluated { get { return base._rootNodesEvaluated; } set { base._rootNodesEvaluated = value; } }

        internal new Signature Signature { get { return base._signature; } set { base._signature = value; } }

        internal new int SignaturesCompared { get { return base._signaturesCompared; } set { base._signaturesCompared = value; } }

        internal new int SignaturesRead { get { return base._signaturesRead; } set { base._signaturesRead = value; } }

        internal new int StringsRead { get { return base._stringsRead; } set { base._stringsRead = value; } }

        internal new int ClosestSignatures { get { return base._closestSignatures; } set { base._closestSignatures = value; } }

        internal new int LowestScore { get { return base._lowestScore; } set { base._lowestScore = value; } }

        internal new string TargetUserAgent { get { return base._targetUserAgent; } set { base._targetUserAgent = value; } }

        internal new byte[] TargetUserAgentArray { get { return base._targetUserAgentArray; } set { base._targetUserAgentArray = value; } }

        internal List<Profile> ExplicitProfiles
        {
            get
            {
                if (_explicitProfiles == null)
                {
                    lock (this)
                    {
                        if (_explicitProfiles == null)
                        {
                            _explicitProfiles = base.Signature != null ?
                                base.Signature.Profiles.ToList() :
                                new List<Profile>();
                        }
                    }
                }
                return _explicitProfiles;
            }
        }
        private List<Profile> _explicitProfiles;
        internal override Profile[] Profiles { get { return ExplicitProfiles.ToArray(); } }
                
        internal override IList<Node> Nodes 
        {
            get { return _nodesList; } 
        }
        private readonly List<Node> _nodesList = new List<Node>();

        #endregion

        #region Working Fields

        internal int NextCharacterPositionIndex;

        internal readonly List<Signature> Signatures = new List<Signature>();

        internal readonly Match Match;

        internal DataSet DataSet { get { return Match.DataSet; } }

        #endregion

        #region Constructor

        internal MatchState(Match match) : base()
        {
            Match = match;
        }

        #endregion

        #region Internal Methods

        internal void Reset()
        {
            NodesEvaluated = 0;
            RootNodesEvaluated = 0;
            SignaturesCompared = 0;
            SignaturesRead = 0;
            StringsRead = 0;
            Signature = null;
            Signatures.Clear();
            Nodes.Clear();
            _explicitProfiles = null;
        }

        /// <summary>
        /// Resets the match for the User-Agent returning all the fields
        /// to the values they would have when the match was first
        /// constructed. Used to avoid having to reallocate memory for 
        /// data structures when a lot of detections are being performed.
        /// </summary>
        /// <param name="targetUserAgent"></param>
        internal void Reset(string targetUserAgent)
        {
            Reset();
            Init(targetUserAgent);
        }

        /// <summary>
        /// Initialises the match object ready for detection.
        /// </summary>
        /// <param name="targetUserAgent"></param>
        internal void Init(string targetUserAgent)
        {
            if (String.IsNullOrEmpty(targetUserAgent) == false)
            {
                TargetUserAgentArray = ASCIIEncoder.GetBytes(targetUserAgent);
            }
            else
            {
                TargetUserAgentArray = new byte[0];
            }
            TargetUserAgent = targetUserAgent;
            ResetNextCharacterPositionIndex();
        }

        /// <summary>
        /// Reset the next character position index based on the length
        /// of the target User-Agent and the root nodes so that checking
        /// starts on the far right of the User-Agent.
        /// </summary>
        internal void ResetNextCharacterPositionIndex()
        {
            NextCharacterPositionIndex = Math.Min(
                base.TargetUserAgentArray.Length - 1,
                Match.DataSet.RootNodes.Count - 1);
        }

        /// <summary>
        /// Inserts the node into the list checking to find it's correct
        /// position in the list first.
        /// </summary>
        /// <param name="node">The node to be added to the match list</param>
        /// <returns>The index of the node inserted into the list</returns>
        internal int InsertNode(Node node)
        {
            var index = ~((List<Node>)Nodes).BinarySearch(node);
            Nodes.Insert(
                index,
                node);
            return index;
        }

        /// <summary>
        /// Returns the start character position of the node within the target
        /// User-Agent, or -1 if the node does not exist.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        internal int IndexOf(Node node)
        {
            var characters = node.Characters;
            var finalIndex = characters.Length - 1;
            for (int index = 0; index < base.TargetUserAgentArray.Length - characters.Length; index++)
            {
                for (int nodeIndex = 0, targetIndex = index;
                    nodeIndex < characters.Length && targetIndex < base.TargetUserAgentArray.Length;
                    nodeIndex++, targetIndex++)
                {
                    if (characters[nodeIndex] != base.TargetUserAgentArray[targetIndex])
                    {
                        break;
                    }
                    else if (nodeIndex == finalIndex)
                    {
                        return index;
                    }
                }
            }
            return -1;
        }

        MatchResult IValueLoader<string, MatchResult>.Load(string key)
        {
            Match.Provider.MatchNoCache(key, this);
            return new MatchResult(this);
        }

        #endregion
    }

    /// <summary>
    /// Used to persist the results of a match for future use.
    /// </summary>
    internal class MatchResult
    {
        #region Properties

        internal virtual long Elapsed
        {
            get { return _elapsed; }
        }
        protected long _elapsed;

        internal virtual MatchMethods Method
        {
            get { return _method; }
        }
        protected MatchMethods _method;

        internal virtual int NodesEvaluated
        {
            get { return _nodesEvaluated; }
        }
        protected int _nodesEvaluated;

        internal virtual int RootNodesEvaluated
        {
            get { return _rootNodesEvaluated; }
        }
        protected int _rootNodesEvaluated;

        internal virtual Signature Signature
        {
            get { return _signature; }
        }
        protected Signature _signature;

        internal virtual int SignaturesCompared
        {
            get { return _signaturesCompared; }
        }
        protected int _signaturesCompared;

        internal virtual int SignaturesRead
        {
            get { return _signaturesRead; }
        }
        protected int _signaturesRead;

        internal virtual int StringsRead
        {
            get { return _stringsRead; }
        }
        protected int _stringsRead;

        internal virtual int ClosestSignatures
        {
            get { return _closestSignatures; }
        }
        protected int _closestSignatures;

        internal virtual int LowestScore
        {
            get { return _lowestScore; }
        }
        protected int _lowestScore;

        internal virtual string TargetUserAgent
        {
            get { return _targetUserAgent; }
        }
        protected string _targetUserAgent;

        internal virtual byte[] TargetUserAgentArray
        {
            get { return _targetUserAgentArray; }
        }
        protected byte[] _targetUserAgentArray;

        internal virtual IList<Node> Nodes
        {
            get { return _nodes; }
        }
        protected IList<Node> _nodes;

        internal virtual Profile[] Profiles
        {
            get { return _profiles; }
        }
        protected Profile[] _profiles;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a default instance of <see cref="MatchResult"/>.
        /// </summary>
        protected MatchResult() { }

        /// <summary>
        /// Constructs an instance of <see cref="MatchResult"/> based on the 
        /// source provided.
        /// </summary>
        /// <param name="source"></param>
        internal MatchResult(MatchState source)
        {
            _elapsed = source.Elapsed;
            _method = source.Method;
            _nodesEvaluated = source.NodesEvaluated;
            _rootNodesEvaluated = source.RootNodesEvaluated;
            _signature = source.Signature;
            _signaturesCompared = source.SignaturesCompared;
            _signaturesRead = source.SignaturesRead;
            _stringsRead = source.StringsRead;
            _closestSignatures = source.ClosestSignatures;
            _lowestScore = source.LowestScore;
            _targetUserAgent = source.TargetUserAgent;
            _targetUserAgentArray = source.TargetUserAgentArray;
            _profiles = (Profile[])source.Profiles.Clone();
            _nodes = source.Nodes.ToArray();
        }

        #endregion
    }
}