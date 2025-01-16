using DotNetProject.Models;
using DotNetProject.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetProject.Controllers
{
    public class MessageController : Controller
    {
        private readonly ApplicationDbContext dbContext;

        public MessageController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        // GET: Message page where the user selects a followed user to send a message
        public IActionResult SendMessage()
        {
            ModelState.Remove("FollowedUsers");
            var userId = HttpContext.Session.GetString("UserID");
            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }

            int currentUserId = Convert.ToInt32(userId);

            // Get the list of followed users for the current user
            var followedUserIds = dbContext.Follows
                .Where(f => f.FollowerUserID == currentUserId)
                .Select(f => f.FollowingUserID)
                .ToList();

            var followedUsers = dbContext.Users
                .Where(u => followedUserIds.Contains(u.Id))
                .ToList();

            var viewModel = new MessageViewModel
            {
                FollowedUsers = followedUsers
            };

            return View("~/Views/message.cshtml", viewModel);
        }

        // POST: Handle sending the message
        [HttpPost]
        public async Task<IActionResult> SendMessage(MessageViewModel model)
        {
            ModelState.Remove("FollowedUsers");
            var userId = HttpContext.Session.GetString("UserID");
            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }

            int currentUserId = Convert.ToInt32(userId);

            if (ModelState.IsValid)
            {
                try
                {
                  
                    var message = new Message
                    {
                        SenderUserID = currentUserId,
                        ReceiverUserID = model.FollowedUserId,
                        Content = model.Message,
                        SentAt = DateTime.Now
                    };

                   
                    dbContext.Messages.Add(message);
                    await dbContext.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Message sent successfully!";
                    return RedirectToAction("ListMessages");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error sending message: {ex.Message}";
                }
            }

          
            var followedUserIds = dbContext.Follows
                .Where(f => f.FollowerUserID == currentUserId)
                .Select(f => f.FollowingUserID)
                .ToList();

            var followedUsers = dbContext.Users
                .Where(u => followedUserIds.Contains(u.Id))
                .ToList();

            model.FollowedUsers = followedUsers;
            return View("~/Views/message.cshtml", model);
        }

        public IActionResult ListMessages()
        {
            var userId = HttpContext.Session.GetString("UserID");
            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }

            int currentUserId = Convert.ToInt32(userId);

            // Fetch messages sent by the current user
            var messages = dbContext.Messages
                .Where(m => m.SenderUserID == currentUserId)
                .Include(m => m.Receiver) // Include Receiver details for display
                .ToList();

            return View("~/Views/ListMessages.cshtml", messages);
        }

        public IActionResult EditMessage(int id)
        {
            ModelState.Remove("FollowedUsers");
            ModelState.Remove("Sender");
            ModelState.Remove("Receiver");
            var userId = HttpContext.Session.GetString("UserID");
            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }

            int currentUserId = Convert.ToInt32(userId);

            // Get the message by ID and ensure it belongs to the current user
            var message = dbContext.Messages
                .FirstOrDefault(m => m.Id == id && m.SenderUserID == currentUserId);

            if (message == null)
            {
                return NotFound(); // Return 404 if message not found or doesn't belong to the user
            }

            return View("~/Views/EditMessage.cshtml", message);
        }


        [HttpPost]
        public async Task<IActionResult> EditMessage(Message updatedMessage)
        {
            ModelState.Remove("FollowedUsers");
            ModelState.Remove("Sender");
            ModelState.Remove("Receiver");
            if (ModelState.IsValid)
            {
                var existingMessage = dbContext.Messages
                    .FirstOrDefault(m => m.Id == updatedMessage.Id);

                if (existingMessage != null)
                {
                    existingMessage.Content = updatedMessage.Content;
                    dbContext.Messages.Update(existingMessage);
                    await dbContext.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Message updated successfully!";
                    return RedirectToAction("ListMessages");
                }
            }

            TempData["ErrorMessage"] = "Failed to update the message.";
            return View("~/Views/EditMessage.cshtml", updatedMessage);
        }
        public IActionResult DeleteMessage(int id)
        {
            var userId = HttpContext.Session.GetString("UserID");
            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }

            int currentUserId = Convert.ToInt32(userId);

            var message = dbContext.Messages
                .FirstOrDefault(m => m.Id == id && m.SenderUserID == currentUserId);

            if (message == null)
            {
                return NotFound();
            }

            return View("~/Views/DeleteMessage.cshtml", message);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMessageConfirmed(int id)
        {
            var message = dbContext.Messages.FirstOrDefault(m => m.Id == id);
            if (message != null)
            {
                dbContext.Messages.Remove(message);
                await dbContext.SaveChangesAsync();

                TempData["SuccessMessage"] = "Message deleted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete the message.";
            }

            return RedirectToAction("ListMessages");
        }

        public IActionResult ReceivedMessages()
        {
            var userId = HttpContext.Session.GetString("UserID");
            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }

            int currentUserId = Convert.ToInt32(userId);

            // Fetch messages received by the current user
            var messages = dbContext.Messages
                .Where(m => m.ReceiverUserID == currentUserId)
                .Include(m => m.Sender) // Include Sender details for display
                .ToList();

            return View("~/Views/ReceivedMessages.cshtml", messages);
        }


    }
}
