﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ReadVideo.Server.Data
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Email { get; set; }
        public string Fullname { get; set; }
    }

}
