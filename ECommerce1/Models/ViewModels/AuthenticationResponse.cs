namespace ECommerce1.Models.ViewModels
{
    /// <summary>
    /// This class is used to return the authentication response to the client.
    /// </summary>
    public class AuthenticationResponse
    {
        /// <summary>
        /// The access token.
        /// </summary>
        public string AccessToken { get; set; }
        /// <summary>
        /// The refresh token used to get a new access token.
        /// </summary>
        public string RefreshToken { get; set; }
    }
}
