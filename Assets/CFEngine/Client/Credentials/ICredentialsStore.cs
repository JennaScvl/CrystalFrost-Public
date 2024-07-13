using System.Collections.Generic;

namespace CrystalFrost.Client.Credentials
{
    public interface ICredentialsStore : IList<LoginCredential>
    {
        void Load();
        void Save();
    }
}
