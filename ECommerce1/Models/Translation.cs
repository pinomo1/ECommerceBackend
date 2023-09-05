namespace ECommerce1.Models
{
    public enum TranslatedObjectType
    {
        CategoryName,
    }

    public class Translation : AModel
    {
        public TranslatedObjectType ObjectType { get; set; }
        public string ObjectId { get; set; }
        public string Locale { get; set; }
        public bool IsDefault { get; set; }
        public string Text { get; set; }
    }
}
