namespace DotNetProject.Models
{
    public class Follow
    {
        public int Id { get; set; }
        public int FollowerUserID { get; set; }
        public int FollowingUserID { get; set; }

        public User Follower { get; set; }
        public User Following { get; set; }
    }
}
