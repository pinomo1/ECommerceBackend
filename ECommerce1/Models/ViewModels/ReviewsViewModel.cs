namespace ECommerce1.Models.ViewModels
{
    public class ReviewsViewModel
    {
        public IEnumerable<ReviewReviewsModel> Reviews { get; set; }
        public int TotalProductCount { get; set; }
        public int OnPageProductCount { get; set; }
        public int TotalPageCount { get; set; }
        public int CurrentPage { get; set; }
    }

    public class ReviewReviewsModel
    {
        public Guid Id { get; set; }
        public string BuyerName { get; set; }
        public int Quality { get; set; }
        public string ReviewText { get; set; }
        public IList<string> Photos { get; set; }
        public ReviewReviewsModel()
        {
            Photos = new List<string>();
        }
    }
}
