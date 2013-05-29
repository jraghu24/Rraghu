﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Core.Models.Editors
{
    /// <summary>
    /// Represents data that has been submitted to be saved for a content property
    /// </summary>
    /// <remarks>
    /// This object exists because we may need to save additional data for each property, more than just
    /// the string representation of the value being submitted. An example of this is uploaded files.
    /// </remarks>
    public class ContentPropertyData
    {
        public ContentPropertyData(string value, IDictionary<string, object> additionalData)
        {
            Value = value;
            AdditionalData = new ReadOnlyDictionary<string, object>(additionalData);
        }

        /// <summary>
        /// The string value submitted for the property
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// A dictionary containing any additional objects that are related to this property when saving
        /// </summary>
        public ReadOnlyDictionary<string, object> AdditionalData { get; private set; }
        
    }
}
