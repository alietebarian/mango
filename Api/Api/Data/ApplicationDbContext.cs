using Api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Api.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    public DbSet<MenuItem> MenuItems { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<ShoppingCart> shoppingCarts { get; set; }
    public DbSet<OrderHeader> OrderHeaders { get; set; }
    public DbSet<OrderDetails> OrderDetails { get; set; }
}
