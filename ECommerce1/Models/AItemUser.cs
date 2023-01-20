namespace ECommerce1.Models
{
    public abstract class AItemUser : AModel
    {
        public Profile User { get; set; }
        public Product Product { get; set; }
    }
}
