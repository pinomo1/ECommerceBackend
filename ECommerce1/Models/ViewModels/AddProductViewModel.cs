namespace ECommerce1.Models.ViewModels
{
    /// <summary>
    /// This class is used to add a product to the database.
    /// </summary>
    public class AddProductViewModel
    {
        /// <summary>
        /// The name of the product.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The description of the product.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// The price of the product.
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// Photos of the product.
        /// </summary>
        public IFormFile?[] Photos { get; set; }
        /// <summary>
        /// Id of a category this item is going to be added to
        /// </summary>
        public string CategoryId { get; set; }
    }
}
