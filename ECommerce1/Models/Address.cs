namespace ECommerce1.Models
{
    /// <summary>
    /// The address of a user
    /// </summary>
    public class Address : AModel
    {
        /// <summary>
        /// The city of the address
        /// </summary>
        public City City { get; set; }
        /// <summary>
        /// The user that owns this address
        /// </summary>
        public AddressedUser User { get; set; }
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
        /// <summary>
        /// Products in said address
        /// </summary>
        public IList<ProductAddress> Products { get; set; }

        public string Normalize()
        {
            return $"{First}\n{Second}\n{City.Name}, {City.Country.Name}\n{Zip}";
        }

        public string Normalize(string number)
        {
            return Normalize() + $"\n{number}";
        }
    }
}
