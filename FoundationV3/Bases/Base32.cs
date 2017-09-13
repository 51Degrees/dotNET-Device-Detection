/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2017 51Degrees Mobile Experts Limited, 5 Charlotte Close,
 * Caversham, Reading, Berkshire, United Kingdom RG4 7BY
 * 
 * This Source Code Form is the subject of the following patents and patent 
 * applications, owned by 51Degrees Mobile Experts Limited of 5 Charlotte
 * Close, Caversham, Reading, Berkshire, United Kingdom RG4 7BY: 
 * European Patent No. 2871816;
 * European Patent Application No. 17184134.9;
 * United States Patent Nos. 9,332,086 and 9,350,823; and
 * United States Patent Application No. 15/686,066.
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

using System.Text;
using System;
using System.IO;

namespace FiftyOne.Foundation.Bases
{
    /// <summary>
    /// <para>
    /// Contains methods to convert data into <see cref="Base32"/> strings.
    /// </para>
    /// <remarks>
    /// <para>
    /// Many mobile devices contain defects that subtly change strings
    /// used as query string parameters or as hidden fields. Encoding strings
    /// using <see cref="Base32"/> ensures the string can always be
    /// read back during subsequent requests. For example a Base64 encoded 
    /// string will use upper and lower case letters. A 
    /// <see cref="Base32"/> encoded strings uses upper case letters and 
    /// numbers only ensuring any mobile device that converts query string
    /// parameters to lower case do not alter the encoded contents.
    /// </para>
    /// <para>
    /// The <see cref="Base32"/> encoding used in this implementation
    /// removes commonly mistyped charaters such as the number 0 (zero) and
    /// the letter O. This means less mistakes will be made if someone copies
    /// out the URL including query string parameters.
    /// </para>
    /// <note type="caution">
    /// We suggest you keep the amount of information stored in query string 
    /// parameters and hidden fields to a minimum to ensure the lowest possible
    /// page weight. Hold information in the session store or a persitent
    /// database that supports stateless operation of your application and store a 
    /// unique reference in hidden fields or query string parameters encoded using
    /// <see cref="Base32"/>.
    /// </note>
    /// </remarks>
    /// </summary>
    public class Base32
    {
        #region Fields

        // the valid chars for the encoding
        private static string ValidChars = "ABCDEFGH" + "JKLMNPQR" + "STUVWXYZ" + "23456789";

        #endregion

        #region Methods

        /// <summary>
        /// <para>
        /// Encodes a plain string as a <see cref="Base32"/> string.
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>
        /// Use the <c>DecodeToString</c> method to decode strings
        /// encoded with this method.
        /// </para>
        /// </remarks>
        /// <param name="value">String to be encoded.</param>
        /// <returns>String encoding of the string.</returns>
        public static string Encode(string value)
        {
            ASCIIEncoding encoding = new ASCIIEncoding();
            return Encode(encoding.GetBytes(value));
        }

        /// <summary>
        /// <para>
        /// Encodes a short array as a <see cref="Base32"/> string.
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>
        /// Use the <see cref="DecodeToShortArray"/> method to decode strings
        /// encoded with this method.
        /// </para>
        /// </remarks>
        /// <param name="values">An array of shorts to be encoded.</param>
        /// <returns>String encoding of the short array.</returns>
        public static string Encode(short[] values)
        {
            using (MemoryStream buffer = new MemoryStream())
            {
                foreach (short value in values)
                    buffer.Write(BitConverter.GetBytes(value), 0, 2);
                return Encode(buffer.ToArray());
            }
        }

        /// <summary>
        /// <para>
        /// Encodes an integer as a <see cref="Base32"/> string.
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>
        /// Use the <c>DecodeToInteger</c> method to decode strings
        /// encoded with this method.
        /// </para>
        /// </remarks>
        /// <param name="value">Integer to be encoded.</param>
        /// <returns>String encoding of the integer.</returns>
        public static string Encode(int value)
        {
            return Encode(BitConverter.GetBytes(value));
        }
        
