namespace ECommerce1.Models
{
    public class Country : AModel
    {
        public string Name { get; set; }
        public IList<City> Cities { get; set; }

        public Country()
        {
            Cities = new List<City>();
        }
    }
}
