using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetProject.Models
{
    public class Message
    {
        public int Id { get; set; }
        public int SenderUserID { get; set; }
        public int ReceiverUserID { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; }


        [ForeignKey("SenderUserID")]
        public User Sender { get; set; }

        [ForeignKey("ReceiverUserID")]
        public User Receiver { get; set; }
    }
}
