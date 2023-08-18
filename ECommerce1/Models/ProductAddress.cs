namespace ECommerce1.Models
{
    public class ProductAddress : AModel, IQuantative
    {
        /// <summary>
        /// Quantity of the product in this address
        /// </summary>
        public int Quantity { get; set; }
        
        /// <summary>
        /// Address of the product
        /// </summary>
        public WarehouseAddress Address { get; set; }

        /// <summary>
        /// Product that this address is for
        /// </summary>
        public Product Product { get; set; }
    }
}
