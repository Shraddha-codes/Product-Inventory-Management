using Microsoft.EntityFrameworkCore;
using ProductInventoryManagement.Models;

namespace ProductInventoryManagement.Database
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<UserDetails> UserDetails { get; set; }
        public DbSet<ProductDetails> ProductDetails { get; set; }
        public DbSet<InventoryDetails> InventoryDetails { get; set; }
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }
    }

}
