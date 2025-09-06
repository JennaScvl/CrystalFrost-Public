using UnityEngine;

namespace CrystalFrost.Client.Credentials
{
    /// <summary>
    /// Defines a factory for creating platform-specific credential stores.
    /// </summary>
    public interface ICredentialsStoreFactory
    {
        /// <summary>
        /// Gets a credential store appropriate for the current operating system.
        /// </summary>
        /// <returns>An instance of <see cref="ICredentialsStore"/>.</returns>
        ICredentialsStore GetCredentialsStore();
    }

    /// <summary>
    /// Implements a factory for creating platform-specific credential stores.
    /// </summary>
    public class CredentialsStoreFactory : ICredentialsStoreFactory
    {
        /// <summary>
        /// Gets a credential store appropriate for the current operating system.
        /// </summary>
        /// <returns>
        /// An instance of <see cref="IWindowsCredentialsStore"/> on Windows,
        /// or <see cref="IDefaultCredentialsStore"/> on other platforms.
        /// </returns>
        public ICredentialsStore GetCredentialsStore()
        {
            // Get a creditals store that is appropriate for the operating system.
            // if there is not an operating system specific one, return the 
            // default store. (The default store does not persist data).
            return Application.platform switch
            {
                RuntimePlatform.WindowsPlayer or
                RuntimePlatform.WindowsEditor
                    => Services.GetService<IWindowsCredentialsStore>(),
                // default - don't try to store credentials securly cause we
                // we don't yet know how for that OS.
                _ => Services.GetService<IDefaultCredentialsStore>(),
            };
        }
    }
}
