namespace ECommerce1.Models.ViewModels
{
    /// <summary>
    /// The base class for all the view models that are used to display a list of products.
    /// </summary>
    public abstract class ProductsViewModel
    {
        /// <summary>
        /// The products to be displayed on the page.
        /// </summary>
        public IEnumerable<ProductsProductViewModel> Products { get; set; }
        /// <summary>
        /// The total number of products in the database by given query.
        /// </summary>
        public int TotalProductCount { get; set; }
        /// <summary>
        /// The number of products to be displayed on the page.
        /// </summary>
        public int OnPageProductCount { get; set; }
        /// <summary>
        /// The total number of pages.
        /// </summary>
        public int TotalPageCount { get; set; }
        /// <summary>
        /// The current page.
        /// </summary>
        public int CurrentPage { get; set; }
        /// <summary>
        /// Minimal price of the products.
        /// </summary>
        public decimal MinPrice { get; set; }
        /// <summary>
        /// Maximal price of the products.
        /// </summary>
        public decimal MaxPrice { get; set; }

        /// <summary>
        /// The sorting method.
        /// 1 - Older first
        /// 2 - Newer first
        /// 3 - Cheaper first
        /// 4 - Expensive first
        /// </summary>
        public enum ProductSorting
        {
            PopularFirst = 1,
            BestFirst,
            CheaperFirst,
            ExpensiveFirst,
            NewerFirst,
            OlderFirst
        }
    }

    /// <summary>
    /// The view model used to display a list of products by category.
    /// </summary>
    public class ProductsViewModelByCategory : ProductsViewModel
    {
        /// <summary>
        /// The category of the products.
        /// </summary>
        public Category Category { get; set; }
    }

    /// <summary>
    /// The view model used to display a list of products by seller.
    /// </summary>
    public class ProductsViewModelBySeller : ProductsViewModel
    {
        /// <summary>
        /// The seller of the products.
        /// </summary>
        public Seller Seller { get; set; }
    }

    /// <summary>
    /// The view model used to display a list of products by title.
    /// </summary>
    public class ProductsViewModelByTitle : ProductsViewModel
    {
        /// <summary>
        /// The title of the products.
        /// </summary>
        public string Title { get; set; }
    }

    /// <summary>
    /// The view model used to display a product in the list.
    /// </summary>
    public class ProductsProductViewModel
    {
        /// <summary>
        /// The product id.
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// The product name.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The product creation time.
        /// </summary>
        public DateTime CreationTime { get; set; }
        /// <summary>
        /// The product description.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// The product price.
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// Preview photo's URL
        /// </summary>
        public string? FirstPhotoUrl { get; set; } 
        /// <summary>
        /// Count of orders
        /// </summary>
        public int OrderCount { get; set; }
        /// <summary>
        /// Rating of product
        /// </summary>
        public double Rating { get; set; }
        /// <summary>
        /// Whether in stock or not
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Whether the product is boosted or not
        /// </summary>
        public bool IsBoosted { get; set; }
    }
}
