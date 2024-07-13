using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

namespace CrystalFrost.Client.Credentials
{
    public interface IWindowsCredentialsStore : ICredentialsStore { }

    /// <summary>
    /// A Credentials store that uses the Windows DPAPI to secure data
    /// for the currently logged in windows user.
    /// </summary>
    public class WindowsCredentialsStore : List<LoginCredential>, IWindowsCredentialsStore
    {
        private readonly ILogger<WindowsCredentialsStore> _log;

        public WindowsCredentialsStore(ILogger<WindowsCredentialsStore> log)
        {
            _log = log;
        }

        public void Load()
        {
            _log.LoadingCredentials();
            
            Clear();

            var filename = Path.Combine(Application.persistentDataPath, "credentials.dat");
            if (!File.Exists(filename))
            {
                _log.FileNotFound(filename);
                return;
            }
                        
            try
            {
                var filestream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                var entropy = new byte[16];
                filestream.Read(entropy, 0, 16);
                var cipherBytes = new byte[filestream.Length - 16];
                filestream.Read(cipherBytes, 0, cipherBytes.Length);
                var plainBytes = ProtectedData.Unprotect(
                    cipherBytes, entropy, DataProtectionScope.CurrentUser);
                var serialized = System.Text.Encoding.UTF8.GetString(plainBytes);
                var data = System.Text.Json.JsonSerializer.Deserialize<List<LoginCredential>>(serialized);
                AddRange(data);
            }
            catch (Exception ex)
            {
                _log.ErrorReadingCredentials(filename, ex);
            }
        }

        public void Save()
        {
            _log.SavingCredentials();
            var data = (List<LoginCredential>)this;
            var serialized = System.Text.Json.JsonSerializer.Serialize(data);
            var plainBytes = System.Text.Encoding.UTF8.GetBytes(serialized);
            var entropy = new byte[16];
            new RNGCryptoServiceProvider().GetBytes(entropy);
            var filename = Path.Combine(Application.persistentDataPath, "credentials.dat");
            var filestream = new FileStream(filename, FileMode.Create, FileAccess.Write);
            var cypherBytes = ProtectedData.Protect(
                plainBytes, entropy, DataProtectionScope.CurrentUser);
            filestream.Write(entropy, 0, 16);
            filestream.Write(cypherBytes, 0, cypherBytes.Length);
            filestream.Flush();
            filestream.Close();
            _log.SavedEncryptedCredentials(filename);
        }
    }


}
