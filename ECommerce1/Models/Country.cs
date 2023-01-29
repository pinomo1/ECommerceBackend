namespace ECommerce1.Models
{
    /// <summary>
    /// Country class
    /// </summary>
    public class Country : AModel
    {
        /// <summary>
        /// Country name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Cities of this country
        /// </summary>
        public IList<City> Cities { get; set; }

        public Country()
        {
            Cities = new List<City>();
        }
    }
}
