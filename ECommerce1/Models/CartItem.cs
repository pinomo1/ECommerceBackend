﻿namespace ECommerce1.Models
{
    /// <summary>
    /// This class represents a cart item.
    /// </summary>
    public class CartItem : AItemUser, IQuantative
    {
        /// <summary>
        /// Quantity of the product
        /// </summary>
        public int Quantity { get; set; }
    }
}
