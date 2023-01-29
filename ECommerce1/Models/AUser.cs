namespace ECommerce1.Models
{
    public abstract class AUser : AModel
    {
        /// <summary>
        /// Id in accounts database
        /// </summary>
        public string AuthId { get; set; }
        /// <summary>
        /// Email of the user
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// Phone number of the user
        /// </summary>
        public string PhoneNumber { get; set; }
    }
}
