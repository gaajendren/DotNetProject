namespace DotNetProject.Models
{
    public class FollowViewModel
    {
        public string SearchTerm { get; set; }
        public List<User> Users { get; set; }

        public List<int> FollowedUsers { get; set; }
    }
}