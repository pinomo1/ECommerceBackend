namespace ECommerce1.Models.ViewModels
{
    public class CategoryResponse
    {
        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        public string Name { get; set; }
        public bool AllowProducts { get; set; }
        public string ImageUrl { get; set; }
    }
}
