using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using UniversityCanteenFoodOrderingSystem.Models;

namespace UniversityCanteenFoodOrderingSystem.Areas.Identity.Data;

// Add profile data for application users by adding properties to the AppUser class
    public class AppUser : IdentityUser
    {
        [Required]
        public string FullName { get; set; }
        public string UserType { get; set; } //User roles(Admin, Customer(student, faculty, etc.))
        
        public DateTime RegisteredOn { get; set; } = DateTime.Now; //current date time
                                                                   //Navigation Property(relationships)

        [ValidateNever] //To prevent validation errors during model binding
        public ICollection<Order> Orders { get; set; } // One user have many orders

        [ValidateNever] //To prevent validation errors during model binding
        public ICollection<Feedback> Feedbacks { get; set; } // One user have many feedbacks
    }

