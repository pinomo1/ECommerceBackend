namespace ECommerce1.Models.ViewModels
{
    public abstract class ProductsViewModel
    {
        public IEnumerable<ProductsProductViewModel> Products { get; set; }
        public int TotalProductCount { get; set; }
        public int OnPageProductCount { get; set; }
        public int TotalPageCount { get; set; }
        public int CurrentPage { get; set; }


        public enum ProductSorting
        {
            OlderFirst = 1,
            NewerFirst,
            CheaperFirst,
            ExpensiveFirst
        }
    }

    public class ProductsViewModelByCategory : ProductsViewModel
    {
        public Category Category { get; set; }
    }

    public class ProductsViewModelBySeller : ProductsViewModel
    {
        public Seller Seller { get; set; }
    }

    public class ProductsViewModelByTitle : ProductsViewModel
    {
        public string Title { get; set; }
    }

    public class ProductsProductViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime CreationTime { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string? FirstPhotoUrl { get; set; } 
    }
}
