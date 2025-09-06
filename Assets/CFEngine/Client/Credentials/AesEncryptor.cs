using System;
using Microsoft.Extensions.Logging;
using OpenMetaverse;
using OpenMetaverse.Assets;
using UnityEngine;
using System.IO;
using System.Text.Json;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Modes;
using System.Security.Cryptography;
using Org.BouncyCastle.Utilities;
using Microsoft.Extensions.Options;
using CrystalFrost.Config;

namespace CrystalFrost.Client.Credentials
{
	public interface IAesEncryptor
	{

		byte[] Encrypt(byte[] data);
		byte[] Decrypt(byte[] encryptedData);
	}

	public class AesEncryptor : IAesEncryptor
	{
		private static readonly System.Random random = new System.Random();
		private const string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
		private readonly ILogger<IAesEncryptor> _log;
		private byte[] _key;

		public AesEncryptor(ILogger<IAesEncryptor> log)
		{
			_log = log;
			_key = LoadKey();
		}

		public byte[] Encrypt(byte[] data)
		{
			using (var aes = Aes.Create())
			{
				aes.Key = this._key;
				aes.Mode = CipherMode.CBC;
				aes.Padding = PaddingMode.PKCS7;
				using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
				{
					using (var ms = new MemoryStream())
					{
						ms.Write(aes.IV, 0, aes.IV.Length);
						using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
						{
							cs.Write(data, 0, data.Length);
							cs.FlushFinalBlock();
						}
						return ms.ToArray();
					}
				}
			}
		}

		public byte[] Decrypt(byte[] encryptedData)
		{
			using (var aes = Aes.Create())
			{
				aes.Key = this._key;
				aes.Mode = CipherMode.CBC;
				aes.Padding = PaddingMode.PKCS7;
				byte[] iv = new byte[aes.BlockSize / 8];
				Array.Copy(encryptedData, iv, iv.Length);
				using (var decryptor = aes.CreateDecryptor(aes.Key, iv))
				{
					using (var ms = new MemoryStream())
					{
						using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
						{
							cs.Write(encryptedData, iv.Length, encryptedData.Length - iv.Length);
							cs.FlushFinalBlock();
						}
						return ms.ToArray();
					}
				}
			}
		}

		/// <summary>
		/// Loads a public from a file or generates a new pair if none exists and generate private key from device info.
		/// </summary>
		/// <returns>A byte array containing both the private and public keys.</returns>
		private byte[] LoadKey()
		{
			var keyFilePath = Path.Combine(Application.persistentDataPath, "key.dat");
			string deviceID = SystemInfo.deviceUniqueIdentifier;
			var privateKey = GenerateKey(deviceID);
			byte[] publicKey = null;
			if (File.Exists(keyFilePath))
			{
				_log.LogDebug("Loading key from file...");
				try
				{
					publicKey = File.ReadAllBytes(keyFilePath);
				}
				catch (Exception ex)
				{
					_log.LogError($"Failed to load key from file: {ex.Message}");
				}
			}

			if (publicKey == null)
			{
				// Generate a new key if the file doesn't exist or loading fails
				_log.LogInformation("Generating new key...");
				string constant = GenerateRandomString(random.Next(32, 64));
				publicKey = GenerateKey(constant);
			}
			byte[] key = new byte[privateKey.Length + publicKey.Length];
			Array.Copy(privateKey, key, privateKey.Length);
			Array.Copy(publicKey, 0, key, privateKey.Length, publicKey.Length);
			SaveKey(publicKey);
			return key;
		}

		private void SaveKey(byte[] key)
		{
			var keyFilePath = Path.Combine(Application.persistentDataPath, "key.dat");
			try
			{
				_log.LogDebug("Saving key to file...");
				File.WriteAllBytes(keyFilePath, key);
			}
			catch (Exception ex)
			{
				_log.LogError($"Failed to save key to file: {ex.Message}");
			}
		}

		public byte[] GenerateKeyDeviceSpecific(string deviceId, string otherInfo)
		{
			string combinedInfo = deviceId + otherInfo;
			byte[] combinedBytes = Encoding.UTF8.GetBytes(combinedInfo);
			int iterations = 10000;
			int derivedKeyLength = 16;
			byte[] salt = {
			0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88,
			0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF, 0x00
		};
			using (var pbkdf2 = new Rfc2898DeriveBytes(combinedBytes, salt, iterations))
			{
				return pbkdf2.GetBytes(derivedKeyLength);
			}
		}


		public byte[] GenerateKey(string info)
		{
			byte[] combinedBytes = Encoding.UTF8.GetBytes(info);
			int iterations = 10000;
			int derivedKeyLength = 16;
			byte[] salt = {
			0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88,
			0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF, 0x00
		};
			using (var pbkdf2 = new Rfc2898DeriveBytes(combinedBytes, salt, iterations))
			{
				return pbkdf2.GetBytes(derivedKeyLength);
			}
		}

		public static string GenerateRandomString(int length)
		{
			StringBuilder stringBuilder = new StringBuilder(length);

			for (int i = 0; i < length; i++)
			{
				stringBuilder.Append(characters[random.Next(characters.Length)]);
			}

			return stringBuilder.ToString();
		}

	}
}