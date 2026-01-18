using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using UniversityCanteenFoodOrderingSystem.Areas.Identity.Data;

namespace UniversityCanteenFoodOrderingSystem.Models
{
    public class Feedback
    {
        [Key]
        public int FeedbackId { get; set; }

        // Foreign Key to AppUser
        public string UserId { get; set; }
        public AppUser User { get; set; }

        // Foreign Key to MenuItem
        public int ItemId { get; set; }
        [ForeignKey("ItemId")] 
        public MenuItem MenuItem { get; set; }



        public int Rating { get; set; } // 1–5
        public string Comment { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.Now;
    }

}
