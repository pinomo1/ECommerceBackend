namespace ECommerce1.Models.ViewModels
{
    public class CartItemViewModel
    {
        /// <summary>
        /// The product that this item is for
        /// </summary>
        public Product Product { get; set; }
        /// <summary>
        /// The quantity of this product in the cart
        /// </summary>
        public int Quantity { get; set; }
    }
}
