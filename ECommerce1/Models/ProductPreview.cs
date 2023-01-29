namespace ECommerce1.Models
{
    /// <summary>
    /// This class is used to display a product in a list of products.
    /// </summary>
    public class ProductPreview : AModel
    {
        /// <summary>
        /// The name of the product.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The price of the product.
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// The URL of the preview photo of the product.
        /// </summary>
        public string PhotoUrl { get; set; }
    }
}
