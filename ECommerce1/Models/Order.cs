namespace ECommerce1.Models
{
    /// <summary>
    /// (Mail) Order class
    /// </summary>
    public class Order : AItemUser
    {
        /// <summary>
        /// Copy of the full address instead of address object
        /// </summary>
        public string AddressCopy { get; set; }

        /// <summary>
        /// Time of order
        /// </summary>
        public DateTime OrderTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int OrderStatus { get; set; }
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
