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
using System.Text;

namespace FiftyOne.Foundation.Mobile.Detection.Entities
{
    /// <summary>
    /// A node index contains the characters and related child nodes of the 
    /// node should any of the characters match at the position.
    /// </summary>
    internal class NodeIndex : NodeIndexBase, IComparable<NodeIndex>
    {
        #region Fields

        /// <summary>
        /// True if the value is an index to a sub string. False
        /// if the value is 1 to 4 consecutive characters.
        /// </summary>
        internal readonly bool IsString;

        /// <summary>
        /// The value of the node index. Interpretation
        /// depends on IsSubString. 
        /// </summary>
        /// <remarks>
        /// If IsSubString is true the 4 bytes represent an offset 
        /// in the strings data structure to 5 or more characters. 
        /// If IsSubString is false the 4 bytes are character 
        /// values themselves where 0 values are ignored.
        /// </remarks>
        private readonly byte[] _value;

        #endregion

        #region Properties

        /// <summary>
        /// Returns the characters related to this node index.
        /// </summary>
        internal byte[] Characters
        {
            get
            {
                if (_characters == null)
                {
                    lock (this)
                    {
                        if (_characters == null)
                        {
                            _characters = GetCharacters();
                        }
                    }
                }
                return _characters;
            }
        }
        private byte[] _characters;
              
        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new instance of <see cref="NodeIndex"/>.
        /// </summary>
        /// <param name="dataSet">
        /// The data set the node is contained within.
        /// </param>
        /// <param name="index">
        /// The index of this object in the Node.
        /// </param>
        /// <param name="isString">
        /// True if the value is an integer offset to a string, or false 
        /// if the value is an array of characters to be used by the node.
        /// </param>
        /// <param name="value">
        /// Array of bytes representing an integer offset to a string, or
        /// the array of characters to be used by the node.
        /// </param>
        /// <param name="relatedNodeOffset">
        /// The offset in the list of nodes to the node the index relates to.
        /// </param>
        internal NodeIndex(
            DataSet dataSet,
            int index,
            bool isString,
            byte[] value,
            int relatedNodeOffset)
            : base(dataSet, index, relatedNodeOffset)
        {
            IsString = isString;
            _value = value;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Called after the entire data set has been loaded to ensure 
        /// any further initialisation steps that require other items in
        /// the data set can be completed.
        /// </summary>
        internal override void Init()
        {
            base.Init();
            if (_characters == null)
            {
                _characters = GetCharacters();
            }
        }

        /// <summary>
        /// Returns the characters the node index relates to.
        /// </summary>
        /// <returns></returns>
        private byte[] GetCharacters()
        {
            if (IsString)
            {
                // Returns the characters based on the sub string referenced.
                return DataSet.Strings[BitConverter.ToInt32(_value, 0)].Value;
            }
            else
            {
                // Return the byte array.
                return _value;
            }
        }

        /// <summary>
        /// Compares a byte array of characters at the position provided
        /// to the array of characters for this node.
        /// </summary>
        /// <param name="other">Array of characters to compare</param>
        /// <param name="startIndex">
        /// The index in the other array to the required characters
        /// </param>
        /// <returns>
        /// The relative position of the node in relation to the other array
        /// </returns>
        /// <para>
        /// Used to determine if a target User-Agent contains the node.
        /// </para>
        internal int CompareTo(byte[] other, int startIndex)
        {
            for (int i = Characters.Length - 1, o = startIndex + Characters.Length - 1; i >= 0; i--, o--)
            {
                var difference = Characters[i].CompareTo(other[o]);
                if (difference != 0)
                    return difference;
            }

            return 0;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Compares this node index to another.
        /// </summary>
        /// <param name="other">
        /// The node index to compare.
        /// </param>
        /// <returns>
        /// Indication of relative value based on ComponentId field.
        /// </returns>
        public int CompareTo(NodeIndex other)
        {
            return CompareTo(other.Characters, 0);
        }

        /// <summary>
        /// Converts the node index into a string.
        /// </summary>
        /// <returns>
        /// A string representation of the node characters.
        /// </returns>
        public override string ToString()
        {
            return String.Format(
                "{0}[{1}]",
                Encoding.ASCII.GetString(Characters),
                RelatedNodeOffset);
        }

        #endregion
    }
}
