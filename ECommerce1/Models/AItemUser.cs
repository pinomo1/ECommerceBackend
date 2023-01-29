namespace ECommerce1.Models
{
    public abstract class AItemUser : AModel
    {
        /// <summary>
        /// The user who owns this item
        /// </summary>
        public Profile User { get; set; }
        /// <summary>
        /// The product that this item is for
        /// </summary>
        public Product Product { get; set; }
    }
}
