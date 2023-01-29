namespace ECommerce1.Models.ViewModels
{
    /// <summary>
    /// View model used to display the category and its subcategories.
    /// </summary>
    public class GetCategoryViewModel
    {
        /// <summary>
        /// The category id.
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// The category name.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Why is this here? I don't know. I think it's a mistake. Ignore this field
        /// </summary>
        public bool AllowProducts { get; set; }

        /// <summary>
        /// The subcategories of this category.
        /// </summary>
        public IList<GetSubCategoryViewModel> ChildCategories { get; set; }

        public GetCategoryViewModel()
        {
            ChildCategories = new List<GetSubCategoryViewModel>();
        }
    }

    /// <summary>
    /// View model used to display the subcategory in the list of categories
    /// </summary>
    public class GetSubCategoryViewModel
    {
        /// <summary>
        /// The subcategory id.
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// The subcategory name.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Determine whether the subcategory allows products or categories (refer to Category model).
        /// </summary>
        public bool AllowProducts { get; set; }
    }
}
