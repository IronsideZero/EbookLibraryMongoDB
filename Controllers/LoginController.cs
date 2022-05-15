using Microsoft.AspNetCore.Mvc;
using EbookLibraryMongoDB.Models;
using EbookLibraryMongoDB.Services;
using Microsoft.Extensions.Configuration;
using System;

namespace EbookLibraryMongoDB.Controllers
{
    public class LoginController : Controller
    {
        private IConfiguration config;
        private UserService userService;

        public LoginController(IConfiguration config)
        {
            this.config = config;
            this.userService = new UserService(config);
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
            if(userService.GetExisting(UserEmail) == 1)
            {
                User user = userService.Get(UserEmail, Password);
                if(user == null)
                {
                    TempData["Message"] = $"Email or password was incorrect";
                    return RedirectToAction(nameof(Index));
                } else
                {
                    //redirect to the "Index" method of the "Login" controller, in the global area (whole project, not in a specific 'Area')
                    return RedirectToAction("Index", "User", new { area = "" });
                }
            } else
            {
                TempData["Message"] = $"There is no user registered with that email";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
