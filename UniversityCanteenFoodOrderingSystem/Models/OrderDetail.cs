using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace UniversityCanteenFoodOrderingSystem.Models
{
    public class OrderDetail
    {
        [Key]
        public int OrderDetailId { get; set; }

        //Foriegn Key to Order
        public int OrderId { get; set; }
        public Order Order { get; set; }

        //Foriegn Key to MenuItem
        public int MenuItemId { get; set; }
        public MenuItem MenuItem { get; set; }


        public int Quantity { get; set; }

        [Precision(18, 2)] 
        public decimal Price { get; set; } //  Add this field
    }
}
