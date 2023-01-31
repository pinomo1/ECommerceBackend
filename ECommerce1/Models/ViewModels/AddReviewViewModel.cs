namespace ECommerce1.Models.ViewModels
{
    /// <summary>
    /// This class is used to add a review to the database.
    /// </summary>
    public class AddReviewViewModel
    {
        /// <summary>
        /// The id of the product that the review is for.
        /// </summary>
        public string ProductId { get; set; }
        /// <summary>
        /// The text of the review.
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// Rating 1-5
        /// </summary>
        public int Rating { get; set; }
        /// <summary>
        /// Photos of the review
        /// </summary>
        public IFormFile?[] Photos { get; set; }
    }
}
