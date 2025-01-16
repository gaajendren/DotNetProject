using Microsoft.AspNetCore.Mvc;
using DotNetProject.Models;
using System.Linq;
using System;
using DotNetProject.Data;
using Microsoft.EntityFrameworkCore;

namespace DotNetProject.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext dbContext;
     
        public UserController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public IActionResult Register()
        {
            return View("~/Views/Auth/register.cshtml");
        }


        [HttpPost]
        public IActionResult Register(string name, string email, string phone, string password, string confirmPassword)
        {

            if (password != confirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match.");
                return View();
            }

            if (ModelState.IsValid)
            {
                var existingUser = dbContext.Users.FirstOrDefault(u => u.Email == email);
                if (existingUser == null)
                {
                    var newUser = new User
                    {
                        Name = name,
                        Email = email,
                        Phone = phone,
                        Password = password 
                    };

                    dbContext.Users.Add(newUser);
                    dbContext.SaveChanges();
                    return RedirectToAction("Login");
                }
                ModelState.AddModelError("", "Email already registered.");
            }
            return View();
        }

        // Login Page
        public IActionResult Login()
        {
            return View("~/Views/Auth/login.cshtml");
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = dbContext.Users.FirstOrDefault(u => u.Email == email && u.Password == password);
            if (user != null)
            {
                // Simulate a session (for simplicity)
             
                HttpContext.Session.SetString("UserName", user.Name.ToString());
                HttpContext.Session.SetString("UserEmail", user.Email.ToString());
                HttpContext.Session.SetString("UserID", user.Id.ToString());
                HttpContext.Session.SetString("Phone", user.Phone.ToString());
                

               
                return RedirectToAction("Dashboard");
            }

            ModelState.AddModelError("", "Invalid email or password.");
            return View("~/Views/Auth/login.cshtml");
        }

        public IActionResult Dashboard()
        {
            var id = HttpContext.Session.GetString("UserID");

            if (HttpContext.Session.GetString("UserName") != null)
            {
                ViewBag.UserName = HttpContext.Session.GetString("UserName");

                if (id == null)
                {
                    return RedirectToAction("Login", "User");
                }

                int currentUserIdInt = Convert.ToInt32(id);

                // Get the list of users that the current user is following
                var followedUserIds = dbContext.Follows
                    .Where(f => f.FollowerUserID == currentUserIdInt)
                    .Select(f => f.FollowingUserID)
                    .ToList();

                // Get all posts of the followed users
                var posts = dbContext.Posts
                    .Where(p => followedUserIds.Contains(p.UserID))
                    .Include(p => p.User)
                    .OrderBy(x => Guid.NewGuid()) 
                    .ToList();


                return View("~/Views/dashboard.cshtml", posts);
            }

          

            return RedirectToAction("Login");
        }


        public IActionResult Logout()
        {
            TempData.Clear(); 
            return RedirectToAction("Login");
        }


        public IActionResult Profile()
        {
           
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login");
            }

            var user = dbContext.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            return View("~/Views/profile.cshtml", user);
        }

        
        [HttpPost]
        public IActionResult EditProfile(User updatedUser)
        {
            ModelState.Remove("Password");
            ModelState.Remove("SentMessages");
            ModelState.Remove("ReceivedMessages");
          
            ModelState.Remove("Posts");

            if (ModelState.IsValid)
            {
                var user = dbContext.Users.FirstOrDefault(u => u.Email == updatedUser.Email);
                if (user != null)
                {
                    user.Name = updatedUser.Name;
                    user.Email = updatedUser.Email;
                    user.Phone = updatedUser.Phone;
                  

                    dbContext.SaveChanges();

                    HttpContext.Session.SetString("UserName", user.Name.ToString());
                    HttpContext.Session.SetString("UserEmail", user.Email.ToString());
                    HttpContext.Session.SetString("Phone", user.Phone.ToString());


                    return RedirectToAction("Dashboard");
                }
            }

            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(error.ErrorMessage); 
                }
            }

            return View("~/Views/profile.cshtml", updatedUser);  
        }

    }
}
