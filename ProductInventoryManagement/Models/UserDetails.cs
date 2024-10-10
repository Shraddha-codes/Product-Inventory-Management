namespace ProductInventoryManagement.Models
{
    public class UserDetails
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string HashKey { get; set; }
        public string Role { get; set; }
    }
}
