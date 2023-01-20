namespace ECommerce1.Models
{
    public class Address : AModel
    {
        public City City { get; set; }
        public Profile User { get; set; }
        public string First { get; set; }
        public string? Second { get; set; }
        public string Zip { get; set; }
    }
}
