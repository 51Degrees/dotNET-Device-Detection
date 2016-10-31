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
using System.Text;
using System.Linq;
using FiftyOne.Foundation.Mobile.Detection.Entities;
using System.Diagnostics;

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// A single static class which controls the device detection process.
    /// </summary>
    /// <para>
    /// The process uses 3 steps to determine the properties associated with the
    /// provided User-Agent.
    /// </para>
    /// <para>
    /// Step 1 - each of the character positions of the target User-Agent are checked
    /// from right to left to determine if a complete node or sub string is present
    /// at that position. For example; the sub string Chrome/11 might indicate the 
    /// User-Agent relates to Chrome version 11 from Google. Once every character
    /// position is checked a list of matching nodes will be available.
    /// </para>
    /// <para>
    /// Step 2 - The list of signatures is then searched to determine if the matching 
    /// nodes relate exactly to an existing signature. Any popular device will be found
    /// at this point. The approach is exceptionally fast at identifying popular
    /// devices. This is termed the Exact match method.
    /// </para>
    /// <para>
    /// Step 3 - If a match has not been found exactly then the target User-Agent is
    /// evaluated to find nodes that have the smallest numeric difference. For example
    /// if Chrome/40 were in the target User-Agent at the same position as Chrome/32 
    /// in the signature then Chrome/32 with a numeric difference score of 8 would be
    /// used. If a signature can then be matched exactly against the new set of nodes
    /// this will be returned. This is termed the Numeric match method.
    /// </para>
    /// <para>
    /// Step 4 - If the target User-Agent is less popular, or newer than the creation time
    /// of the data set, a small sub set of possible signatures are identified from the
    /// matchied nodes. The sub set is limited to the most popular 200 signatures.
    /// </para>
    /// <para>
    /// Step 5 - The sub strings of the signatures from Step 4 are then evaluated to determine
    /// if they exist in the target User-Agent. For example if Chrome/32 in the target 
    /// appears one character to the left of Chrome/32 in the signature then a difference
    /// of 1 would be returned between the signature and the target. This is termed the
    /// Nearest match method.
    /// </para>
    /// <para>
    /// Step 6 - The signatures from Step 4 are evaluated against the target User-Agent to 
    /// determine the difference in relevent characters between them. The signature with the 
    /// lowest difference in ASCII character values with the target is returned. This is termed
    /// the Closest match method.
    /// </para>
    /// <remarks>
    /// Random User-Agents will not identify any matching nodes. In these situations a 
    /// default set of profiles are returned.
    /// </remarks>
    /// <remarks>
    /// The characteristics of the detection data set will determine the accuracy of the
    /// result match. Older data sets that are unaware of the latest devices, or User-Agent
    /// formats in use will be less accurate.
    /// </remarks>
    /// <para>
    /// For more information see https://51degrees.com/Support/Documentation/Net
    /// </para>
    internal static class Controller
    {
        #region Classes used by Nearest and Closest methods

        /// <summary>
        /// Used to filter multiple lists of ordered ranked signature indexes
        /// so that those that appear the most times are set in the top
        /// indexes list.
        /// </summary>
        internal class MostFrequentFilter : List<int>
        {
            /// <summary>
            /// Fronts an array of integers. Used to identify duplicate items 
            /// in the lists that are being filtered.
            /// </summary>
            private class OrderedList
            {
                private static readonly SearchLists<int, int> _search = new
                    SearchLists<int, int>();

                internal readonly IList<int> Items;
                private int _nextStartIndex = 0;
                private int _currentIndex = -1;

                /// <summary>
                /// Constructs a new instance of the OrderedList.
                /// </summary>
                /// <param name="items">
                /// Array of integers to incldue in the list
                /// </param>
                internal OrderedList(IList<int> items)
                {
                    Items = items;
                }
                
                /// <summary>
                /// Determines if the ordered list contains the value requested. 
                /// Updates the start index as we know the next request to Contains
                /// will always be for a value larger than the one just passed.
                /// </summary>
                /// <param name="value">Value to be checked in the list</param>
                /// <returns>
                /// True if the list contains the value, otherwise false
                /// </returns>
                internal bool Contains(int value)
                {
                    var index = _search.BinarySearch(Items, value);
                    if (index < 0)
                    {
                        _nextStartIndex = ~index;
                    }
                    else
                    {
                        _nextStartIndex = index + 1;
                    }
                    return index >= 0;
                }

                internal int Current
                {
                    get { return Items[_currentIndex]; }
                }

                internal bool MoveNext()
                {
                    _currentIndex++;
                    return _currentIndex < Items.Count;
                }

                internal void Reset()
                {
                    _currentIndex = -1;
                    _nextStartIndex = 0;
                }
            }

            /// <summary>
            /// Constructs a new instance of <see cref="MostFrequentFilter"/>
            /// </summary>
            /// <remarks>
            /// The nodes are always ordered based on the ascending lowest value
            /// in each list that is current.
            /// </remarks>
            /// <param name="state">Current state of the match process</param>
            internal MostFrequentFilter(MatchState state)
            {
                Init(state.Nodes.OrderBy(i => 
                    i.RankedSignatureIndexes.Count).Select(i =>
                        new OrderedList(i.RankedSignatureIndexes)).ToArray(),
                    state.DataSet.MaxSignatures);
            }

            /// <summary>
            /// Constructs a new instance of <see cref="MostFrequentFilter"/> for unit testing.
            /// </summary>
            /// <param name="lists">Lists of arrays to be used as input</param>
            /// <param name="maxResults">The maxmimum number of results to return</param>
            internal MostFrequentFilter(int[][] lists, int maxResults)
            {
                Init(lists.OrderBy(i => i.Length).Select(i =>
                    new OrderedList(i)).ToArray(),
                    maxResults);
            }

            /// <summary>
            /// Keep adding integers to the list until there are insufficient
            /// lists remaining to make a difference or we've reached the
            /// maxmium number of results to return.
            /// </summary>
            private void Init(OrderedList[] lists, int maxResults)
            {
                int topCount = 0;
                if (lists.Length == 1)
                {
                    AddRange(lists[0].Items.Take(maxResults));
                }
                else if (lists.Length > 1)
                {
                    for (var listIndex = 0;
                        listIndex < lists.Length &&
                        lists.Length - listIndex >= topCount;
                        listIndex++)
                    {
                        foreach (var list in lists)
                        {
                            list.Reset();
                        }
                        while (lists[listIndex].MoveNext())
                        {
                            if (GetHasProcessed(lists, listIndex) == false)
                            {
                                var count = GetCount(lists, listIndex, topCount);
                                if (count > topCount)
                                {
                                    topCount = count;
                                    Clear();
                                }
                                if (count == topCount)
                                {
                                    Add(lists[listIndex].Current);
                                }
                            }
                        }
                    }
                }
                Sort();
                if (Count > maxResults)
                {
                    RemoveRange(maxResults, Count - maxResults);
                }
            }

            /// <summary>
            /// If the value of the target node has already been processed because
            /// it's contained in a previous list then return true. If not and
            /// it still needs to be checked return false.
            /// </summary>
            /// <param name="lists">Lists being filtered</param>
            /// <param name="index">
            /// Index of the list whose current value should be checked in 
            /// prior lists.
            /// </param>
            /// <returns>True if the value has been processed, otherwise false</returns>
            private bool GetHasProcessed(OrderedList[] lists, int index)
            {
                for (var i = index - 1; i >= 0; i--)
                {
                    if (lists[i].Contains(lists[index].Current))
                    {
                        return true;
                    }
                }
                return false;
            }

            /// <summary>
            /// Returns the number of lists the target value is contained in.
            /// </summary>
            /// <param name="lists">Lists being filtered</param>
            /// <param name="index">
            /// Index of the list whose current value should be counted
            /// </param>
            /// <param name="topCount">Highest count so far</param>
            /// <returns>
            /// Number of lists that contain the value held by the list at the index
            /// </returns>
            private int GetCount(OrderedList[] lists, int index, int topCount)
            {
                var count = 1;
                for (var i = index + 1; 
                     i < lists.Length &&
                     lists.Length - index + count > topCount; 
                     i++)
                {
                    if (lists[i].Contains(lists[index].Current))
                    {
                        count++;
                    }
                }
                return count;
            }
        }

        private abstract class BaseScore
        {
            /// <summary>
            /// Gets the score for the specific node of the signature.
            /// </summary>
            /// <param name="state"></param>
            /// <param name="node"></param>
            /// <returns></returns>
            protected abstract int GetScore(MatchState state, Node node);

            /// <summary>
            /// Sets any initial score before each node is evaluated.
            /// </summary>
            /// <param name="signature"></param>
            /// <param name="lastNodeCharacter"></param>
            /// <returns></returns>
            protected abstract int GetInitialScore(Signature signature, int lastNodeCharacter);

            /// <summary>
            /// Checks all the signatures using the scoring method provided.
            /// </summary>
            /// <param name="state">Current state of the match process</param>
            /// <param name="rankedSignatureIndexes">List of ranked signature indexes</param>
            internal void EvaluateSignatures(MatchState state, IList<int> rankedSignatureIndexes)
            {
                state.LowestScore = int.MaxValue;
                var lastNodeCharacter = state.Nodes.Last().Root.Position;
                foreach (var rankedSignatureIndex in rankedSignatureIndexes.Take(state.DataSet.MaxSignatures))
                {
                    var signatureIndex = state.DataSet.RankedSignatureIndexes[rankedSignatureIndex];
                    var signature = state.DataSet.Signatures[signatureIndex];
                    EvaluateSignature(state, signature, lastNodeCharacter);
                }
            }

            /// <summary>
            /// Compares all the characters up to the max length between the 
            /// signature and the target User-Agent updating the match
            /// information if this signature is better than any evaluated
            /// previously.
            /// </summary>
            /// <param name="state">Information about the detection</param>
            /// <param name="lastNodeCharacter">
            /// The position of the last character in the matched nodes
            /// </param>
            /// <param name="signature">
            /// The siganture to be evaluated.
            /// </param>
            private void EvaluateSignature(
                MatchState state,
                Signature signature,
                int lastNodeCharacter)
            {
                state.SignaturesCompared++;

                // Get the score between the target and the signature stopping if it's
                // going to be larger than the lowest score already found.
                int score = GetScore(state, signature, lastNodeCharacter);

                // If the score is lower than the current lowest then use this signature.
                if (score < state.LowestScore)
                {
                    state.LowestScore = score;
                    state.Signature = signature;
                }
            }

            /// <summary>
            /// Steps through the nodes of the signature comparing those that aren't
            /// contained in the matched nodes to determine a score between the signature
            /// and the target User-Agent. If that score becomes greater or equal to the
            /// lowest score determined so far then stop.
            /// </summary>
            /// <param name="state"></param>
            /// <param name="signature"></param>
            /// <param name="lastNodeCharacter"></param>
            /// <returns></returns>
            private int GetScore(MatchState state, Signature signature, int lastNodeCharacter)
            {
                var runningScore = GetInitialScore(signature, lastNodeCharacter);

                // We only need to check the nodes that are different. As the nodes
                // are in the same order we can simply look for those that are different.
                int matchNodeIndex = 0;
                int signatureNodeIndex = 0;

                while (signatureNodeIndex < signature.NodeOffsets.Count &&
                       runningScore < state.LowestScore)
                {
                    var matchNodeOffset = matchNodeIndex >= state.Nodes.Count ?
                        int.MaxValue : state.Nodes[matchNodeIndex].Index;
                    var signatureNodeOffset = signature.NodeOffsets[signatureNodeIndex];
                    if (matchNodeOffset > signatureNodeOffset)
                    {
                        // The matched node is either not available, or is higher than
                        // the current signature node. The signature node is not contained
                        // in the match so we must score it.
                        var score = GetScore(state, signature.Nodes[signatureNodeIndex]);

                        // If the score is less than zero then a score could not be 
                        // determined and the signature can't be compared to the target
                        // User-Agent. Exit with a high score.
                        if (score < 0)
                            return int.MaxValue;
                        runningScore += score;
                        signatureNodeIndex++;
                    }
                    else if (matchNodeOffset == signatureNodeOffset)
                    {
                        // They both are the same so move to the next node in each.
                        matchNodeIndex++;
                        signatureNodeIndex++;
                    }
                    else if (matchNodeOffset < signatureNodeOffset)
                    {
                        // The match node is lower so move to the next one and see if
                        // it's higher or equal to the current signature node.
                        matchNodeIndex++;
                    }
                }

                return runningScore;
            }
        }

        /// <summary>
        /// Used to calculate the closest match result.
        /// </summary>
        private class ClosestScore : BaseScore
        {
            /// <summary>
            /// Calculate the initial score based on the difference in length of 
            /// the right most node and the target User-Agent.
            /// </summary>
            /// <param name="signature"></param>
            /// <param name="lastNodeCharacter"></param>
            /// <returns></returns>
            protected override int GetInitialScore(Signature signature, int lastNodeCharacter)
            {
                return Math.Abs(lastNodeCharacter + 1 - signature.Length);
            }

            /// <summary>
            /// Returns the difference score between the node and the target user
            /// agent working from right to left.
            /// </summary>
            /// <param name="state"></param>
            /// <param name="node"></param>
            /// <returns></returns>
            protected override int GetScore(MatchState state, Node node)
            {
                int score = 0;
                int nodeIndex = node.Characters.Length - 1, targetIndex = node.Position + node.Length;

                // Adjust the score and indexes if the node is too long.
                if (targetIndex >= state.TargetUserAgentArray.Length)
                {
                    score = targetIndex - state.TargetUserAgentArray.Length;
                    nodeIndex -= score;
                    targetIndex = state.TargetUserAgentArray.Length - 1;
                }

                while (nodeIndex >= 0 && score < state.LowestScore)
                {
                    var difference = Math.Abs(
                        state.TargetUserAgentArray[targetIndex] -
                        node.Characters[nodeIndex]);
                    if (difference != 0)
                    {
                        var numericDifference = NumericDifference(
                            node.Characters,
                            state.TargetUserAgentArray,
                            ref nodeIndex,
                            ref targetIndex);
                        if (numericDifference != 0)
                        {
                            score += numericDifference;
                        }
                        else
                        {
                            score += (difference * 10);
                        }
                    }
                    nodeIndex--;
                    targetIndex--;
                }

                return score;
            }
            
            /// <summary>
            /// Checks for a numeric rather than character difference 
            /// between the signature and the target User-Agent at the
            /// character position provided by the index. 
            /// Updates the difference variable based on the result.
            /// </summary>
            /// <param name="node">
            /// Node array of characters
            /// </param>
            /// <param name="target">
            /// Target User-Agent array
            /// </param>
            /// <param name="nodeIndex">
            /// Start character to be checked in the node array
            /// </param>
            /// <param name="targetIndex">
            /// Start character position to the checked in the target array
            /// </param>
            private static int NumericDifference(
                byte[] node,
                byte[] target,
                ref int nodeIndex,
                ref int targetIndex)
            {
                // Move right when the characters are numeric to ensure
                // the full number is considered in the difference comparison.
                var newNodeIndex = nodeIndex + 1;
                var newTargetIndex = targetIndex + 1;
                while (newNodeIndex < node.Length &&
                    newTargetIndex < target.Length &&
                    Utils.GetIsNumeric(target[newTargetIndex]) &&
                    Utils.GetIsNumeric(node[newNodeIndex]))
                {
                    newNodeIndex++;
                    newTargetIndex++;
                }
                nodeIndex = newNodeIndex - 1;
                targetIndex = newTargetIndex - 1;

                // Find when the characters stop being numbers.
                var characters = 0;
                while (
                    nodeIndex >= 0 &&
                    Utils.GetIsNumeric(target[targetIndex]) &&
                    Utils.GetIsNumeric(node[nodeIndex]))
                {
                    nodeIndex--;
                    targetIndex--;
                    characters++;
                }

                // If there is more than one character that isn't a number then
                // compare the numeric values.
                if (characters > 1)
                {
                    return Math.Abs(
                        Utils.GetNumber(target, targetIndex + 1, characters) -
                        Utils.GetNumber(node, nodeIndex + 1, characters));
                }

                return 0;
            }
        }

        /// <summary>
        /// Used to determine if all the signature node sub strings are in the target
        /// just at different character positions.
        /// </summary>
        private class NearestScore : BaseScore
        {
            protected override int GetInitialScore(Signature signature, int lastNodeCharacter)
            {
                return 0;
            }

            /// <summary>
            /// If the sub string is contained in the target but in a different position
            /// return the difference between the two sub string positions.
            /// </summary>
            /// <param name="state"></param>
            /// <param name="node"></param>
            /// <returns>-1 if a score can't be determined, or the difference in positions</returns>
            protected override int GetScore(MatchState state, Node node)
            {
                var index = state.IndexOf(node);
                if (index >= 0)
                {
                    return Math.Abs(node.Position + 1 - index);
                }

                // Return -1 to indicate that a score could not be calculated.
                return -1;
            }
        }

        #endregion

        #region Static Fields

        /// <summary>
        /// Used to calculate nearest scores between the match and the User-Agent.
        /// </summary>
        private static readonly NearestScore _nearest = new NearestScore();

        /// <summary>
        /// Used to calculate closest scores between the match and the User-Agent.
        /// </summary>
        private static readonly ClosestScore _closest = new ClosestScore();

        #endregion

        #region Internal Methods

        /// <summary>
        /// Entry point to the detection process. Provided with a <see cref="Match"/>
        /// configured with the information about the request.
        /// </summary>
        /// <remarks>
        /// The dataSet may be used by other threads in parallel and is not assumed to be used
        /// by only one detection process at a time.
        /// </remarks>
        /// <param name="state">The match state object to be updated.</param>
        /// <remarks>
        /// The memory implementation of the data set will always perform fastest but does
        /// consume more memory. 
        /// </remarks>
        internal static void Match(MatchState state)
        {
            if (state.DataSet.Disposed)
            {
                throw new InvalidOperationException(
                    "Data Set has been disposed and can't be used for match");
            }

            // If the User-Agent is too short then don't try to match and 
            // return defaults.
            if (state.TargetUserAgentArray.Length == 0 ||
                state.TargetUserAgentArray.Length < state.DataSet.MinUserAgentLength)
            {
                // Set the default values.
                MatchDefault(state);
            }
            else
            {
                // Starting at the far right evaluate the nodes in the data
                // set recording matched nodes. Continue until all character
                // positions have been checked.
                Evaluate(state);

                // Can a precise match be found based on the nodes?
                var signatureIndex = GetExactSignatureIndex(state);

                if (signatureIndex >= 0)
                {
                    // Yes a precise match was found.
                    state.Signature = state.DataSet.Signatures[signatureIndex];
                    state.Method = MatchMethods.Exact;
                    state.LowestScore = 0;
                }
                else
                {
                    // No. So find any other nodes that match if numeric differences
                    // are considered.
                    EvaluateNumeric(state);

                    // Can a precise match be found based on the nodes?
                    signatureIndex = GetExactSignatureIndex(state);

                    if (signatureIndex >= 0)
                    {
                        // Yes a precise match was found.
                        state.Signature = state.DataSet.Signatures[signatureIndex];
                        state.Method = MatchMethods.Numeric;
                    }
                    else if (state.Nodes.Count > 0)
                    {
                        // Look for the closest signatures to the nodes found.
                        var closestSignatures = GetClosestSignatures(state);

#if DEBUG
                        // Validate the list is in ascending order of ranked signature index.
                        var enumerator = closestSignatures.GetEnumerator();
                        var lastIndex = -1;
                        while (enumerator.MoveNext())
                        {
                            Debug.Assert(lastIndex < enumerator.Current);
                            lastIndex = enumerator.Current;
                        }
#endif

                        // Try finding a signature with identical nodes just 
                        // not in exactly the same place.
                        _nearest.EvaluateSignatures(state, closestSignatures);

                        if (state.Signature != null)
                        {
                            // All the sub strings matched, just in different 
                            // character positions.
                            state.Method = MatchMethods.Nearest;
                        }
                        else
                        {
                            // Find the closest signatures and compare them
                            // to the target looking at the smallest character
                            // difference.
                            _closest.EvaluateSignatures(state, closestSignatures);
                            state.Method = MatchMethods.Closest;
                        }
                    }
                }

                // If there still isn't a signature then set the default.
                if (state.Signature == null)
                {
                    MatchDefault(state);
                }
            }
        }

        /// <summary>
        /// If the nodes of the match correspond exactly to a signature then
        /// return the index of the signature found. Otherwise -1.
        /// </summary>
        /// <returns></returns>
        private static int GetExactSignatureIndex(MatchState state)
        {
            int iterations;
            int index = state.DataSet.SignatureSearch.BinarySearch(
                state.Nodes, out iterations);
            state.SignaturesRead += iterations;
            return index;
        }

        /// <summary>
        /// Returns an enumeration of the closest signatures which most closely 
        /// match the target User-Agent string. Where a single signature
        /// is not present across all the nodes the signatures which match
        /// the most nodes from the target User-Agent string are returned.
        /// </summary>
        /// <returns>An enumeration of the closest signatures</returns>
        private static IList<int> GetClosestSignatures(MatchState state)
        {
            IList<int> result;
            if (state.Nodes.Count == 1)
            {
                // Return the single nodes list of ranked signature indexes.
                result = state.Nodes[0].RankedSignatureIndexes;
            }
            else
            {
                // There are multiple lists so filter them to return the most
                // frequently occuring ranked signature indexes.
                result = new MostFrequentFilter(state);
            }
            state.ClosestSignatures = result.Count;
            return result;
        }
                
        #endregion

        #region Step 1 - Initial Match Methods
        
        /// <summary>
        /// Evaluates the match at the current character position until there
        /// are no more characters left to evaluate.
        /// </summary>
        /// <param name="state">Information about the detection</param>
        private static void Evaluate(MatchState state)
        {
            while (state.NextCharacterPositionIndex >= 0)
            {
                state.RootNodesEvaluated++;
                var node = state.DataSet.RootNodes[state.NextCharacterPositionIndex].GetCompleteNode(state);
                if (node != null)
                {
                    // Insert this node into the list for the match. It will always
                    // have a lower index than it's predecessors.
                    state.Nodes.Insert(
                        0,
                        node);

                    // Check from the next root node that can be positioned to 
                    // the left of this one.
                    state.NextCharacterPositionIndex = node.NextCharacterPosition;
                }
                else
                    // No nodees matched at the character position, move to the next 
                    // root node to the left.
                    state.NextCharacterPositionIndex--;
            }
        }
        
        #endregion

        #region Step 2 - Numeric Match Methods

        /// <summary>
        /// Evaluate the target User-Agent again, but this time
        /// </summary>
        /// <param name="state"></param>
        private static void EvaluateNumeric(MatchState state)
        {
            state.ResetNextCharacterPositionIndex();
            var existingNodeIndex = state.Nodes.Count - 1;
            while (state.NextCharacterPositionIndex > 0)
            {
                if (existingNodeIndex < 0 ||
                    state.Nodes[existingNodeIndex].Root.Position < state.NextCharacterPositionIndex)
                {
                    state.RootNodesEvaluated++;
                    var node = state.DataSet.RootNodes[state.NextCharacterPositionIndex].GetCompleteNumericNode(state);
                    // If there is a node and it doesn't overlap with an existing one then
                    // add it to the list.
                    if (node != null &&
                        node.GetIsOverlap(state) == false)
                    {
                        // Insert the node and update the existing index so that
                        // it's the node to the left of this one.
                        existingNodeIndex = state.InsertNode(node) - 1;

                        // Move to the position of the node found as 
                        // we can't use the next node incase there's another
                        // not part of the same signatures closer.
                        state.NextCharacterPositionIndex = node.Position;
                    }
                    else
                    {
                        state.NextCharacterPositionIndex--;
                    }
                }
                else
                {
                    // The next position to evaluate should be to the left
                    // of the existing node already in the list.
                    state.NextCharacterPositionIndex = state.Nodes[existingNodeIndex].Position;

                    // Swap the existing node for the next one in the list.
                    existingNodeIndex--;
                }
            }
        }

        #endregion

        #region Default Match

        /// <summary>
        /// The detection failed and a default match needs to be returned.
        /// </summary>
        /// <param name="state">Information about the match state</param>
        internal static void MatchDefault(MatchState state)
        {
            state.Method = MatchMethods.None;
            state.ExplicitProfiles.Clear();
            state.ExplicitProfiles.AddRange(state.DataSet.Components.Select(i => i.DefaultProfile));
        }
                        
        #endregion
    }
}
