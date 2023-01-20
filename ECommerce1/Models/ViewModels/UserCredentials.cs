namespace ECommerce1.Models.ViewModels
{
    public class UserCredentials : ARegistrationCredentials
    {
        public string FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string LastName { get; set; }
    }
}
