namespace ECommerce1.Models
{
    /// <summary>
    /// This class represents a photo of a product.
    /// </summary>
    public class ProductPhoto : APhoto
    {
        /// <summary>
        /// The product this photo belongs to.
        /// </summary>
        public Product Product { get; set; }
    }
}
