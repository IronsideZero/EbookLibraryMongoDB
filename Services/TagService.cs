using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using EbookLibraryMongoDB.Models;
using System;

namespace EbookLibraryMongoDB.Services
{
    /// <summary>
    /// This class provides all the basic CRUD operations on a book
    /// </summary>
    public class TagService
    {
        private readonly IMongoCollection<BookTag> tags;

        public TagService(IConfiguration config)
        {
            MongoClient client = new MongoClient(config.GetConnectionString("EbookLibMongo"));
            IMongoDatabase database = client.GetDatabase("EbookOrganizerDB");
            tags = database.GetCollection<BookTag>("Tags");
        }       

        public List<BookTag> Get()
        {
            return tags.Find(BookTag => true).ToList();
        }

        /* Get operations for books, right now just by Id, will have to add search by all properties
         */
        public BookTag Get(string name)
        {
            return tags.Find(tag => tag.Name == name).FirstOrDefault();
        }      

        public BookTag Add(BookTag tag)
        {
            tags.InsertOne(tag);
            return tag;
        }

        

        public void Remove(BookTag tagToRemove)
        {
            tags.DeleteOne(tag => tag.Name == tagToRemove.Name);
        }

       
    }
}

