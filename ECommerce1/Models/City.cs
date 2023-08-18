namespace ECommerce1.Models
{
    /// <summary>
    /// City class
    /// </summary>
    public class City : AModel
    {
        /// <summary>
        /// Name of the city
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Country this city belongs to
        /// </summary>
        public Country Country { get; set; }

        /// <summary>
        /// Addresses of this city
        /// </summary>
        public IList<UserAddress> UserAddresses { get; set; }
        /// <summary>
        /// Warehouses of this city
        /// </summary>
        public IList<WarehouseAddress> WarehouseAddresses { get; set; }

        public City()
        {
            UserAddresses = new List<UserAddress>();
            WarehouseAddresses = new List<WarehouseAddress>();
        }
    }
}
