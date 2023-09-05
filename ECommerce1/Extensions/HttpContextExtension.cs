namespace ECommerce1.Extensions
{
    public static class HttpContextExtension
    {
        public static string GetLocale(this HttpContext context)
        {
            var cookies = context.Request.GetTypedHeaders().Cookie;
            var cookie = cookies.FirstOrDefault(c => c.Name == "locale");
            string locale = cookie?.Value.ToString() ?? "en";
            return locale;
        }
    }
}
