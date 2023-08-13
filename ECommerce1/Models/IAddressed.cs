namespace ECommerce1.Models
{
    public interface IAddressed
    {
        /// <summary>
        /// Address list
        /// </summary>
        public IList<Address> Addresses { get; set; }
    }
}
