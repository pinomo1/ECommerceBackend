namespace ECommerce1.Models.ViewModels
{
    /// <summary>
    /// View model for adding a category
    /// </summary>
    public class AddCategoryViewModel
    {
        /// <summary>
        /// The parent category id (if is subcategory)
        /// </summary>
        public Guid? ParentCategoryId { get; set; }
        /// <summary>
        /// The name of the category
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Refer to Category model
        /// </summary>
        public bool AllowProducts { get; set; }
    }
}
