using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using EbookLibraryMongoDB.Models;
using System;

namespace EbookLibraryMongoDB.Services
{
    /// <summary>
    /// This class provides database access to all controllers, rather than having a separate database access service for each collection. It is injected as a dependency 
    /// in Startup.cs with the line 'services.AddScoped<DatabaseService>();' in the ConfigureServices method. This allows any controller that needs it in its constructor 
    /// to find and use it. This class renders the classes BookService, UserService, and TagService obsolete. 
    /// </summary>
    public class DatabaseService
    {
        private readonly IMongoCollection<User> users;
        private readonly IMongoCollection<Book> books;
        private readonly IMongoCollection<BookTag> bookTags;

        public DatabaseService(IConfiguration config)
        {
            MongoClient client = new MongoClient(config.GetConnectionString("EbookLibMongo"));
            IMongoDatabase database = client.GetDatabase("EbookOrganizerDB");
            users = database.GetCollection<User>("Users");
            books = database.GetCollection<Book>("Books");
            bookTags = database.GetCollection<BookTag>("BookTags");
        }

        /*
         USER SERVICES
         */
        /// <summary>
        /// Method for logging a user in. Takes in email and password. Gets an existing user based on that email (no validation), and then takes 
        /// the entered plaintext password, the unvalidated user's stored salt, and the unvalidated user's stored encrypted password. It calls the 
        /// checkPAssword method of the PasswordService, which encrypts the plaintext password using the same salt, and returns true if they match, 
        /// and false otherwise. If it returns true, this method returns the now validated user. Otherwise it returns null. 
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public User GetUser(string email, string password)
        {
            User userToValidate = users.Find(user => user.UserEmail == email).Single();
            if (userToValidate != null)
            {
                bool validPass = PasswordService.checkPassword(password, userToValidate.Salt, userToValidate.Password);
                if (validPass)
                {
                    return userToValidate;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Method for getting a user based on an already validated user email stored in the session
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public User GetUser(string email)
        {
            User user = users.Find(user => user.UserEmail == email).FirstOrDefault();
            if (user != null)
            {
                return user;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// This method checks if a user exists in the database, based on email. Returns false if they do not exist, true if they do. 
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public bool UserExists(string email)
        {
            User userToValidate = users.Find<User>(user => user.UserEmail == email).FirstOrDefault();

            if (userToValidate != null)
            {
                //user was found
                return true;
            }
            else
            {
                //user doesn't exist
                return false;
            }
        }

        /// <summary>
        /// Method to create a new user. It takes an email and password (email has already been verified) and feeds the password to the 
        /// PasswordService, which generates a salt for the password and encrypts it, then returns the encrypted password and salt. These 
        /// are stored along with the email in the new user, which is then added to the database. The newly created user is then returned. 
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public User CreateUser(string email, string password)
        {
            string saltToStore;
            string encryptedPassword = PasswordService.encryptPassword(password, out saltToStore);
            User newUser = new User(email, encryptedPassword, saltToStore);
            users.InsertOne(newUser);
            return newUser;
        }

        /// <summary>
        /// Method to add a book to a user's library
        /// </summary>
        /// <param all book properties except Id primary key></param>
        public void UpdateAddBook(User user, string isbn, string title, string author, DateTime pubDate, string series, int posInSeries, bool owned, double avgPrice, string localFilePath, int pages, string language, List<Models.BookTag> tags, string description)
        {

            bool alreadyExists = false;
            Book newBook = CreateNewBook(isbn, title, author, pubDate, series, posInSeries, owned, avgPrice, localFilePath, pages, language, tags, description, out alreadyExists);
            if (alreadyExists)
            {

            }
            else
            {

            }
            user.Books.Add(newBook);
        }

        /// <summary>
        /// Method to add a book to a user's library, given a user and book already constructed. 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="book"></param>
        public void UpdateAddBook(User user, Book book)
        {
            //still need to check for duplicates
            user.Books.Add(book);
            var filter = Builders<User>.Filter.Eq("UserEmail", user.UserEmail);//set the filter so this only affects a document where the "UserEmail" field equals user.UserEmail
            var update = Builders<User>.Update.Set("Books", user.Books);//on the document the filter allows through, set the "Books" field to be user.Books
            var result = users.UpdateOne(filter, update);
        }

        /*
         BOOK SERVICES
         */
        public List<Book> GetBook()
        {
            return books.Find(book => true).ToList();
        }

        /* Get operations for books, right now just by Id, will have to add search by all properties
         */
        public Book GetBook(string id)
        {
            return books.Find(book => book.Id == id).FirstOrDefault();
        }

        public Book GetBookByISBN(string ISBN)
        {
            return books.Find(book => book.ISBN == ISBN).FirstOrDefault();
        }

        public Book AddBook(Book book)
        {
            books.InsertOne(book);
            return book;
        }

        public void UpdateBook(string id, Book bookIn)
        {
            books.ReplaceOne(book => book.Id == id, bookIn);
        }

        public void RemoveBook(Book bookIn)
        {
            books.DeleteOne(book => book.Id == bookIn.Id);
        }

        public void RemoveBook(string id)
        {
            books.DeleteOne(book => book.Id == id);
        }

        /// <summary>
        /// This method is called when the user creates a new book (or tries to). All fields are nullable except isbn, title, and author. If any of those are null, this method returns null. Otherwise, 
        /// a check is done to see if there is a matching isbn already existing. If so, that book is returned, and the out parameter is set to true to facilitate an alert to the user. Otherwise, a new 
        /// book is created, then added to the list of books. Then the out parameter is set to false, and the new book is returned.
        /// </summary>
        /// <param everything except id which is generated by mongo></param>
        /// <returns>Book, either the new book or the book that already exists</returns>
        public Book CreateNewBook(string isbn, string title, string author, DateTime pubDate, string series, int posInSeries, bool owned, double avgPrice, string localFilePath, int pages, string language, List<BookTag> tags, string description, out bool alreadyExists)
        {
            if (isbn == null || title == null || author == null)
            {
                alreadyExists = false;
                return null;
            }

            Book existingBook = GetBookByISBN(isbn);
            if (existingBook == null)
            {
                Book newBook = new Book(isbn, title, author, pubDate, series, posInSeries, owned, avgPrice, localFilePath, pages, language, tags, description);
                AddBook(newBook);
                alreadyExists = false;
                return newBook;
            }
            else
            {
                //put some stuff in here to add more tags to an already existing book if it is 'added' again

                alreadyExists = true;
                return existingBook;
            }
        }

        /*
         TAG SERVICES
         */
        public List<BookTag> GetTagList()
        {
            return bookTags.Find(BookTag => true).ToList();
        }

        /* Get operations for books, right now just by Id, will have to add search by all properties
         */
        public BookTag GetTag(string name)
        {
            return bookTags.Find(tag => tag.Name == name).FirstOrDefault();
        }

        public BookTag AddTag(BookTag tag)
        {
            bookTags.InsertOne(tag);
            return tag;
        }



        public void RemoveTag(BookTag tagToRemove)
        {
            bookTags.DeleteOne(tag => tag.Name == tagToRemove.Name);
        }
    }
}
