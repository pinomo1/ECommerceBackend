namespace ECommerce1.Models
{
    public class Product : AModel
    {
        public string Name { get; set; }
        public DateTime CreationTime { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }

        public Category Category { get; set; }
        public Seller Seller { get; set; }
        public IList<ProductPhoto> ProductPhotos { get; set; }
        public IList<Review> Reviews { get; set; }
        public IList<CartItem> CartItems { get; set; }
        public IList<Order> Orders { get; set; }

        public Product()
        {
            ProductPhotos = new List<ProductPhoto>();
            Reviews = new List<Review>();
            CartItems = new List<CartItem>();
            Orders = new List<Order>();
        }
    }
}
