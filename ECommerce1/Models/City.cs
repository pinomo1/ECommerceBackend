namespace ECommerce1.Models
{
    public class City : AModel
    {
        public string Name { get; set; }
        public Country Country { get; set; }

        public IList<Address> Addresses { get; set; }

        public City()
        {
            Addresses = new List<Address>();
        }
    }
}
