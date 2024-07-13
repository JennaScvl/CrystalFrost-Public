﻿using System.IO;
using UnityEngine;


namespace CrystalFrost.Config
{
	/// <summary>
	/// Contains configuration that affects the Texture subsystem.
	/// </summary>
    public class TextureConfig
    {
        public const string subsectionName = "Textures";
		//Todo: is better to cache path is grid specific to prevent conflict of same assetID
		private readonly string cachePath = Path.Combine(Application.persistentDataPath, "assettexture");

		public bool isCachingAllowed { get; set; } = true;

		public string getCachePath()
		{
			return cachePath;
		}

		/// <summary>
		/// A limit on the number of textures waiting to be loaded into the GPU.
		/// </summary>
		public int MaxReadyTextures { get; set; } = 5;

		/// <summary>
		/// A Limit on the number of textures waiting be decoded.
		/// Decodeing a texture prepares it to be loaded into the GPU.
		/// </summary>
        public int MaxDownloadedTextures { get; set; } = 5;
    }
}