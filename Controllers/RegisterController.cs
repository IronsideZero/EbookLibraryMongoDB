using Microsoft.AspNetCore.Mvc;
using EbookLibraryMongoDB.Models;
using EbookLibraryMongoDB.Services;
using Microsoft.Extensions.Configuration;
using System;

namespace EbookLibraryMongoDB.Controllers
{
    public class RegisterController : Controller
    {
        private IConfiguration config;
        private UserService userService;

        //I don't know where this is called, or how it knows to call this one instead of the implied no-arg constructor, but it does and it works. 
        public RegisterController(IConfiguration config)
        {
            this.config = config;
            this.userService = new UserService(config);
        }
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Method triggered when the user clicks the Create button on the Register page. Pulls in two form field values *NOTE: NO INPUT SANITIZATION AT THIS TIME*
        /// and attempts to locate an existing user with that email. If the email isn't already used, then a new user with that email is created and added to the 
        /// database. The request is then redirected to the login screen.
        /// </summary>
        /// <param name="UserEmail"></param>
        /// <param name="Password"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Register(string UserEmail, string Password)
        {
            //need to point the form to this method in the form's asp-action field

            if (ModelState.IsValid)
            {
                int alreadyExists = userService.GetExisting(UserEmail);
                if (alreadyExists == -1)
                {
                    TempData["Message"] = $"A user with that email already exists!";
                    return RedirectToAction(nameof(Index));
                } else
                {
                    userService.Create(UserEmail, Password);
                    TempData["Message"] = null;
                    //return RedirectToAction(nameof(Success));
                    //redirect to the "Index" method of the "Login" controller, in the global area (whole project, not in a specific 'Area')
                    return RedirectToAction("Index", "Login", new { area = "" });
                }
                
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Model is invalid");
                TempData["Message"] = $"The model wasn't valid!";
                return RedirectToAction(nameof(Index));
            }
            //string email = UserEmail;
            //string password = Password;
            //Console.WriteLine("Entered post method");
            //System.Diagnostics.Debug.WriteLine(UserEmail + " " + Password);
            //TempData["Message"] = $"Successfully Registered {email} with password {password}";
            ////return View("Success");
            //return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// This method can be used later to display post registration information regarding a confirmation email or text or whatever. 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Success()
        {
            //redirect to the "Index" method of the "Login" controller, in the global area (whole project, not in a specific 'Area')
            return RedirectToAction("Index", "Login", new { area = "" });
        }
    }
}
