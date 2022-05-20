using Microsoft.AspNetCore.Mvc;
using EbookLibraryMongoDB.Models;
using EbookLibraryMongoDB.Services;
using Microsoft.Extensions.Configuration;
using System;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http;

namespace EbookLibraryMongoDB.Controllers
{
    public class LoginController : Controller
    {
        private IConfiguration config;
        private UserService userService;
        private readonly DatabaseService db;

        public LoginController(IConfiguration config, DatabaseService db)
        {
            this.config = config;
            this.userService = new UserService(config);
            this.db = db;
            //var session = HttpContext.Session;
        }

        public IActionResult Index()
        {
            return View("LoginIndex");
        }

        /// <summary>
        /// This method is triggered when the user clicks Login after entering their credentials. First it checks to see that a user with 
        /// that email exists at all. Then it calls the Get method of UserService, which does the password validity check. A user is returned 
        /// if the password is valid, and the request is redirected to the user's index page. If the password is invalid, the request returns 
        /// to the login page with an error message. 
        /// </summary>
        /// <param name="UserEmail"></param>
        /// <param name="Password"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Login(string UserEmail, string Password)
        {
            System.Diagnostics.Debug.WriteLine("Entered login method");
            bool result = db.UserExists(UserEmail);
            System.Diagnostics.Debug.WriteLine($"Result is {result}");
            if (db.UserExists(UserEmail))
            {
                System.Diagnostics.Debug.WriteLine($"Found a user with email {UserEmail}");
                User user = db.GetUser(UserEmail, Password);
                if(user == null)
                {
                    System.Diagnostics.Debug.WriteLine($"email or pass incorrect");
                    TempData["Message"] = $"Email or password was incorrect";
                    return RedirectToAction(nameof(Index));
                } else
                {
                    System.Diagnostics.Debug.WriteLine($"Found user, should have worked");
                    HttpContext.Session.SetString("User", user.UserEmail);//set a session variable called User to be user.UserEmail
                    //redirect to the "Index" method of the "User" controller, in the global area (whole project, not in a specific 'Area')
                    return RedirectToAction("Index", "User", new { area = "" });
                }
            } else
            {
                System.Diagnostics.Debug.WriteLine($"Didn't find user {UserEmail}");
                TempData["Message"] = $"There is no user registered with that email";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
