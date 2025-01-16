namespace DotNetProject.Models
{
    public class Post
    {
        public int Id { get; set; }
        public string Caption { get; set; }
        public string ImagePath { get; set; }
        public int UserID { get; set; }  
        public DateTime CreatedAt { get; set; }

        public User User { get; set; }
    }

}
