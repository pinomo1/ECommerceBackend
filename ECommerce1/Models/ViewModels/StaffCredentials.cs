namespace ECommerce1.Models.ViewModels
{
    /// <summary>
    /// Staff credentials for registration
    /// </summary>
    public class StaffCredentials : ARegistrationCredentials
    {
        /// <summary>
        /// Staff's display name
        /// </summary>
        public string DisplayName { get; set; }
    }
}
