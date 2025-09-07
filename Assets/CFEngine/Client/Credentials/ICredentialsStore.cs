using System.Collections.Generic;

namespace CrystalFrost.Client.Credentials
{
    /// <summary>
    /// Defines a store for managing login credentials, with methods for loading and saving.
    /// </summary>
    public interface ICredentialsStore : IList<LoginCredential>
    {
        /// <summary>
        /// Loads credentials from a persistent source.
        /// </summary>
        void Load();

        /// <summary>
        /// Saves credentials to a persistent source.
        /// </summary>
        void Save();
    }
}
