namespace SmartProctor.Client
{
    public class AppStates
    {
        public string LastUrl { get; set; }
        
        /// <summary>
        /// Store the user name for DeepLens auth
        /// </summary>
        public string UserId { get; set; }
        
        /// <summary>
        /// Store the password for DeepLens auth
        /// </summary>
        public string Password { get; set; }
    }
}