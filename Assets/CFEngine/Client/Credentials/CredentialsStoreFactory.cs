using UnityEngine;

namespace CrystalFrost.Client.Credentials
{
    public interface ICredentialsStoreFactory
    {
        ICredentialsStore GetCredentialsStore();
    }

    public class CredentialsStoreFactory : ICredentialsStoreFactory
    {
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
