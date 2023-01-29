namespace ECommerce1.Models
{
    /// <summary>
    /// Class staff (admin/moderator/tech support)
    /// </summary>
    public class Staff : AUser
    {
        /// <summary>
        /// Staff's display name
        /// </summary>
        public string DisplayName { get; set; }
    }
}
