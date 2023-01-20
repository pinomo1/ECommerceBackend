namespace ECommerce1.Options
{
    public class TokenGeneratorOptions
    {
        public string Secret { get; set; }
        public TimeSpan AccessExpiration { get; set; }
        public TimeSpan RefreshExpiration { get; set; }
    }
}
