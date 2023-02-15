namespace ECommerce1.Models.ViewModels
{
    public class OrderViewModel
    {
        public IList<OrdersOrderViewModel> Orders { get; set; }

        /// <summary>
        /// The total number of orders in the database by given query.
        /// </summary>
        public int TotalProductCount { get; set; }
        /// <summary>
        /// The number of orders to be displayed on the page.
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
        public OrderViewModel()
        {
            Orders = new List<OrdersOrderViewModel>();
        }
    }

    public class OrdersOrderViewModel : AModel
    {
        /// <summary>
        /// The user who owns this item
        /// </summary>
        public string UserId { get; set; }
        public string ProductId { get; set; }
        /// <summary>
        /// Product name
        /// </summary>
        public string ProductName { get; set; }
        /// <summary>
        /// Product description
        /// </summary>
        // public string Description { get; set; }
        /// <summary>
        /// Product price
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// Whether in stock or not
        /// </summary>
        public bool? InStock { get; set; }
        /// <summary>
        /// Copy of the full address instead of address object
        /// </summary>
        public string AddressCopy { get; set; }

        /// <summary>
        /// Time of order
        /// </summary>
        public DateTime OrderTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int OrderStatus { get; set; }
    }
}
