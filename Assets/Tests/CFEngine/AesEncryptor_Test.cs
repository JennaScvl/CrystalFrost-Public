using NUnit.Framework;
using CrystalFrost.Client.Credentials;

namespace CrystalFrostEngine.Tests
{
    public class AesEncryptor_Test
    {
        [Test]
        public void GenerateRandomBytes_AreNotTheSame()
        {
            // Act
            var bytes1 = AesEncryptor.GenerateRandomBytes(32);
            var bytes2 = AesEncryptor.GenerateRandomBytes(32);

            // Assert
            CollectionAssert.AreNotEqual(bytes1, bytes2, "Generated random bytes should not be the same.");
        }
    }
}
