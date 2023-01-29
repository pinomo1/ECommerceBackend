namespace ECommerce1.Models
{
    /// <summary>
    /// This class represents a category
    /// </summary>
    public class Category : AModel
    {
        /// <summary>
        /// The parent category if any
        /// </summary>
        public Category? ParentCategory { get; set; }
        /// <summary>
        /// The name of the category
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Type of the category, there are 2:
        /// 1) true - category that stores products
        /// 2) false - category that stores other categories
        /// </summary>
        public bool AllowProducts { get; set; }

        /// <summary>
        /// The products that are in this category if AllowProducts is true
        /// </summary>
        public IList<Product> Products { get; set; }
        /// <summary>
        /// The child categories of this category if AllowProducts is false
        /// </summary>
        public IList<Category> ChildCategories { get; set; }

        public Category()
        {
            Products = new List<Product>();
            ChildCategories = new List<Category>();
        }
    }
}
