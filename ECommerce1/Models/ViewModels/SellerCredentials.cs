namespace ECommerce1.Models.ViewModels
{
    /// <summary>
    /// The seller's credentials for registration.
    /// </summary>
    public class SellerCredentials : ARegistrationCredentials
    {
        /// <summary>
        /// The company name.
        /// </summary>
        public string CompanyName { get; set; }
        /// <summary>
        /// The website URL.
        /// </summary>
        public string WebsiteUrl { get; set; }
    }
}
