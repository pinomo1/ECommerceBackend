namespace ECommerce1.Models
{
    /// <summary>
    /// This class represents a buyer's profile.
    /// </summary>
    public class Profile : AUser
    {
        /// <summary>
        /// The first name/name of the buyer.
        /// </summary>
        public string FirstName { get; set; }
        /// <summary>
        /// The middle name/patronym of the buyer if any.
        /// </summary>
        public string? MiddleName { get; set; }
        /// <summary>
        /// The last name/family name/surname of the buyer.
        /// </summary>
        public string LastName { get; set; }
        /// <summary>
        /// Address list of the buyer.
        /// </summary>
        public IList<Address> Addresses { get; set; }
        /// <summary>
        /// Items that are in cart of the buyer.
        /// </summary>
        public IList<CartItem> CartItems { get; set; }
        /// <summary>
        /// Order list of the buyer.
        /// </summary>
        public IList<Order> Orders { get; set; }
        /// <summary>
        /// Review list of the buyer.
        /// </summary>
        public IList<Review> Reviews { get; set; }
        /// <summary>
        /// Favourite items of the buyer.
        /// </summary>
        public IList<FavouriteItem> FavouriteItems { get; set; }

        public Profile()
        {
            Addresses = new List<Address>();
            CartItems = new List<CartItem>();
            Orders = new List<Order>();
            Reviews = new List<Review>();
            FavouriteItems = new List<FavouriteItem>();
        }
    }
}
