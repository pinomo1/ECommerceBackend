namespace ECommerce1.Models
{
    /// <summary>
    /// A review of an item by a user.
    /// </summary>
    public class ReviewPhoto : APhoto
    {
        /// <summary>
        /// The review this photo is associated with.
        /// </summary>
        public Review Review { get; set; }
    }
}
