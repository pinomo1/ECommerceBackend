namespace ECommerce1.Models.ViewModels
{
    public class GetCategoryViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool AllowProducts { get; set; }

        public IList<GetSubCategoryViewModel> ChildCategories { get; set; }

        public GetCategoryViewModel()
        {
            ChildCategories = new List<GetSubCategoryViewModel>();
        }
    }

    public class GetSubCategoryViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool AllowProducts { get; set; }
    }
}
