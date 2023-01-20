namespace ECommerce1.Models.ViewModels
{
    public class AddCategoryViewModel
    {
        public Guid? ParentCategoryId { get; set; }
        public string Name { get; set; }
        public bool AllowProducts { get; set; }
    }
}
