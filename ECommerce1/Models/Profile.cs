namespace ECommerce1.Models
{
    public class Profile : AUser
    {
        public string FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string LastName { get; set; }
        public IList<Address> Addresses { get; set; }
        public IList<CartItem> CartItems { get; set; }
        public IList<Order> Orders { get; set; }
        public IList<Review> Reviews { get; set; }

        public Profile()
        {
            Addresses = new List<Address>();
            CartItems = new List<CartItem>();
            Orders = new List<Order>();
            Reviews = new List<Review>();
        }
    }
}
