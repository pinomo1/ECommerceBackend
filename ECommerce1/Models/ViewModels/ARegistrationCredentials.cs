namespace ECommerce1.Models.ViewModels
{
    public abstract class ARegistrationCredentials
    {
        private string _email;
        public string Email
        {
            get { return _email; }
            set
            {
                _email = value.ToLower();
            }
        }
        public string Password { get; set; }
        public string PasswordConfirmation { get; set; }
        public string PhoneNumber { get; set; }
    }
}
