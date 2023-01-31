namespace ECommerce1.Models.ViewModels
{
    /// <summary>
    /// This class is used to add an address to the database.
    /// </summary>
    public class AddAddressViewModel
    {
        /// <summary>
        /// The id of the city of this address
        /// </summary>
        public string CityId { get; set; }
        /// <summary>
        /// The first line of the address
        /// </summary>
        public string First { get; set; }
        /// <summary>
        /// The second line of the address
        /// </summary>
        public string? Second { get; set; }
        /// <summary>
        /// The zip code of the address
        /// </summary>
        public string Zip { get; set; }
    }
}
