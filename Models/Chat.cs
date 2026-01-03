using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HarvestHub.WebApp.Models
{
    public class Conversation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string BuyerId { get; set; }  // ApplicationUser Id

        [Required]
        public string FarmerId { get; set; } // ApplicationUser Id

        public int? CropId { get; set; }     // Related crop (optional)

        [ForeignKey("BuyerId")]
        public ApplicationUser Buyer { get; set; }

        [ForeignKey("FarmerId")]
        public ApplicationUser Farmer { get; set; }

        [ForeignKey("CropId")]
        public Crop? Crop { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;

        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }

    public class ChatMessage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ConversationId { get; set; }

        [ForeignKey("ConversationId")]
        public Conversation Conversation { get; set; }

        [Required]
        public string SenderId { get; set; } // ApplicationUser Id

        [Required]
        public string SenderName { get; set; }

        [Required]
        public string Message { get; set; }

        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
    }
}
