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

using FiftyOne.Foundation.Properties;
using System.IO;
using System.Reflection;
using System.Linq;

namespace FiftyOne.Foundation.Licence
{
    /// <summary>
    /// Represents information about a 51Degrees product.
    /// </summary>
    public class Product
    {
        #region Public Fields

        /// <summary>
        /// Holds product id.
        /// </summary>
        public readonly byte Id;
        
        /// <summary>
        /// Holds product version.
        /// </summary>
        public readonly byte Version;
        
        /// <summary>
        /// Number of installation count.
        /// </summary>
        public readonly byte Quantity;

        /// <summary>
        /// The type of Licence for the product.
        /// </summary>
        public readonly LicenceTypes Type;

        /// <summary>
        /// The version of the assembly.
        /// </summary>
        public readonly byte AssemblyVersion;
        
        #endregion

        #region Public Properties
        
        /// <summary>
        /// Returns true if the product is valid for the current assembly.
        /// </summary>
        public bool IsValid
        {
            get
            {
                return (Version <= AssemblyVersion && 
                    LicenceConstants.ProductIDs.Contains(Id));
            }
        }

        /// <summary>
        /// Returns true if the product is valid and for an evaluation.
        /// </summary>
        public bool IsTrial
        {
            get
            {
                return IsValid && Type == LicenceTypes.Evaluation;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs an object of type <see cref="Product"/>.
        /// </summary>
        public Product() 
        {
            AssemblyVersion = (byte)GetType().Assembly.GetName().Version.Major;
        }

        /// <summary>
        /// Constructs an object of type <see cref="Product"/>.
        /// </summary>
        /// <param name="productId">Id of the product.</param>
        /// <param name="version">Product version.</param>
        /// <param name="quantity">Number of installations allowed for this product.</param>
        /// <param name="type">The type of Licence.</param>
        public Product(byte productId, byte version, byte quantity, LicenceTypes type) 
            : this()
        {
            Id = productId;
            Version = version;
            Quantity = quantity;
            Type = type;
        }

        /// <summary>
        /// Constructs an object of type <see cref="Product"/>.
        /// </summary>
        /// <param name="reader">Reader attached to a source data stream.</param>
        internal Product(BinaryReader reader)
            : this()
        {
            Id = reader.ReadByte();
            Version = reader.ReadByte();
            Quantity = reader.ReadByte();
            Type = (LicenceTypes)reader.ReadByte();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Persists data about the product.
        /// </summary>
        /// <param name="writer">Writer attached to an output stream for data storage.</param>
        internal void Write(BinaryWriter writer)
        {
            writer.Write(Id);
            writer.Write(Version);
            writer.Write(Quantity);
            writer.Write((byte)Type);
        }

        #endregion
    }
}