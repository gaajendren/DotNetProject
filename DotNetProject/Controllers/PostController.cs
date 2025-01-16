using DotNetProject.Data;
using DotNetProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotNetProject.Controllers
{
    public class PostController : Controller
    {

        private readonly ApplicationDbContext dbContext;

        public PostController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public IActionResult CreatePost()
        {
            return View("~/Views/Post/Post.cshtml");  
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost(Post post, IFormFile image)
        {
            ModelState.Remove("UserID");
            ModelState.Remove("User");
            ModelState.Remove("ImagePath");

            if (ModelState.IsValid)
            {

                var userIdString = HttpContext.Session.GetString("UserID");

                if (int.TryParse(userIdString, out int userId)) 
                {
                    post.UserID = userId;
                }
                else
                {
                   
                    return RedirectToAction("Login", "User");
                }

                if (image != null && image.Length > 0)
                {
                    
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                    // Save the image to wwwroot/images folder
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                 
                    post.ImagePath = "/images/" + fileName;
                    

                    dbContext.Posts.Add(post);
                    await dbContext.SaveChangesAsync();

                    return RedirectToAction("Dashboard", "User");
                }
                else
                {
                    ModelState.AddModelError("", "Please upload an image.");
                    return View("Post", post);  
                }

              
            }

            return View("~/Views/Post/Post.cshtml", post); 
        }

        public IActionResult ListPosts()
        {
            var posts = dbContext.Posts
                         .Include(p => p.User) 
                         .ToList();

            return View("~/Views/Post/List_post.cshtml", posts);
        }
      

        // Get Edit Post
        public IActionResult Edit(int id)
        {
            var post = dbContext.Posts.FirstOrDefault(p => p.Id == id);
            if (post == null)
            {
                return NotFound();
            }
            return View(post);
        }

        // Post Edit Post
        [HttpPost]
        public async Task<IActionResult> Edit(Post updatedPost, IFormFile? newImage)
        {
            ModelState.Remove("UserID");
            ModelState.Remove("User");
            ModelState.Remove("ImagePath");

            var existingPost = dbContext.Posts.FirstOrDefault(p => p.Id == updatedPost.Id);
            if (existingPost == null)
            {
                return NotFound();
            }

            
            existingPost.Caption = updatedPost.Caption;

           
            if (newImage != null && newImage.Length > 0)
            {
              
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(newImage.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

               
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await newImage.CopyToAsync(stream);
                }

               
                if (!string.IsNullOrEmpty(existingPost.ImagePath))
                {
                    var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingPost.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

               
                existingPost.ImagePath = "/images/" + fileName;
            }

            await dbContext.SaveChangesAsync();
            return RedirectToAction("ListPosts");
        }

        // Delete Post
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var post = dbContext.Posts.FirstOrDefault(p => p.Id == id);
            if (post == null)
            {
                return NotFound();
            }

            // Remove the post from the database
            dbContext.Posts.Remove(post);
            dbContext.SaveChanges();

            return RedirectToAction("ListPosts");
        }



    }
}
