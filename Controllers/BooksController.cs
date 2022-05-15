using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EbookLibraryMongoDB.Services;
using MongoDbGenericRepository.Attributes;

namespace EbookLibraryMongoDB.Controllers
{
    [CollectionName("Books")]
    public class BooksController : Controller
    {
        private readonly BookService bookService;

        public BooksController(BookService bookService)
        {
            this.bookService = bookService;
        }
        
        /*I think this method is where I'm going to have to have a lot of validation stuff, like, "if identity matches booklist owner, then get the list" or something like that*/
        // GET: BooksController
        public ActionResult Index()
        {
            //return View();
            return View(bookService.Get());
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

        // POST: BooksController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
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
