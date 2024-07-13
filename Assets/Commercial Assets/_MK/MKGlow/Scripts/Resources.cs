//////////////////////////////////////////////////////
// MK Glow Resources	    	    	       		//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2021 All rights reserved.            //
//////////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
#if UNITY_EDITOR
using UnityEditor;
#endif
*/

#pragma warning disable
namespace MK.Glow
{
    [System.Serializable]
    /// <summary>
    /// Stores runtime required resources
    /// </summary>
    public sealed class Resources : ScriptableObject
    {        
        internal static void ResourcesNotAvailableWarning()
        {
            Debug.LogWarning("MK Glow resources asset couldn't be found. Effect will be skipped.");
        }

        internal static MK.Glow.Resources LoadResourcesAsset()
        {
            return UnityEngine.Resources.Load<MK.Glow.Resources>("MKGlowResources");
        }

        internal static void UnLoadResourcesAsset(MK.Glow.Resources asset)
        {
            UnityEngine.Resources.UnloadAsset(asset);
        }

        /*
        #if UNITY_EDITOR
        //[MenuItem("Window/MK/Glow/Create Resources Asset")]
        static void CreateAsset()
        {
            Resources asset = ScriptableObject.CreateInstance<Resources>();

            AssetDatabase.CreateAsset(asset, "Assets/_MK/MKGlow/Resources.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = asset;
        }
        #endif
        */

        [SerializeField]
        private Texture2D _lensSurfaceDirtTextureDefault;
        internal Texture2D lensSurfaceDirtTextureDefault { get { return _lensSurfaceDirtTextureDefault; } }
        [SerializeField]
        private Texture2D _lensSurfaceDiffractionTextureDefault;
        internal Texture2D lensSurfaceDiffractionTextureDefault { get { return _lensSurfaceDiffractionTextureDefault; } }
        [SerializeField]
        private Texture2D _lensFlareColorRampDefault;
        internal Texture2D lensFlareColorRampDefault { get { return _lensFlareColorRampDefault; } }

        [SerializeField]
        private Shader _selectiveRenderShader;
        internal Shader selectiveRenderShader { get { return _selectiveRenderShader; } }
        [SerializeField]
        private Shader _sm20Shader;
        internal Shader sm20Shader { get { return _sm20Shader; } }
        [SerializeField]
        private Shader _sm25Shader;
        internal Shader sm25Shader { get { return _sm25Shader; } }
        [SerializeField]
        private Shader _sm35Shader;
        internal Shader sm35Shader { get { return _sm35Shader; } }
        [SerializeField]
        private Shader _sm45Shader;
        internal Shader sm45Shader { get { return _sm45Shader; } }
        
        [SerializeField]
        private Shader _sm40GeometryShader;
        internal Shader sm40GeometryShader { get { return _sm40GeometryShader; } }

        [SerializeField]
        private ComputeShader _computeShader;
        [SerializeField]
        private ComputeShader _computeShaderGles;
        internal ComputeShader computeShader { get { return SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3 ? _computeShaderGles : _computeShader; } }
    }
}