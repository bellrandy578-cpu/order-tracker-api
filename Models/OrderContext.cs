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
            // Configure relationship
            modelBuilder.Entity<OrderHistory>()
                .HasOne(h => h.Order)
                .WithMany(o => o.History)
                .HasForeignKey(h => h.OrderId);
        }
    }

   
}
