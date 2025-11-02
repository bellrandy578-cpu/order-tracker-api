using Microsoft.EntityFrameworkCore;

namespace OrderTracker.Models
{

    public class OrderContext : DbContext
    {
        public OrderContext(DbContextOptions<OrderContext> options)
            : base(options)
        {
        }

        public DbSet<Order> OrderItems { get; set; } = null!;

        public DbSet<OrderHistory> OrderHistoryItems { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure the one-to-many relationship starting from the 'Order' (principal) side
            modelBuilder.Entity<Order>()
                .HasMany(o => o.History) // An Order has many OrderHistory records
                .WithOne(h => h.Order)               // Each OrderHistory record has one Order
                .HasForeignKey(h => h.OrderId)       // The foreign key is the OrderId property in OrderHistory
                .IsRequired();                       // The foreign key is non-nullable (required relationship)
        }
    }

   
}
