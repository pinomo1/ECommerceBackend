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
    }
}
