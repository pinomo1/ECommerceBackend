namespace ECommerce1.Models.ViewModels
{
    public abstract class ARegistrationCredentials
    {
        /// <summary>
        /// The email address of the user.
        /// </summary>
        private string _email;
        /// <summary>
        /// The email address of the user.
        /// </summary>
        public string Email
        {
            get { return _email; }
            set
            {
                _email = value.ToLower();
            }
        }
        /// <summary>
        /// The password of the user.
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// The password of the user.
        /// </summary>
        public string PasswordConfirmation { get; set; }
        /// <summary>
        /// The phone number of the user.
        /// </summary>
        public string PhoneNumber { get; set; }
    }
}
