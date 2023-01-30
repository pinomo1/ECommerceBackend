namespace ECommerce1.Models.ViewModels
{
    public class AllCategoriesResponse
    {
        public CategoryResponse[] MainCategories { get; set; }
        public CategoryResponse[] SubCategories { get; set; }
    }
}
