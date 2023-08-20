namespace ECommerce1.Models
{
    public class WarehouseAddress : AAddress
    {
        /// <summary>
        /// The user that owns this address
        /// </summary>
        public Seller User { get; set; }
        /// <summary>
        /// Products in said address
        /// </summary>
        public IList<ProductAddress> Products { get; set; }
    }
}
