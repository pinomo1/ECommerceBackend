namespace ECommerce1.Models
{
    public class Category : AModel
    {
        public Category? ParentCategory { get; set; }
        public string Name { get; set; }
        public bool AllowProducts { get; set; }

        public IList<Product> Products { get; set; }
        public IList<Category> ChildCategories { get; set; }

        public Category()
        {
            Products = new List<Product>();
            ChildCategories = new List<Category>();
        }
    }
}
