namespace DotNetProject.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }

        public ICollection<Post> Posts { get; set; }

        public ICollection<Message> SentMessages { get; set; }  
        public ICollection<Message> ReceivedMessages { get; set; }  
    }
}
