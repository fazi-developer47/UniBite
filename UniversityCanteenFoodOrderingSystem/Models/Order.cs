using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UniversityCanteenFoodOrderingSystem.Areas.Identity.Data;

namespace UniversityCanteenFoodOrderingSystem.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        // Foreign Key to AppUser
        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public AppUser User { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Column(TypeName = "nvarchar(20)")]
        public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;

        [Required]
        [Precision(18, 2)]
        public decimal TotalPrice { get; set; }

        [Column(TypeName = "nvarchar(30)")]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnPickup;

        //New flag
        public bool IsDeletedByUser { get; set; } = false;

        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>(); // initialized
    }

    public enum OrderStatus
    {
        Pending,    // Order placed but not yet processed
        Preparing,  // Canteen staff is preparing the food
        Ready,      // Food is ready for pickup
        Delivered   // Food handed over to customer
    }

    public enum PaymentMethod
    {
        CashOnPickup,            // Customer pays cash when collecting food
        OnlineTransferOnPickup   // Customer transfers money online at pickup time
    }
}
