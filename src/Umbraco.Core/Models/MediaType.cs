using System;
using System.Runtime.Serialization;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents the content type that a <see cref="Media"/> object is based on
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class MediaType : ContentTypeCompositionBase, IMediaType
    {
        public MediaType(int parentId) : base(parentId)
        {
        }

        /// <summary>
        /// Method to call when Entity is being saved
        /// </summary>
        /// <remarks>Created date is set and a Unique key is assigned</remarks>
        internal override void AddingEntity()
        {
            base.AddingEntity();
            Key = Guid.NewGuid();
        }

        /// <summary>
        /// Method to call when Entity is being updated
        /// </summary>
        /// <remarks>Modified Date is set and a new Version guid is set</remarks>
        internal override void UpdatingEntity()
        {
            base.UpdatingEntity();
        }
    }
}