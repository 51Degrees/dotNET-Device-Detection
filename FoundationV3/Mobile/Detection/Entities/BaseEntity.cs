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

using System.Collections.Generic;
using System.IO;
using System;
namespace FiftyOne.Foundation.Mobile.Detection.Entities
{
    /// <summary>
    /// Base class for all entities in the <see cref="DataSet"/>.
    /// </summary>
    /// <para>
    /// All entities must belong to a data set and contain a unique integer key.
    /// This class provides this functionality along with many common methods
    /// used by multiple entities.
    /// </para>
    /// <para>
    /// For more information see
    /// https://51degrees.com/Support/Documentation/Net
    /// </para>
    /// <remarks>Not intended to be used directly by 3rd parties.</remarks>
    public abstract class BaseEntity : 
        IComparable<BaseEntity>, IComparable<int>, IEquatable<int>
    {
        #region Properties

        /// <summary>
        /// The data set the entity relates to.
        /// </summary>
        public virtual IDataSet DataSet { get; private set; }

        /// <summary>
        /// The unique index of the item in the collection of items, or
        /// the unique offset to the item in the source data structure.
        /// </summary>
        public readonly int Index;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs the base item for the data set and index provided.
        /// </summary>
        /// <remarks>Not intended to be used directly by 3rd parties.</remarks>
        /// <param name="dataSet">
        /// The <see cref="IDataSet"/> being created
        /// </param>
        /// <param name="indexOrOffset">
        /// The unique index of the item in the collection of items, or
        /// the unique offset to the item in the source data structure.
        /// </param>
        protected BaseEntity(IDataSet dataSet, int indexOrOffset)
        {
            DataSet = dataSet;
            Index = indexOrOffset;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Compares the integer index or offset provided to the this entities.
        /// </summary>
        /// <param name="indexOrOffset">
        /// The unique index of the item in the collection of items, or
        /// the unique offset to the item in the source data structure.
        /// </param>
        /// <returns>
        /// A signed number indicating the relative values of this instance and 
        /// value.
        /// <list type="bullet">
        /// <item>Less than zero: This instance is less than value.</item>
        /// <item>Zero: This instance is equal to value.</item>
        /// <item>Greater than zero: This instance is greater than value.</item>
        /// </list>
        /// </returns>
        public int CompareTo(int indexOrOffset)
        {
            return Index.CompareTo(indexOrOffset);
        }

        /// <summary>
        /// Compares entities based on their Index properties.
        /// </summary>
        /// <param name="other">
        /// The entity to be compared against.
        /// </param>
        /// <returns>
        /// The position of one entity over the other.
        /// </returns>
        public int CompareTo(BaseEntity other)
        {
            return CompareTo(other.Index);
        }
        
        /// <summary>
        /// Evaluates the index of the entity to the value provided for equality.
        /// </summary>
        /// <param name="other">Value to evaluate for equality</param>
        /// <returns>True if the other value equals the index, otherwise false</returns>
        public bool Equals(int other)
        {
            return Index.Equals(other);
        }

        /// <summary>
        /// Returns the index as the hash code for the entity. This can only be considered
        /// when using entities from the same data set.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Index;
        }

        #endregion
    }
}
