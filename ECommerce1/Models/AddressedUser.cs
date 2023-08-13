namespace ECommerce1.Models
{
    public abstract class AddressedUser : AUser, IAddressed
    {
        /// <summary>
        /// Address list of the seller.
        /// </summary>
        public IList<Address> Addresses { get; set; }
    }
}
