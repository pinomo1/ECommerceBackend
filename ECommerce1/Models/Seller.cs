namespace ECommerce1.Models
{
    public class Seller : AUser
    {
        public string CompanyName { get; set; }   
        public string WebsiteUrl { get; set; }
        public string ProfilePhotoUrl { get; set; }
        public IList<Product> Products { get; set; }
    }
}
