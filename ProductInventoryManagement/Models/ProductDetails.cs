namespace ProductInventoryManagement.Models
{
    public class ProductDetails
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedDate { get; set; }
        public string SKU { get; set; }
        public string Categories { get; set; }
    }

    public class ProductDetailsDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string SKU { get; set; }
        public int Quantity { get; set; }

        public virtual InventoryDetails Inventory { get; set; }
    }

}
