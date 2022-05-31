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
                    db.AddTag(newTag);
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


            //bool alreadyExists;
            //Book book = bookService.CreateNew(isbn, title, author, pubdate, series, posInSeries, ownedBool, avgPrice, location, pages, language, appliedTags, description, out alreadyExists);
            //Book book = db.CreateNewBook(isbn, title, author, pubdate, series, posInSeries, ownedBool, avgPrice, location, pages, language, appliedTags, description, out alreadyExists);
            Book book = db.CreateNewBookNoDb(isbn, title, author, pubdate, series, posInSeries, ownedBool, avgPrice, location, pages, language, appliedTags, description);
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

        /// <summary>
        /// GET method for editing a book. Triggered when on the User Index page, the User selects the Edit link on a book's record
        /// </summary>
        /// <returns></returns>
        // GET: BooksController/Edit/5
        public ActionResult Edit()
        {
            string isbn = (string)TempData["isbn"];
            TempData["isbn"] = isbn;
            Book book = db.GetBookByISBN(isbn);

            User owner = db.GetUser(HttpContext.Session.GetString("User"));
            Book bookToEdit = owner.Books.Find(book => book.ISBN == isbn);
            if(bookToEdit != null)
            {                
                return View(bookToEdit);
            } else
            {
                TempData["ErrorMessage"] = "That book could not be found in your library. You may only edit details of a book in your library. Please check the ISBN, or add the book to your library before editing.";
                return RedirectToAction("Index", "User", new { area = "" });
            }
            
        }

        /// <summary>
        /// POST method triggered when the User completes the form on the Book edit page. The ISBN is readonly in the html, but since this can be altered, the book's original ISBN is carried over from the 
        /// User's Index page through TempData. Someone really determined to change the ISBN could still do this, but they wouldn't really hurt anything. The rest of the data is collected from the form. The 
        /// form is pre-filled with the existing data, so the fields should only be altered if the User deliberately goes and alters them. The ISBN is checked almost immediately, and the User is returned to 
        /// the edit page with a warning message if they don't match. Then the same operations are completed to convert the form's string data to other formats as needed, as is done in the Create() method. 
        /// The original book is then found in the owner's Book list, and that book's data is altered to match the new form fields before being replaced in the list. At present, the page is then redirected 
        /// to the User's list of books, however later it will be changed to redirect to the edited book's details page. 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="collection">All entries made into the form whose submit triggers this method, organized in key-value pairs. Each value is a string.</param>
        /// <returns></returns>
        // POST: BooksController/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            //prep variables for the data
            string isbnItShouldBe = (string)TempData["isbn"];
            System.Diagnostics.Debug.WriteLine($"isbn it should be is {isbnItShouldBe}");
            string isbn;
            string title;
            string author;
            DateTime pubdate;           
            string series;
            int posInSeries;            
            string owned;//will be true, false if checked, just false otherwise
            bool ownedBool;
            double avgPrice;            
            string location;
            int pages;            
            string language;
            string description;
            string tags;
            List<BookTag> appliedTags = new List<BookTag>();
            User owner;

            //get the data
            try
            {
                owner = db.GetUser(HttpContext.Session.GetString("User"));
                isbn = collection["ISBN"];
                System.Diagnostics.Debug.WriteLine($"isbn it is: {isbn}");
                //if they try to change the isbn, reload the page with a warning message
                if (!isbn.Equals(isbnItShouldBe))
                {
                    TempData["ErrorMessage"] = $"You can't change the isbn of a book. If the isbn is wrong, enter the correction as a new book and then delete this one.";
                    return RedirectToAction(nameof(Edit));
                }
                title = collection["Title"];
                author = collection["Author"];                
                bool dateValid = DateTime.TryParse(collection["PubDate"], out pubdate);
                series = collection["Series"];                
                Int32.TryParse(collection["PosInSeries"], out posInSeries);
                owned = collection["Owned"];//will be true, false if checked, just false otherwis                
                Double.TryParse((string)collection["AvgPrice"], out avgPrice);
                location = collection["LocalFilePath"];                
                Int32.TryParse(collection["Pages"], out pages);
                language = collection["Language"];
                description = collection["Description"];
                tags = collection["Tags"];

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
            } catch
            {
                TempData["ErrorMessage"] = $"One of those fields was wrong somehow in the Edit POST method.";
                return RedirectToAction(nameof(Edit));
            }

            //if the data is good, apply the data
            Book existingBook = owner.Books.Find(book => book.ISBN == isbn);            
            if(existingBook == null)
            {
                TempData["ErrorMessage"] = $"That book could not be found in your library. Check the isbn and try again.";
                return RedirectToAction(nameof(Edit));
            }
            int indexOfExistingBook = owner.Books.FindIndex(book => book.ISBN == isbn);
            existingBook.Title = title;
            existingBook.Author = author;
            existingBook.PubDate = pubdate;
            existingBook.Series = series;
            existingBook.PosInSeries = posInSeries;
            existingBook.Owned = ownedBool;
            existingBook.AvgPrice = avgPrice;
            existingBook.LocalFilePath = location;
            existingBook.Pages = pages;
            existingBook.Language = language;
            existingBook.Description = description;
            existingBook.Tags = appliedTags;
        
            owner.Books[indexOfExistingBook] = existingBook;

            try
            {                
                db.UpdateUserEditedBook(owner);
                //return RedirectToAction("Index", "User", new { area = "" });//change this to go to the edited book's details page later
                return RedirectToAction("ShowDetails", "Books", new { area = "", isbn = isbn});
            } catch
            {
                TempData["ErrorMessage"] = $"Something went wrong while attempting to update your library.";
                return RedirectToAction(nameof(Edit));
            }
        }

        //GET:
        public ActionResult ShowDetails(string isbn)
        {
            System.Diagnostics.Debug.WriteLine($"isbn is: {isbn}");
            User owner = db.GetUser(HttpContext.Session.GetString("User"));
            Book bookToDisplay = owner.Books.Find(book => book.ISBN == isbn);
            if(bookToDisplay != null)
            {
                return View(bookToDisplay);
            } else
            {
                TempData["ErrorMessage"] = "That book could not be found in your library. You may only edit details of a book in your library. Please check the ISBN, or add the book to your library before editing.";
                return RedirectToAction("Index", "User", new { area = "" });
            }            
        }

        //GET:
        public ActionResult ReturnToIndex()
        {
            return RedirectToAction("Index", "User", new { area = "" });
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
