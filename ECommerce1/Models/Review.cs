namespace ECommerce1.Models
{
    /// <summary>
    /// A review of an item by a user.
    /// </summary>
    public class Review : AItemUser
    {
        /// <summary>
        /// The text of the review.
        /// </summary>
        public string ReviewText { get; set; }
        /// <summary>
        /// The quality of the item, on a scale of 1 to 5.
        /// </summary>
        public int Quality { get; set; }
        /// <summary>
        /// List of photos associated with the review.
        /// </summary>
        public IList<ReviewPhoto> Photos { get; set; }

        public Review()
        {
            Photos = new List<ReviewPhoto>();
        }
    }
}
