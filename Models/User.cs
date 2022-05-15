using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;
using MongoDbGenericRepository.Attributes;
//using AspNetCore.Identity.MongoDbCore.Models;

namespace EbookLibraryMongoDB.Models
{
    [CollectionName("Users")]
    public class User 
    {

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }//mongo driver will automatically create an id?

        [BsonElement("UserEmail")]
        [EmailAddress]
        [Required(ErrorMessage = "This user must have an email")]
        public string UserEmail { get; set; }

        [BsonElement("Password")]
        [Required(ErrorMessage = "User must have a password")]
        public string Password { get; set; }

        [BsonElement("Salt")]
        public string Salt { get; set; }

        [BsonElement("Books")]
        public List<Book> Books { get; set; }

        public User(string email, string password, string salt)
        {
            this.UserEmail = email;
            this.Password = password;
            this.Salt = salt;
            this.Books = new List<Book>();
        }

        public User()
        {

        }
    }
}
