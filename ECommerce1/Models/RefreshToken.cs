namespace ECommerce1.Models
{
    /// <summary>
    /// The refresh token is used to refresh the access token
    /// </summary>
    public class RefreshToken
    {
        /// <summary>
        /// The token that will be used to refresh the access token
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// The user id of the user that the token belongs to in the accounts database
        /// </summary>
        public string AppUserId { get; set; }
        /// <summary>
        /// The date and time that the token expires
        /// </summary>
        public DateTime ExpiresAt { get; set; }
        /// <summary>
        /// The user that the token belongs to
        /// </summary>
        public AuthUser AuthUser { get; set; }
    }
}
