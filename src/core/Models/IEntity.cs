﻿using System;

namespace core.Models
{
	public interface IEntity
	{        
        string Id { get; set; }
	}
	public class Entity: IEntity
	{
        [MongoDB.Bson.Serialization.Attributes.BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get; set; }
	}
}