
using System;

namespace CrystalFrost.Client.Credentials
{
    /// <summary>
    /// Defines the information needed login.
    /// </summary>
    public class LoginCredential
    {
        /// <summary>
        /// Gets or sets the login server URI.
        /// </summary>
        public string LoginServer { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the user's first name.
        /// </summary>
        public string FirstName { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the user's last name.
        /// </summary>
        public string LastName { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the user's password.
        /// </summary>
        public string Password { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the date and time the credential was last used.
        /// </summary>
        public DateTime LastUsed { get; set; } = DateTime.MinValue;
    }
}
