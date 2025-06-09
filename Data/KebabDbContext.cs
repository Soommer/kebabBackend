using kebabBackend.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace kebabBackend.Data
{
    public class KebabDbContext : DbContext
    {
        public KebabDbContext(DbContextOptions<KebabDbContext> options) : base(options) { }

        public DbSet<menuItem> menuItems { get; set; }
        public DbSet<kebabImage> kebabImages { get; set; }
        public DbSet<MeatType> meatTypes { get; set; }
        public DbSet<extraIgredients> extraIgredients { get; set; }
        public DbSet<CartItem> cartItems { get; set; }
        public DbSet<Cart> carts { get; set; }
        public DbSet<CartItemExtraIngredient> cartItemExtraIngredients { get; set; }
        public DbSet<Souce> souces { get; set; }
        public DbSet<paydOrder> paydOrders { get; set; }
        public DbSet<User> user { get; set; }
        public DbSet<menuItemCategory> menuItemCategories { get; set; }
    }
}
