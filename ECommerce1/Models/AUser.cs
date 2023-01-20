namespace ECommerce1.Models
{
    public abstract class AUser : AModel
    {
        public string AuthId { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }
}
