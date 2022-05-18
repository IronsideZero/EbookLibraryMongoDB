using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EbookLibraryMongoDB.Services;
using EbookLibraryMongoDB.Models;


namespace EbookLibraryMongoDB.Controllers
{
    public class UserController : Controller
    {
        private readonly UserService userService;
        private readonly DatabaseService db;

        public UserController(UserService userService, DatabaseService db)
        {
            this.userService = userService;
            this.db = db;
        }

        // GET: UserController
        public ActionResult Index()
        {
            //will need to check the user that just logged in here. Cookie with jwt?
            string sessionOwner = HttpContext.Session.GetString("User");
            //User user = userService.Get(sessionOwner);
            User user = db.GetUser(sessionOwner);
            if (user != null)
            {
                return View(user);
            } else
            {
                return RedirectToAction("Index", "Login", new { area = "" });
            }
        }

        // GET: UserController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: UserController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: UserController/Create
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

        // GET: UserController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: UserController/Edit/5
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

        // GET: UserController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: UserController/Delete/5
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

        /// <summary>
        /// This method simply redirects from the User controller to the Books controller, and passes along the isbn of the book whose Edit button was clicked 
        /// in the TempData
        /// </summary>
        /// <param name="isbn">Taken from the routeValues property of the Html.ActionLink method on the Index page</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult EditBook(string isbn)
        {
            TempData["isbn"] = isbn;
            return RedirectToAction("Edit", "Books", new { area = "" });            
        }
    }
}
