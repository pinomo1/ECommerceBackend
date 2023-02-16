namespace ECommerce1.Models.ViewModels
{
    public class AddressViewModel
    {
        public string First { get; set; }
        public string Second { get; set; }
        public string Zip { get; set; }
        public CityViewModel City { get; set; }
        public CountryViewModel Country { get; set; }
    }

    public class CityViewModel
    {
        public string Name { get; set; }
        public string Id { get; set; }
    }

    public class CountryViewModel
    {
        public string Name { get; set; }
        public string Id { get; set; }
    }
}
