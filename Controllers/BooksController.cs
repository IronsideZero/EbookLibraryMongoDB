using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EbookLibraryMongoDB.Services;
using MongoDbGenericRepository.Attributes;
using System;
using System.Collections.Generic;
using EbookLibraryMongoDB.Models;

namespace EbookLibraryMongoDB.Controllers
{
    [CollectionName("Books")]    
    public class BooksController : Controller
    {
        private readonly BookService bookService;
        private readonly DatabaseService db;

        public BooksController(BookService bookService, DatabaseService db)
        {
            this.bookService = bookService;  
            this.db = db;
        }
        
        // GET: BooksController
        public ActionResult Index()
        {
            //return View();
            //return View(bookService.Get());
            return RedirectToAction(nameof(Create));
        }

        // GET: BooksController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: BooksController/Create
        public ActionResult Create()
        {
            
            return View();
        }

        /// <summary>
        /// This method is called when the User adds a book to their personal library. First, all field values are collected. Fields that are not meant to be strings 
        /// are parsed out into their proper datatypes. Then the active user is retrieved using the email stored in the session (change to cookie and jwt later). Then 
        /// the field values are checked. ISBN, title, and author cannot be null or the form will fail and a message will be displayed. PubDate must be prior to the 
        /// current date, or it will be set to the current date. The value of the Owned checkbox is determined. Then the tags are separated. The page instructs the 
        /// user to separate individual tags by comma. Each word is then turned into a Tag object. Once all this is done, a new Book is created. If a book with a matching 
        /// isbn already exists, then the existing book is returned instead. That book is then added to the user's book list using the database service. Finally, the page 
        /// is returned to the user's index page, where their list of books, including the one they just added, is displayed. 
        /// </summary>
        /// <param name="collection">All entries made into the form whose submit triggers this method, organized in key-value pairs. Each value is a string.</param>
        /// <returns></returns>
        // POST: BooksController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            string isbn = collection["ISBN"];
            string title = collection["Title"];
            string author = collection["Author"];
            DateTime pubdate;
            bool dateValid = DateTime.TryParse(collection["PubDate"], out pubdate);
            string series = collection["Series"];
            int posInSeries;
            Int32.TryParse(collection["PosInSeries"], out posInSeries);
            string owned = collection["Owned"];//will be true, false if checked, just false otherwise
            bool ownedBool;
            double avgPrice;
            Double.TryParse((string)collection["AvgPrice"], out avgPrice);
            string location = collection["LocalFilePath"];
            int pages;
            Int32.TryParse(collection["Pages"], out pages);
            string language = collection["Language"];
            string description = collection["Description"];
            string tags = collection["Tags"];

            string activeUserName = HttpContext.Session.GetString("User");
            //User user = bookService.GetUser(activeUserName);
            User user = db.GetUser(activeUserName);

            if (String.IsNullOrEmpty(isbn) || String.IsNullOrEmpty(title) || String.IsNullOrEmpty(author))
            {
                TempData["Message"] = $"A book requires, at a minimum, an ISBN, a title, and an author.";
                return View();
            }

            if(dateValid)
            {
                if (pubdate >= DateTime.Today)
                {
                    pubdate = DateTime.Today;
                }
            } else
            {
                pubdate = DateTime.Today;
            }

            //parse out owned or not
            if (owned.Length > 5)
            {
                ownedBool = true;
            }
            else
            {
                ownedBool = false;
            }

            //parse out tags
            string[] individualTags = tags.Split(",", StringSplitOptions.RemoveEmptyEntries);

            List<BookTag> appliedTags = new List<BookTag>();
            foreach (string tag in individualTags)
            {
                //BookTag existingTag = bookService.GetTag(tag);
                BookTag existingTag = db.GetTag(tag.Trim());
                if (existingTag == null)
                {
                    BookTag newTag = new BookTag(tag.Trim());
                    bookService.Add(newTag);
                    appliedTags.Add(newTag);
                }
                else
                {
                    appliedTags.Add(existingTag);
                }

            }

            foreach (BookTag tag in appliedTags)
            {
                System.Diagnostics.Debug.WriteLine($"Tag is {tag.Name}");
            }


            bool alreadyExists;
            //Book book = bookService.CreateNew(isbn, title, author, pubdate, series, posInSeries, ownedBool, avgPrice, location, pages, language, appliedTags, description, out alreadyExists);
            Book book = db.CreateNewBook(isbn, title, author, pubdate, series, posInSeries, ownedBool, avgPrice, location, pages, language, appliedTags, description, out alreadyExists);
            //bookService.UpdateUserLibrary(user, book);
            db.UpdateAddBook(user, book);

            try
            {
                //return RedirectToAction(nameof(Index));
                return RedirectToAction("Index", "User", new { area = "" });
            }
            catch
            {
                return View();
            }
        }

        // GET: BooksController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: BooksController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: BooksController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: BooksController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
