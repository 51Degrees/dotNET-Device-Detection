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
using FiftyOne.Foundation.Mobile.Configuration;

namespace FiftyOne.Foundation.UI
{
    internal class FilterData
    {
        internal string Property = String.Empty;
        internal string MatchExpression = String.Empty;
        internal bool Enabled = true;

        private LocationData _parent = null;

        internal LocationData Parent
        {
            get { return _parent; }
        }

        internal FilterData(LocationData parent)
        {
            _parent = parent;
        }

        internal FilterData(LocationData parent, BinaryReader reader) 
            : this (parent)
        {
            
            Deserialize(reader);
        }

        internal FilterData(LocationData parent, FilterElement element)
            : this(parent)
        {
            Property = element.Property;
            MatchExpression = element.MatchExpression;
            Enabled = element.Enabled;
        }

        internal FilterElement GetElement()
        {
            FilterElement element = new FilterElement();
            element.Property = Property;
            element.MatchExpression = MatchExpression;
            element.Enabled = Enabled;
            return element;
        }

        internal void Serialize(BinaryWriter writer)
        {
            writer.Write(Property);
            writer.Write(MatchExpression);
            writer.Write(Enabled);
        }

        internal void Deserialize(BinaryReader reader)
        {
            Property = reader.ReadString();
            MatchExpression = reader.ReadString();
            Enabled = reader.ReadBoolean();
        }
    }

    internal class LocationData : List<FilterData>
    {
        internal bool Enabled = true;
        internal string Name = String.Empty;
        internal string Url = String.Empty;
        internal string MatchExpression = String.Empty;
        internal bool ShowFilters = false;

        private RedirectData _parent = null;

        internal RedirectData Parent
        {
            get { return _parent; }
        }

        internal LocationData(RedirectData parent)
        {
            _parent = parent;
        }

        internal LocationData(RedirectData parent, BinaryReader reader) 
            : this (parent)
        {
            Deserialize(reader);
        }

        internal LocationData(RedirectData parent, LocationElement element)
            : this (parent)
        {
            Enabled = element.Enabled;
            Name = element.Name;
            Url = element.Url;
            MatchExpression = element.MatchExpression;

            foreach (FilterElement item in element)
                base.Add(new FilterData(this, item));
        }

        internal LocationElement GetElement()
        {
            LocationElement element = new LocationElement();
            element.Enabled = Enabled;
            element.Name = Name;
            element.Url = Url;
            element.MatchExpression = MatchExpression;

            foreach (FilterData item in this)
                element.Add(item.GetElement());

            return element;
        }

        internal void Serialize(BinaryWriter writer)
        {
            writer.Write(Enabled);
            writer.Write(Name);
            writer.Write(Url);
            writer.Write(MatchExpression);
            writer.Write(ShowFilters);
            writer.Write(Count);
            foreach (FilterData item in this)
                item.Serialize(writer);
        }

        internal void Deserialize(BinaryReader reader)
        {
            Enabled = reader.ReadBoolean();
            Name = reader.ReadString();
            Url = reader.ReadString();
            MatchExpression = reader.ReadString();
            ShowFilters = reader.ReadBoolean();
            Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
                Add(new FilterData(this, reader));
        }
    }

    internal class RedirectData : List<LocationData>
    {
        internal bool Enabled = true;
        internal string DevicesFile = String.Empty;
        internal int Timeout = 20;
        internal bool FirstRequestOnly = true;
        internal bool OriginalUrlAsQueryString = false;
        internal string MobileHomePageUrl = String.Empty;
        internal string MobilePagesRegex = String.Empty;

        internal RedirectData(BinaryReader reader) 
        {
            Deserialize(reader);
        }

        internal RedirectData(RedirectSection section)
        {
            Enabled = section.Enabled;
            DevicesFile = section.DevicesFile;
            Timeout = section.Timeout;
            FirstRequestOnly = section.FirstRequestOnly;
            OriginalUrlAsQueryString = section.OriginalUrlAsQueryString;
            MobileHomePageUrl = section.MobileHomePageUrl;
            MobilePagesRegex = section.MobilePagesRegex;

            foreach (LocationElement element in section.Locations)
                base.Add(new LocationData(this, element));
        }

        internal RedirectSection GetElement()
        {
            System.Configuration.Configuration configuration = Support.GetConfigurationContainingSectionGroupName("fiftyOne/redirect");

            if (configuration == null)
                return null;

            RedirectSection element = configuration.GetSection("fiftyOne/redirect") as RedirectSection;

            if (element != null)
            {
                element.DevicesFile = DevicesFile;
                element.Timeout = Timeout;
                element.FirstRequestOnly = FirstRequestOnly;
                element.OriginalUrlAsQueryString = OriginalUrlAsQueryString;
                element.MobileHomePageUrl = MobileHomePageUrl;
                element.MobilePagesRegex = MobilePagesRegex;

                element.Locations.Clear();
                foreach (LocationData item in this)
                    element.Locations.Add(item.GetElement());
            }

            return element;
        }

        internal void Serialize(BinaryWriter writer)
        {
            writer.Write(DevicesFile);
            writer.Write(Timeout);
            writer.Write(FirstRequestOnly);
            writer.Write(OriginalUrlAsQueryString);
            writer.Write(MobileHomePageUrl);
            writer.Write(MobilePagesRegex);
            writer.Write(Count);
            foreach (LocationData item in this)
                item.Serialize(writer);
        }

        internal void Deserialize(BinaryReader reader)
        {
            DevicesFile = reader.ReadString();
            Timeout = reader.ReadInt32();
            FirstRequestOnly = reader.ReadBoolean();
            OriginalUrlAsQueryString = reader.ReadBoolean();
            MobileHomePageUrl = reader.ReadString();
            MobilePagesRegex = reader.ReadString();
            Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
                Add(new LocationData(this, reader));
        }
    }
}
