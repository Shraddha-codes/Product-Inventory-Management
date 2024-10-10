namespace ProductInventoryManagement.Models
{
    public class InventoryTransaction
    {
        public int Id { get; set; }
        public int ProductID { get; set; }
        public int QuantityChanged { get; set; }
        public DateTime Timestamp { get; set; }
        public string Reason { get; set; }
        public string Users { get; set; } 
        public virtual ProductDetails Product { get; set; } 
    }

    public class InventoryAdjustmentDto
    {
        public int QuantityChanged { get; set; }
        public string Reason { get; set; }
        public string User { get; set; } 
    }

}
