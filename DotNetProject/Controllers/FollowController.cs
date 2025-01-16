using DotNetProject.Data;
using DotNetProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotNetProject.Controllers
{
    public class FollowController : Controller
    {
        private readonly ApplicationDbContext dbContext;

        public FollowController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

       

        // Search users and display them in a list for follow action
        public IActionResult FollowUser(string searchTerm)
        {
            var currentUserId = HttpContext.Session.GetString("UserID");
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "User");
            }

            int currentUserIdInt = Convert.ToInt32(currentUserId);

            // Get a list of users that the current user is following
            var followedUsers = dbContext.Follows
                .Where(f => f.FollowerUserID == currentUserIdInt)
                .Select(f => f.FollowingUserID)
                .ToList();

            var usersQuery = dbContext.Users.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                usersQuery = usersQuery.Where(u => u.Name.Contains(searchTerm) && u.Id != currentUserIdInt);
            }
            else
            {
                usersQuery = usersQuery.Where(u => u.Id != currentUserIdInt); // Exclude the current user
            }

            var users = usersQuery.ToList();

            var viewModel = new FollowViewModel
            {
                SearchTerm = searchTerm,
                Users = users,
                FollowedUsers = followedUsers 
            };

            return View("~/Views/follow.cshtml", viewModel);
        }

     


        [HttpPost]
        public async Task<IActionResult> Follow(int userId)
        {
            var currentUserId = HttpContext.Session.GetString("UserID");
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "User");
            }

            int currentUserIdInt = Convert.ToInt32(currentUserId);

            // Check if the user is already following
            var existingFollow = await dbContext.Follows
                .FirstOrDefaultAsync(f => f.FollowerUserID == currentUserIdInt && f.FollowingUserID == userId);

            if (existingFollow == null)
            {
                var follow = new Follow
                {
                    FollowerUserID = currentUserIdInt,
                    FollowingUserID = userId
                };

                dbContext.Follows.Add(follow);
                await dbContext.SaveChangesAsync();
            }

            return RedirectToAction("FollowUser");
        }

        public IActionResult FollowedUsers()
        {
            var currentUserId = HttpContext.Session.GetString("UserID");
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "User");
            }

            int currentUserIdInt = Convert.ToInt32(currentUserId);

            // Get the list of users that the current user is following
            var followedUsers = dbContext.Follows
                .Where(f => f.FollowerUserID == currentUserIdInt)
                .Select(f => f.FollowingUserID)
                .ToList();

            var users = dbContext.Users
                .Where(u => followedUsers.Contains(u.Id))
                .ToList();

            return View("~/Views/FollowedUsers.cshtml",users);
        }


        [HttpPost]
        public async Task<IActionResult> Unfollow(int userId)
        {
            var currentUserId = HttpContext.Session.GetString("UserID");
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "User");
            }

            int currentUserIdInt = Convert.ToInt32(currentUserId);

            // Find the follow entry and remove it
            var existingFollow = await dbContext.Follows
                .FirstOrDefaultAsync(f => f.FollowerUserID == currentUserIdInt && f.FollowingUserID == userId);

            if (existingFollow != null)
            {
                dbContext.Follows.Remove(existingFollow);
                await dbContext.SaveChangesAsync();
            }

            return RedirectToAction("FollowUser");
        }

    }
}