        /// <summary>
        /// <para>
        /// Encodes a byte array as a <see cref="Base32"/> encoded string.
        /// </para>
        /// </summary>
        /// <remarks>
        /// <example>
        /// <para>
        /// The following code will encrypt the string "Hello World" and store
        /// the result as a <see cref="Base32"/> encoded string in the variable
        /// <c>secure</c>.
        /// </para>
        /// <code lang="C#">
        ///     string secure = Mobile.Base32.Encode(
        ///         Mobile.Crypto.Encrypt("Hello World"));
        /// </code>
        /// <para>
        /// <c>secure</c> can be decoded using the following code.
        /// </para>
        /// <code lang="C#">
        ///     string plain = Mobile.Crypto.Decrypt(
        ///         Mobile.Base32.Decode(secure));
        /// </code>
        /// </example>
        /// </remarks>
        /// <param name="bytes">Byte array to encode.</param>
        /// <returns>String encoding of the byte array.</returns>
        public static string Encode(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();         // holds the base32 chars
            byte index;
            int hi = 5;
            int currentByte = 0;

            while (currentByte < bytes.Length)
            {
                // do we need to use the next byte?
                if (hi > 8)
                {
                    // get the last piece from the current byte, shift it to the right
                    // and increment the byte counter
                    index = (byte)(bytes[currentByte++] >> (hi - 5));
                    if (currentByte != bytes.Length)
                    {
                        // if we are not at the end, get the first piece from
                        // the next byte, clear it and shift it to the left
                        index = (byte)(((byte)(bytes[currentByte] << (16 - hi)) >> 3) | index);
                    }

                    hi -= 3;
                }
                else if (hi == 8)
                {
                    index = (byte)(bytes[currentByte++] >> 3);
                    hi -= 3;
                }
                else
                {

                    // simply get the stuff from the current byte
                    index = (byte)((byte)(bytes[currentByte] << (8 - hi)) >> 3);
                    hi += 5;
                }

                sb.Append(ValidChars[index]);
            }

            return sb.ToString();
        }

        /// <summary>
        /// <para>
        /// Takes a string previously generate with the <c>Encode</c> method and 
        /// returns the original string value.
        /// </para>
        /// </summary>
        /// <param name="value"><see cref="Base32"/> encoded string data.
        /// </param>
        /// <returns>The decoded string value.</returns>
        public static string DecodeToString(string value)
        {
            return Encoding.ASCII.GetString(Decode(value));
        }

        /// <summary>
        /// <para>
        /// Takes a string previously generate with the <c>Encode</c> method and 
        /// returns the original short array value.
        /// </para>
        /// </summary>
        /// <param name="value"><see cref="Base32"/> encoded string data.</param>
        /// <returns>The decoded short array.</returns>
        public static short[] DecodeToShortArray(string value)
        {
            byte[] bytes = Decode(value);
            short[] values = new short[bytes.Length / 2];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = BitConverter.ToInt16(bytes, i * 2);
            }
            return values;
        }

        /// <summary>
        /// <para>
        /// Takes a string previously generate with the <c>Encode</c> method and 
        /// returns the original integer value.
        /// </para>
        /// </summary>
        /// <param name="value"><see cref="Base32"/> encoded string data.</param>
        /// <returns>The decoded integer value.</returns>
        public static int DecodeToInteger(string value)
        {
            return BitConverter.ToInt32(Decode(value), 0);
        }

        /// <summary>
        /// <para>
        /// Takes a string previously generate with the <c>Encode</c> method and 
        /// returns the original byte array.
        /// </para>
        /// </summary>
        /// <param name="value"><see cref="Base32"/> encoded string data.</param>
        /// <returns>The decoded byte array.</returns>
        public static byte[] Decode(string value)
        {
            byte[] bytes = null;
            try
            {
                int numBytes = value.Length * 5 / 8;
                bytes = new byte[numBytes];

                // all UPPERCASE chars
                value = value.ToUpper();

                int bit_buffer;
                int currentCharIndex;
                int bits_in_buffer;

                if (value.Length < 3)
                {
                    bytes[0] = (byte)(ValidChars.IndexOf(value[0]) | ValidChars.IndexOf(value[1]) << 5);
                    return bytes;
                }

                bit_buffer = (ValidChars.IndexOf(value[0]) | ValidChars.IndexOf(value[1]) << 5);
                bits_in_buffer = 10;
                currentCharIndex = 2;
                for (int i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = (byte)bit_buffer;
                    bit_buffer >>= 8;
                    bits_in_buffer -= 8;
                    while (bits_in_buffer < 8 && currentCharIndex < value.Length)
                    {
                        bit_buffer |= ValidChars.IndexOf(value[currentCharIndex++]) << bits_in_buffer;
                        bits_in_buffer += 5;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
            return bytes;
        }

        #endregion
    }
}