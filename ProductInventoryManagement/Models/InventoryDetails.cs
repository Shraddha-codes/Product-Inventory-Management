namespace ProductInventoryManagement.Models
{
    public class InventoryDetails
    {
        public int Id { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public string WarehouseLocation { get; set; }

    }
}
