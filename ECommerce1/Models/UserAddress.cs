namespace ECommerce1.Models
{
    /// <summary>
    /// The address of a user
    /// </summary>
    public class UserAddress : AAddress
    {
        /// <summary>
        /// The user that owns this address
        /// </summary>
        public Profile User { get; set; }
    }
}
