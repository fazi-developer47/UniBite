using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UniversityCanteenFoodOrderingSystem.Areas.Identity.Data;
using UniversityCanteenFoodOrderingSystem.Models;

namespace UniversityCanteenFoodOrderingSystem.Areas.Identity.Data;

public class DBContextUniBite : IdentityDbContext<AppUser>
{
    public DBContextUniBite(DbContextOptions<DBContextUniBite> options)
        : base(options)
    {
    }


    public DbSet<MenuItem> MenuItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }
    public DbSet<Feedback> Feedbacks { get; set; }


    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);


        builder.Entity<Order>()
            .Property(o => o.OrderStatus)
            .HasConversion<string>();
        builder.Entity<Order>()
            .Property(o => o.PaymentMethod)
            .HasConversion<string>();

        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);

        builder.Entity<AppUser>(entity => { entity.ToTable(name: "Users");  });
        builder.Entity<IdentityRole>(entity => { entity.ToTable(name: "Roles"); });
        builder.Entity<IdentityUserRole<string>>(entity => { entity.ToTable("UserRoles"); });
        builder.Entity<IdentityUserClaim<string>>(entity => { entity.ToTable("UserClaims"); });
        builder.Entity<IdentityUserLogin<string>>(entity => { entity.ToTable("UserLogins"); });
        builder.Entity<IdentityRoleClaim<string>>(entity => { entity.ToTable("RoleClaims"); });
        builder.Entity<IdentityUserToken<string>>(entity => { entity.ToTable("UserTokens"); });


    }
}
