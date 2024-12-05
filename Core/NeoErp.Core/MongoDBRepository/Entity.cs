﻿using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Runtime.Serialization;

namespace NeoErp.Core.MongoDBRepository
{

    /// <summary>
    /// Abstract Entity for all the BusinessEntities.
    /// </summary>
    [DataContract]
        [Serializable]
        [BsonIgnoreExtraElements(Inherited = true)]
        public abstract class Entity : IEntity<string>
        {
            /// <summary>
            /// Gets or sets the id for this object (the primary record for an entity).
            /// </summary>
            /// <value>The id for this object (the primary record for an entity).</value>
            [DataMember]
            [BsonId]
            public virtual string Id { get; set; }
        }
    
}