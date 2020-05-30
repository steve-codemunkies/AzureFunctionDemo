namespace AzureFunctionStaticFiles
{
    /// <summary>
    /// Security configuration for the application
    /// </summary>
    public class SecurityOptions
    {
        /// <summary>
        /// The user who is allowed to used the application
        /// </summary>
        public string SecretUser { get; set; }

        /// <summary>
        /// The file to serve when an unauthorised user requests any file
        /// </summary>
        public string UnauthorisedFile { get; set; }
    }
}
