using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce1.Models
{
    /// <summary>
    /// Seller is a type of user that can sell products.
    /// </summary>
    public class Seller : AUser
    {
        /// <summary>
        /// The name of the company that the seller represents.
        /// </summary>
        public string CompanyName { get; set; }
        /// <summary>
        /// The URL of the profile photo of the seller.
        /// </summary>
        public string ProfilePhotoUrl { get; set; }
        /// <summary>
        /// The products that the seller has for sale.
        /// </summary>
        public IList<Product> Products { get; set; }
        // <summary>
        // Address list of the buyer.
        // </summary>
        public IList<WarehouseAddress> Addresses { get; set; }

        public DateTime BoostedUntil { get; set; } = DateTime.MinValue;

        [NotMapped]
        public bool IsBoosted
        {
            get
            {
                return BoostedUntil > DateTime.Now;
            }
        }
    }
}
