using System;
using System.Collections.Generic;
using System.Web.Services;

namespace Detector
{
    /// <summary>
    /// Class used to provide properties and associated values.
    /// </summary>
    [Serializable]
    public class Property
    {
        public string Name;
        public string[] Values;

        public Property() { }

        public Property(string name, string[] values)
        {
            Name = name;
            Values = values;
        }
    }

    /// <summary>
    /// Example web server for returning 51Degrees.mobi properties. Session is disabled.
    /// </summary>
    [WebService(Namespace = "http://mobiledevice.51degrees.mobi/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    public class MobileDevice : System.Web.Services.WebService
    {
        /// <summary>
        /// Returns a property value based on the name.
        /// Returns null if a property name match wasn't found.
        /// </summary>
        /// <param name="capability">The name of a 51Degrees.mobi or .NET property.</param>
        /// <returns></returns>
        [WebMethod(false)]
        public string GetProperty(string propertyName)
        {
            // First looks for a 51Degrees.mobi property name.
            if (Context.Request.Browser[propertyName] != null)
                return Context.Request.Browser[propertyName];

            // Then looks for a .NET property name.
            if (Context.Request.Browser.Capabilities[propertyName] != null)
                return Context.Request.Browser.Capabilities[propertyName].ToString();

            // No match found, return null.
            return null;
        }

        /// <summary>
        /// Returns a list of properties in a string array in the same order they were given.
        /// Any property not matched are set as null.
        /// </summary>
        /// <param name="propertyNames">An array of 51Degrees.mobi or .NET capability names.</param>
        /// <returns></returns>
        [WebMethod(false)]
        public string[] GetProperties(string[] propertyNames)
        {
            // Checks if the array is populated.
            if (propertyNames == null)
                return null;

            // Cycles through the array and replaces the property string with the resulting property string.
            for (int i = 0; i < propertyNames.Length; i++)
            {
                // First looks for a 51Degrees.mobi property name.
                if (Context.Request.Browser[propertyNames[i]] != null)
                    propertyNames[i] = Context.Request.Browser[propertyNames[i]];

                    // Then tries a .NET property name.
                else if (Context.Request.Browser.Capabilities[propertyNames[i]] != null)
                    propertyNames[i] = Context.Request.Browser.Capabilities[propertyNames[i]].ToString();

                    // Property name not found, give null instead.
                else
                    propertyNames[i] = null;
            }
            return propertyNames;
        }

        /// <summary>
        /// Returns an array of property types containing the property names
        /// and values for all properties associated with the device.
        /// </summary>
        /// <returns>Array of property types.</returns>
        [WebMethod(false)]
        public Property[] GetAllProperties()
        {
            List<Property> results = new List<Property>();

            // Get all the properties and values for the device.
            SortedList<string, List<string>> allProperties = 
                Context.Request.Browser.Capabilities["51Degrees.mobi"] as SortedList<string, List<string>>;

            // Check properties are available in case the device couldn't
            // be determined.
            if (allProperties != null)
            {
                // Copy all the properties and values to the return format.
                allProperties.Capacity = allProperties.Count;
                foreach (string key in allProperties.Keys)
                    if (allProperties[key].Count > 0)
                        results.Add(new Property(key, allProperties[key].ToArray()));
            }

            return results.ToArray();
        }
    }
}