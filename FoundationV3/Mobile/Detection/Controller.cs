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
    /// provided user agent.
    /// </para>
    /// <para>
    /// Step 1 - each of the character positions of the target user agent are checked
    /// from right to left to determine if a complete node or sub string is present
    /// at that position. For example; the sub string Chrome/11 might indicate the 
    /// user agent relates to Chrome version 11 from Google. Once every character
    /// position is checked a list of matching nodes will be available.
    /// </para>
    /// <para>
    /// Step 2 - The list of signatures is then searched to determine if the matching 
    /// nodes relate exactly to an existing signature. Any popular device will be found
    /// at this point. The approach is exceptionally fast at identifying popular
    /// devices. This is termed the Exact match method.
    /// </para>
    /// <para>
    /// Step 3 - If a match has not been found exactly then the target user agent is
    /// evaluated to find nodes that have the smallest numeric difference. For example
    /// if Chrome/40 were in the target user agent at the same position as Chrome/32 
    /// in the signature then Chrome/32 with a numeric difference score of 8 would be
    /// used. If a signature can then be matched exactly against the new set of nodes
    /// this will be returned. This is termed the Numeric match method.
    /// </para>
    /// <para>
    /// Step 4 - If the target user agent is less popular, or newer than the creation time
    /// of the data set, a small sub set of possible signatures are identified from the
    /// matchied nodes. The sub set is limited to the most popular 200 signatures.
    /// </para>
    /// <para>
    /// Step 5 - The sub strings of the signatures from Step 4 are then evaluated to determine
    /// if they exist in the target user agent. For example if Chrome/32 in the target 
    /// appears one character to the left of Chrome/32 in the signature then a difference
    /// of 1 would be returned between the signature and the target. This is termed the
    /// Nearest match method.
    /// </para>
    /// <para>
    /// Step 6 - The signatures from Step 4 are evaluated against the target user agent to 
    /// determine the difference in relevent characters between them. The signature with the 
    /// lowest difference in ASCII character values with the target is returned. This is termed
    /// the Closest match method.
    /// </para>
    /// <remarks>
    /// Random user agents will not identify any matching nodes. In these situations a 
    /// default set of profiles are returned.
    /// </remarks>
    /// <remarks>
    /// The characteristics of the detection data set will determine the accuracy of the
    /// result match. Older data sets that are unaware of the latest devices, or user agent
    /// formats in use will be less accurate.
    /// </remarks>
    /// <para>
    /// For more information see https://51degrees.com/Support/Documentation/Net
    /// </para>
    internal static class Controller
    {
        #region Classes used by Nearest and Closest methods

        private abstract class BaseScore
        {
            /// <summary>
            /// Gets the score for the specific node of the signature.
            /// </summary>
            /// <param name="match"></param>
            /// <param name="node"></param>
            /// <returns></returns>
            protected abstract int GetScore(Match match, Node node);

            /// <summary>
            /// Sets any initial score before each node is evaluated.
            /// </summary>
            /// <param name="match"></param>
            /// <param name="signature"></param>
            /// <param name="lastNodeCharacter"></param>
            /// <returns></returns>
            protected abstract int GetInitialScore(Match match, Signature signature, int lastNodeCharacter);

            /// <summary>
            /// Checks all the signatures using the scoring method provided.
            /// </summary>
            /// <param name="signatures"></param>
            /// <param name="match"></param>
            internal void EvaluateSignatures(Match match, IEnumerable<Signature> signatures)
            {
                match.LowestScore = int.MaxValue;
                var lastNodeCharacter = match.Nodes.Last().Root.Position;
                foreach (var signature in signatures.Take(match.DataSet.MaxSignatures))
                    EvaluateSignature(match, signature, lastNodeCharacter);
            }

            /// <summary>
            /// Compares all the characters up to the max length between the 
            /// signature and the target user agent updating the match
            /// information if this signature is better than any evaluated
            /// previously.
            /// </summary>
            /// <param name="match">Information about the detection</param>
            /// <param name="lastNodeCharacter">
            /// The position of the last character in the matched nodes
            /// </param>
            /// <param name="signature">
            /// The siganture to be evaluated.
            /// </param>
            private void EvaluateSignature(
                Match match,
                Signature signature,
                int lastNodeCharacter)
            {
                match._signaturesCompared++;

                // Get the score between the target and the signature stopping if it's
                // going to be larger than the lowest score already found.
                int score = GetScore(match, signature, lastNodeCharacter);

                // If the score is lower than the current lowest then use this signature.
                if (score < match.LowestScore)
                {
                    match.LowestScore = score;
                    match._signature = signature;
                }
            }

            /// <summary>
            /// Steps through the nodes of the signature comparing those that aren't
            /// contained in the matched nodes to determine a score between the signature
            /// and the target user agent. If that score becomes greater or equal to the
            /// lowest score determined so far then stop.
            /// </summary>
            /// <param name="match"></param>
            /// <param name="signature"></param>
            /// <param name="lastNodeCharacter"></param>
            /// <returns></returns>
            private int GetScore(Match match, Signature signature, int lastNodeCharacter)
            {
                var runningScore = GetInitialScore(match, signature, lastNodeCharacter);

                // We only need to check the nodes that are different. As the nodes
                // are in the same order we can simply look for those that are different.
                int matchNodeIndex = 0;
                int signatureNodeIndex = 0;

                while (signatureNodeIndex < signature.NodeOffsets.Length &&
                       runningScore < match.LowestScore)
                {
                    var matchNodeOffset = matchNodeIndex >= match.Nodes.Count ? int.MaxValue : match.Nodes[matchNodeIndex].Index;
                    var signatureNodeOffset = signature.NodeOffsets[signatureNodeIndex];
                    if (matchNodeOffset > signatureNodeOffset)
                    {
                        // The matched node is either not available, or is higher than
                        // the current signature node. The signature node is not contained
                        // in the match so we must score it.
                        var score = GetScore(match, signature.Nodes[signatureNodeIndex]);

                        // If the score is less than zero then a score could not be 
                        // determined and the signature can't be compared to the target
                        // user agent. Exit with a high score.
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
            /// the right most node and the target user agent.
            /// </summary>
            /// <param name="match"></param>
            /// <param name="signature"></param>
            /// <param name="lastNodeCharacter"></param>
            /// <returns></returns>
            protected override int GetInitialScore(Match match, Signature signature, int lastNodeCharacter)
            {
                return Math.Abs(lastNodeCharacter + 1 - signature.Length);
            }

            /// <summary>
            /// Returns the difference score between the node and the target user
            /// agent working from right to left.
            /// </summary>
            /// <param name="match"></param>
            /// <param name="node"></param>
            /// <returns></returns>
            protected override int GetScore(Match match, Node node)
            {
                int score = 0;
                int nodeIndex = node.Characters.Length - 1, targetIndex = node.Position + node.Length;

                // Adjust the score and indexes if the node is too long.
                if (targetIndex >= match.TargetUserAgentArray.Length)
                {
                    score = targetIndex - match.TargetUserAgentArray.Length;
                    nodeIndex -= score;
                    targetIndex = match.TargetUserAgentArray.Length - 1;
                }

                while (nodeIndex >= 0 && score < match.LowestScore)
                {
                    var difference = Math.Abs(
                        match.TargetUserAgentArray[targetIndex] -
                        node.Characters[nodeIndex]);
                    if (difference != 0)
                    {
                        var numericDifference = NumericDifference(
                            node.Characters,
                            match.TargetUserAgentArray,
                            ref nodeIndex,
                            ref targetIndex);
                        if (numericDifference != 0)
                            score += numericDifference;
                        else
                            score += (difference * 10);
                    }
                    nodeIndex--;
                    targetIndex--;
                }

                return score;
            }


            /// <summary>
            /// Checks for a numeric rather than character difference 
            /// between the signature and the target user agent at the
            /// character position provided by the index. 
            /// Updates the difference variable based on the result.
            /// </summary>
            /// <param name="node">
            /// Node array of characters
            /// </param>
            /// <param name="target">
            /// Target user agent array
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
                    BaseEntity.GetIsNumeric(target[newTargetIndex]) &&
                    BaseEntity.GetIsNumeric(node[newNodeIndex]))
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
                    BaseEntity.GetIsNumeric(target[targetIndex]) &&
                    BaseEntity.GetIsNumeric(node[nodeIndex]))
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
                        BaseEntity.GetNumber(target, targetIndex + 1, characters) -
                        BaseEntity.GetNumber(node, nodeIndex + 1, characters));
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
            protected override int GetInitialScore(Match match, Signature signature, int lastNodeCharacter)
            {
                return 0;
            }

            /// <summary>
            /// If the sub string is contained in the target but in a different position
            /// return the difference between the two sub string positions.
            /// </summary>
            /// <param name="match"></param>
            /// <param name="node"></param>
            /// <returns>-1 if a score can't be determined, or the difference in positions</returns>
            protected override int GetScore(Match match, Node node)
            {
                var index = match.IndexOf(node);
                if (index >= 0)
                    return Math.Abs(node.Position + 1 - index);

                // Return -1 to indicate that a score could not be calculated.
                return -1;
            }
        }

        #endregion

        #region Static Fields

        /// <summary>
        /// Used to calculate nearest scores between the match and the user agent.
        /// </summary>
        private static readonly NearestScore _nearest = new NearestScore();

        /// <summary>
        /// Used to calculate closest scores between the match and the user agent.
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
        /// <param name="match">
        /// The match object to be updated.
        /// </param>
        /// <remarks>
        /// The memory implementation of the data set will always perform fastest but does
        /// consume more memory. 
        /// </remarks>
        internal static void Match(
            Match match)
        {
            if (match.DataSet.Disposed)
                throw new InvalidOperationException(
                    "Data Set has been disposed and can't be used for match");

            // If the user agent is too short then don't try to match and 
            // return defaults.
            if (match.TargetUserAgentArray.Length == 0 ||
                match.TargetUserAgentArray.Length < match.DataSet.MinUserAgentLength)
            {
                // Set the default values.
                MatchDefault(match);
            }
            else
            {
                // Starting at the far right evaluate the nodes in the data
                // set recording matched nodes. Continue until all character
                // positions have been checked.
                Evaluate(match);

                // Can a precise match be found based on the nodes?
                var signatureIndex = match.GetExactSignatureIndex();

                if (signatureIndex >= 0)
                {
                    // Yes a precise match was found.
                    match._signature = match.DataSet.Signatures[signatureIndex];
                    match._method = MatchMethods.Exact;
                    match.LowestScore = 0;
                }
                else
                {
                    // No. So find any other nodes that match if numeric differences
                    // are considered.
                    EvaluateNumeric(match);

                    // Can a precise match be found based on the nodes?
                    signatureIndex = match.GetExactSignatureIndex();

                    if (signatureIndex >= 0)
                    {
                        // Yes a precise match was found.
                        match._signature = match.DataSet.Signatures[signatureIndex];
                        match._method = MatchMethods.Numeric;
                    }
                    else if (match.Nodes.Count > 0)
                    {
                        // Look for the closest signatures to the nodes found.
                        var closestSignatures = match.GetClosestSignatures();

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

                        // Try finding a signature with identical nodes just not in exactly the 
                        // same place.
                        _nearest.EvaluateSignatures(match, closestSignatures.Select(i =>
                            match.DataSet.Signatures[match.DataSet.RankedSignatureIndexes[i].Value]));

                        if (match._signature != null)
                        {
                            // All the sub strings matched, just in different character positions.
                            match._method = MatchMethods.Nearest;
                        }
                        else
                        {
                            // Find the closest signatures and compare them
                            // to the target looking at the smallest character
                            // difference.
                            _closest.EvaluateSignatures(match, closestSignatures.Select(i =>
                                match.DataSet.Signatures[match.DataSet.RankedSignatureIndexes[i].Value]));
                            match._method = MatchMethods.Closest;
                        }
                    }
                }

                // If there still isn't a signature then set the default.
                if (match._profiles == null &&
                    match._signature == null)
                    MatchDefault(match);
            }
        }
                        
        #endregion

        #region Step 1 - Initial Match Methods
        
        /// <summary>
        /// Evaluates the match at the current character position until there
        /// are no more characters left to evaluate.
        /// </summary>
        /// <param name="match">Information about the detection</param>
        private static void Evaluate(Match match)
        {
            while (match.NextCharacterPositionIndex >= 0)
            {
                match._rootNodesEvaluated++;
                var node = match.DataSet.RootNodes[match.NextCharacterPositionIndex].GetCompleteNode(match);
                if (node != null)
                {
                    // Insert this node into the list for the match. It will always
                    // have a lower index than it's predecessors.
                    match.Nodes.Insert(
                        0,
                        node);

                    // Check from the next root node that can be positioned to 
                    // the left of this one.
                    match.NextCharacterPositionIndex = node.NextCharacterPosition;
                }
                else
                    // No nodees matched at the character position, move to the next 
                    // root node to the left.
                    match.NextCharacterPositionIndex--;
            }
        }
        
        #endregion

        #region Step 2 - Numeric Match Methods

        /// <summary>
        /// Evaluate the target user agent again, but this time
        /// </summary>
        /// <param name="match"></param>
        private static void EvaluateNumeric(Match match)
        {
            match.ResetNextCharacterPositionIndex();
            var existingNodeIndex = match.Nodes.Count - 1;
            while (match.NextCharacterPositionIndex > 0)
            {
                if (existingNodeIndex < 0 || 
                    match.Nodes[existingNodeIndex].Root.Position < match.NextCharacterPositionIndex)
                {
                    match._rootNodesEvaluated++;
                    var node = match.DataSet.RootNodes[match.NextCharacterPositionIndex].GetCompleteNumericNode(match);
                    // If there is a node and it doesn't overlap with an existing one then
                    // add it to the list.
                    if (node != null &&
                        node.GetIsOverlap(match) == false)
                    {
                        // Insert the node and update the existing index so that
                        // it's the node to the left of this one.
                        existingNodeIndex = match.InsertNode(node) - 1;

                        // Move to the position of the node found as 
                        // we can't use the next node incase there's another
                        // not part of the same signatures closer.
                        match.NextCharacterPositionIndex = node.Position;
                    }
                    else
                        match.NextCharacterPositionIndex--;
                }
                else
                {
                    // The next position to evaluate should be to the left
                    // of the existing node already in the list.
                    match.NextCharacterPositionIndex = match.Nodes[existingNodeIndex].Position;

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
        /// <param name="match">Information about the detection</param>
        internal static void MatchDefault(Match match)
        {
            match._method = MatchMethods.None;
            match._profiles = match.DataSet.Components.Select(i => i.DefaultProfile).ToArray();
        }
                        
        #endregion
    }
}
