//////////////////////////////////////////////////////
// MK Glow Compatibility	    	    	       	//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2021 All rights reserved.            //
//////////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace MK.Glow
{
	public static class Compatibility
    {
        private static readonly bool _defaultHDRFormatSupported = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.DefaultHDR);
        private static readonly bool _11R11G10BFormatSupported = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RGB111110Float);
        private static readonly bool _2A10R10G10BFormatSupported = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGB2101010);
        //RenderToTexture and a hdr color format required
        public static readonly bool IsSupported = _11R11G10BFormatSupported ? true : _2A10R10G10BFormatSupported ? true : _defaultHDRFormatSupported ? true : false;
        
        /// <summary>
        /// Returns true if the device and used API supports geometry shaders
        /// </summary>
        public static bool CheckGeometryShaderSupport()
        {
            return SystemInfo.graphicsShaderLevel >= 40 && SystemInfo.supportsGeometryShaders;
        }

        /// <summary>
        /// Returns true if the device and used API supports direct compute
        /// </summary>
        public static bool CheckComputeShaderSupport()
        {
            #if UNITY_2017_1_OR_NEWER
                return SystemInfo.supportsComputeShaders && SystemInfo.supportsComputeShaders;
            #else
                //On lower unity versions its impossible to get a temporary RT with randomwrites enabled, so dont allow direct compute
                return false;
            #endif
        }

        /// <summary>
        /// Returns true if the device and used API supports lens flare
        /// </summary>
        /// <returns></returns>
        public static bool CheckLensFlareFeatureSupport()
        {
            return SystemInfo.graphicsShaderLevel >= 35 && SystemInfo.supportedRenderTargetCount >= 2 && !PipelineProperties.singlePassStereoInstancedEnabled;
        }

        /// <summary>
        /// Returns true if the device and used API support glare
        /// </summary>
        /// <returns></returns>
        public static bool CheckGlareFeatureSupport()
        {
            return SystemInfo.graphicsShaderLevel >= 45 && SystemInfo.supportedRenderTargetCount >= 6 && !PipelineProperties.singlePassStereoInstancedEnabled;
        }

        /// <summary>
        /// Returns the supported rendertexture format used for rendering
        /// </summary>
        /// <returns></returns>
        internal static RenderTextureFormat CheckSupportedRenderTextureFormat()
        {
            //return _defaultHDRFormatSupported ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;
            return _11R11G10BFormatSupported ? RenderTextureFormat.RGB111110Float : _2A10R10G10BFormatSupported ? RenderTextureFormat.ARGB2101010 : _defaultHDRFormatSupported ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;
        }
    }
}
