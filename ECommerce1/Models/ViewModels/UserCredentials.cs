namespace ECommerce1.Models.ViewModels
{
    /// <summary>
    /// The user's credentials for registration.
    /// </summary>
    public class UserCredentials : ARegistrationCredentials
    {
        /// <summary>
        /// The user's first name.
        /// </summary>
        public string FirstName { get; set; }
        /// <summary>
        /// The user's last name.
        /// </summary>
        public string LastName { get; set; }
    }
}
