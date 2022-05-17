using MongoDB.Driver;
using EbookLibraryMongoDB.Models;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System;
using MongoDB.Bson;

namespace EbookLibraryMongoDB.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> users;
        private BookService bookService;

        public UserService(IConfiguration config)
        {
            MongoClient client = new MongoClient(config.GetConnectionString("EbookLibMongo"));
            IMongoDatabase database = client.GetDatabase("EbookOrganizerDB");
            users = database.GetCollection<User>("Users");
            //bookService = new BookService(config);
        }

        /// <summary>
        /// Method for logging a user in. Takes in email and password. Gets an existing user based on that email (no validation), and then takes 
        /// the entered plaintext password, the unvalidated user's stored salt, and the unvalidated user's stored encrypted password. It calls the 
        /// checkPAssword method of the PasswordService, which encrypts the plaintext password using the same salt, and returns true if they match, 
        /// and false otherwise. If it returns true, this method returns the now validated user. Otherwise it returns null. 
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public User Get(string email, string password)
        {
            User userToValidate = users.Find(user => user.UserEmail == email).Single();
            if(userToValidate != null)
            {
                bool validPass = PasswordService.checkPassword(password, userToValidate.Salt, userToValidate.Password);
                if(validPass)
                {
                    return userToValidate;
                } else
                {
                    return null;
                }
            } else
            {
                return null;
            }
        }

        /// <summary>
        /// Method for getting a user based on an already validated user email stored in the session
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public User Get(string email)
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
            
            if( userToValidate != null )
            {
                //user was found
                return true;
            } else
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
        public User Create(string email, string password)
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
            Book newBook = bookService.CreateNew(isbn, title, author, pubDate, series, posInSeries, owned, avgPrice, localFilePath, pages, language, tags, description, out alreadyExists);
            if(alreadyExists)
            {

            } else
            {

            }
            user.Books.Add(newBook);
        }

        public void UpdateAddBook(User user, Book book)
        {
            user.Books.Add(book);
            var filter = Builders<User>.Filter.Eq("UserEmail", user.UserEmail);//set the filter so this only affects a document where the "UserEmail" field equals user.UserEmail
            var update = Builders<User>.Update.Set("Books", user.Books);//on the document the filter allows through, set the "Books" field to be user.Books
            var result = users.UpdateOne(filter, update);
        }
    }
}
