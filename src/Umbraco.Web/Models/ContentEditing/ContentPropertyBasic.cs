﻿using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// Represents a content property to be saved
    /// </summary>
    [DataContract(Name = "property", Namespace = "")]
    public class ContentPropertyBasic
    {
        [DataMember(Name = "id", IsRequired = true)]
        [Required]
        public int Id { get; set; }

        [DataMember(Name = "value")]
        public string Value { get; set; }

        [DataMember(Name = "alias", IsRequired = true)]
        [Required(AllowEmptyStrings = false)]
        public string Alias { get; set; }

        protected bool Equals(ContentPropertyBasic other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as ContentPropertyBasic;
            return other != null && Equals(other);
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
}