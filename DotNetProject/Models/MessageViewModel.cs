namespace DotNetProject.Models
{
    public class MessageViewModel
    {
        public int FollowedUserId { get; set; }
        public string Message { get; set; }
        public List<User> FollowedUsers { get; set; }
    }
}