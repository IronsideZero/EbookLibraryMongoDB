using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;

namespace EbookLibraryMongoDB.Models
{
    public class Book
    {
        //Properties of a Book object
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("ISBN")]
        public string ISBN { get; set; }
        
        [BsonElement("Title")]
        [Required(ErrorMessage = "This book has to have a name.")]
        public string Title { get; set; }
        
        [BsonElement("Author")]
        [Required(ErrorMessage = "Someone wrote this book. Find out who.")]
        public string Author { get; set; }
        
        [BsonElement("PublishDate")]
        public DateTime PubDate { get; set; }
        
        [BsonElement("Series")]
        public string Series { get; set; }
        
        [BsonElement("PositionInSeries")]
        public int PosInSeries { get; set; }
        
        [BsonElement("Owned")]
        public bool Owned { get; set; }
        
        [BsonElement("AveragePrice")]
        public double AvgPrice { get; set; }
        
        //LocalFilePath will only be populated if Owned is true. Not sure if it's possible or how to actually tie it to the Owned property
        [BsonElement("Location")]
        public string LocalFilePath { get; set; }
        
        [BsonElement("Pages")]
        public int Pages { get; set; }
        
        [BsonElement("Language")]
        public string Language { get; set; }
        
        [BsonElement("Tags")]
        public List<Tag> Tags { get; set; }
        
        [BsonElement("Description")]
        public string Description { get; set; }

        public Book()
        {

        }
        
        public Book(string isbn, string title, string author, DateTime pubDate, string series, int posInSeries, bool owned, double avgPrice, string localFilePath, int pages, string language, List<Tag> tags, string description)
        {
            ISBN = isbn;
            Title = title;
            Author = author;
            PubDate = pubDate;
            Series = series;
            PosInSeries = posInSeries;
            Owned = owned;
            AvgPrice = avgPrice;
            LocalFilePath = localFilePath;
            Pages = pages;
            Language = language;
            Tags = tags;
            Description = description;
        }
    }
}
