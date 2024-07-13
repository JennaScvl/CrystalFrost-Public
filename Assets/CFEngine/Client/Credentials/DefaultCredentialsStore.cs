using System.Collections.Generic;

namespace CrystalFrost.Client.Credentials
{
    /// <summary>
    /// Defines a credentials store for Operating systems not explicity supported.
    /// </summary>
    public interface IDefaultCredentialsStore : ICredentialsStore { }

    /// <summary>
    /// A credentials store that does not persist the credentials to file.
    /// For use on operating systems where we can't be sure we are storing
    /// things securely, and so we don't store them at all.
    /// </summary>
    public class DefaultCredentialsStore : List<LoginCredential>, IDefaultCredentialsStore
    {
        public void Load()
        {
            // nothing was peristed, so nothing can be loaded.
            Clear();
        }

        public void Save()
        {
            // Do not persist.
        }
    }
}
