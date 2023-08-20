namespace ECommerce1.Models
{
    /// <summary>
    /// (Mail) Order class
    /// </summary>
    public class Order : AModel, IQuantative
    {
        /// <summary>
        /// The user who owns this item
        /// </summary>
        public Profile User { get; set; }
        /// <summary>
        /// The product that this item is for
        /// </summary>
        public Product Product { get; set; }
        /// <summary>
        /// Copy of the full address instead of address object
        /// </summary>
        public string CustomerAddressCopy { get; set; }
        /// <summary>
        /// Copy of the full address instead of address object
        /// </summary>
        public string WarehouseAddressCopy { get; set; }
        /// <summary>
        /// Time of order
        /// </summary>
        public DateTime OrderTime { get; set; }
        /// <summary>
        /// Status of the order
        /// </summary>
        public int OrderStatus { get; set; }
        
        /// <summary>
        /// Quantity of the product
        /// </summary>
        public int Quantity { get; set; }
    }

    public enum OrderStatus
    {
        Unverified = 0,
        Cancelling,
        Cancelled,
        Returning,
        Returned,
        Delivering,
        Delivered
    }
}
