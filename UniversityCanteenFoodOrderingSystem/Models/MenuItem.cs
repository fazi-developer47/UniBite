using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace UniversityCanteenFoodOrderingSystem.Models
{
    public class MenuItem
    {
        [Key]
        public int ItemId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Category { get; set; }
        [Required]
        [Precision(18, 2)] //Add precision
        public decimal Price { get; set; }
        public bool Availability { get; set; }
        public string? ImageUrl { get; set; } // new column

        public ICollection<OrderDetail> OrderDetails { get; set; }
        public ICollection<Feedback> Feedbacks { get; set; }
    }

}
