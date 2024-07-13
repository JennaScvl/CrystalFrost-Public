//////////////////////////////////////////////////////
// MK Glow Common									//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2021 All rights reserved.            //
//////////////////////////////////////////////////////

//////////////////////////////////////////////////////
// Keyword matrix                       			//
//////////////////////////////////////////////////////
// _MK_BLOOM             	    | MK_BLOOM
// _MK_LENS_SURFACE      	    | MK_LENS_SURFACE
// _MK_LENS_FLARE        	    | MK_LENS_FLARE                   	
// _MK_GLARE_1             	    | MK_GLARE_1
// _MK_GLARE_2             	    | MK_GLARE_2
// _MK_GLARE_3             	    | MK_GLARE_3
// _MK_GLARE_4             	    | MK_GLARE_4
// _MK_DEBUG_RAW_BLOOM      	| MK_DEBUG_RAW_BLOOM
// _MK_DEBUG_RAW_LENS_FLARE 	| MK_DEBUG_RAW_LENS_FLARE
// _MK_DEBUG_RAW_GLARE      	| MK_DEBUG_RAW_GLARE
// _MK_DEBUG_BLOOM          	| MK_DEBUG_BLOOM
// _MK_DEBUG_LENS_FLARE     	| MK_DEBUG_LENS_FLARE
// _MK_DEBUG_GLARE          	| MK_DEBUG_GLARE
// _MK_COPY              	    | MK_COPY
// _MK_DEBUG_COMPOSITE      	| MK_DEBUG_COMPOSITE
// _MK_LEGACY_BLIT      		| MK_LEGACY_BLIT
// _MK_RENDER_PRIORITY_QUALITY  | MK_RENDER_PRIORITY_QUALITY
// _MK_RENDER_PRIORITY_BALANCED | MK_RENDER_PRIORITY_BALANCED
// _MK_NATURAL                  | MK_NATURAL
// _MK_HQ_ANTI_FLICKER          | MK_HQ_ANTI_FLICKER

//////////////////////////////////////////////////////
// Supported features based on shader model         //
//////////////////////////////////////////////////////
// 2.0  | Bloom, Lens Surface
// 2.5  | Bloom, Lens Surface
// 3.5  | Bloom, Lens Surface, Lens Flare
// 4.5+ | Bloom, Lens Surface, Lens Flare, Glare, Geometry Shaders, Direct Compute

///////////////////////////////////
// Direct Compute Feature Matrix //
///////////////////////////////////
//   2x4  |   3x8	  |   4x16
//0	 --		  ---		  ----
//1	 +-		  +--		  +---
//2	 -+		  -+-		  -+--
//3	 ++		  ++-		  ++--
//4			  --+		  --+-
//5			  +++		  +++-
//6			  -++		  -++-
//7			  +-+		  +-+-
//8						  -+-+
//9 			  		  ---+
//10			  		  --++
//11					  -+++
//12				 	  ++-+
//13					  ++++
//14					  +-++
//15					  +--+

///////////////////////////////
//		CBuffer Inputs		 //
///////////////////////////////
// Index | Buffer | Size
// 0 | _BloomThreshold | 2
// 2 | _LumaScale | 1
// 3 | _BloomSpread | 1
// 4 | _BloomIntensity | 1
// 5 | _Blooming  | 1
// 6 | _LensSurfaceDirtIntensity | 1
// 7 | _LensSurfaceDiffractionIntensity | 1
// 8 | _LensFlareThreshold | 2
// 10 | _LensFlareGhostParams | 4
// 14 | _LensFlareHaloParams | 3
// 17 | _LensFlareSpread | 1
// 18 | _LensFlareChromaticAberration | 1
// 19 | _GlareThreshold | 2
// 21 | _GlareScattering | 4
// 25 | _GlareDirection01 | 4
// 29 | _GlareDirection23 | 4
// 33 | _GlareBlend | 1
// 34 | _GlareIntensity | 4
// 38 | _ResolutionScale | 2
// 40 | _GlareOffset | 4
// 44 | _LensSurfaceDirtTex_ST | 4
// 48 | _GlareGlobalIntensity | 1
// 49 | _ViewMatrix | 16
// 65 | _SinglePassStereoScale | 1
// 66

#ifndef MK_GLOW_COMMON
	#define MK_GLOW_COMMON

	#include "UnityCG.cginc"
	
	/*
	#ifdef _COMPUTE_SHADER
		#define COMPUTE_SHADER
	#else
		uniform half _SinglePassStereoScale;
		#ifdef UNITY_COLORSPACE_GAMMA
			#define COLORSPACE_GAMMA
		#endif
		#ifdef _GEOMETRY_SHADER
			#define GEOMETRY_SHADER
		#endif
	#endif
	*/

	//Somehow on metal api the cross compile code causes compiler issues
	//therefore disable it for now
	#ifdef COMPUTE_SHADER
		#undef COMPUTE_SHADER
	#endif

	uniform half _SinglePassStereoScale;
	#ifdef UNITY_COLORSPACE_GAMMA
		#define COLORSPACE_GAMMA
	#endif
	#ifdef _GEOMETRY_SHADER
		#define GEOMETRY_SHADER
	#endif

	#if defined(_MK_HQ_ANTI_FLICKER) && SHADER_TARGET >= 25
		#ifndef MK_HQ_ANTI_FLICKER
			#define MK_HQ_ANTI_FLICKER
		#endif
	#endif

	uniform float2 _RenderTargetSize;

	#if defined(_HDRP) && SHADER_TARGET >= 35
		#ifndef HDRP
			#define HDRP
		#endif
	#endif

	#ifndef MK_LEGACY_XR_SUPPORT
		#define MK_LEGACY_XR_SUPPORT 1

		#if MK_LEGACY_XR_SUPPORT == 0
			#undef MK_LEGACY_XR_SUPPORT
		#endif
	#endif

	#if defined(_MK_RENDER_PRIORITY_QUALITY) && (defined(COMPUTE_SHADER) || SHADER_TARGET >= 25)
		#define MK_RENDER_PRIORITY_QUALITY
	#elif defined(_MK_RENDER_PRIORITY_BALANCED) && (defined(COMPUTE_SHADER) || SHADER_TARGET >= 25)
		#define MK_RENDER_PRIORITY_BALANCED
	#else
		#define RENDER_PRIORITY_PERFORMANCE
	#endif

	#ifdef _MK_LEGACY_BLIT
		#define MK_LEGACY_BLIT
	#endif	

	#ifdef COMPUTE_SHADER
		uniform StructuredBuffer<float> _CArgBuffer;
	#endif

	#ifdef _MK_PPSV2
		#define MK_PPSV2
	#endif

	/////////////////////////////////////////////////////////////////////////////////////////////
	// Shader Model dependent Macros
	/////////////////////////////////////////////////////////////////////////////////////////////
	#if defined(HDRP) && ((defined(SHADER_API_D3D11) && !defined(SHADER_API_XBOXONE) && !defined(SHADER_API_GAMECORE)) || defined(SHADER_API_PSSL) || defined(SHADER_API_VULKAN)) || !defined(HDRP) && (defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED))
		#ifndef MK_TEXTURE_2D_AS_ARRAY
			#define MK_TEXTURE_2D_AS_ARRAY
		#endif
	#endif
	#if defined(MK_TEXTURE_2D_AS_ARRAY) && defined(MK_PPSV2)
		#undef MK_TEXTURE_2D_AS_ARRAY
	#endif
	#if defined(COMPUTE_SHADER) || SHADER_TARGET >= 35
		#if defined(MK_TEXTURE_2D_AS_ARRAY)
			#define UNIFORM_TEXTURE_2D(textureName) uniform Texture2DArray<half4> textureName;
			#define UNIFORM_SAMPLER_AND_TEXTURE_2D(textureName) uniform Texture2DArray<half4> textureName; uniform SamplerState sampler_linear_clamp##textureName;
			#define DECLARE_TEXTURE_2D_ARGS(textureName, samplerName) Texture2DArray<half4> textureName, SamplerState samplerName
		#else
			#define UNIFORM_TEXTURE_2D(textureName) uniform Texture2D<half4> textureName;
			#define UNIFORM_SAMPLER_AND_TEXTURE_2D(textureName) uniform Texture2D<half4> textureName; uniform SamplerState sampler_linear_clamp##textureName;
			#define DECLARE_TEXTURE_2D_ARGS(textureName, samplerName) Texture2D<half4> textureName, SamplerState samplerName
		#endif

		#define UNIFORM_TEXTURE_2D_NO_SCALE(textureName) uniform Texture2D<half4> textureName;
		#define UNIFORM_SAMPLER_AND_TEXTURE_2D_NO_SCALE(textureName) uniform Texture2D<half4> textureName; uniform SamplerState sampler_linear_clamp##textureName;
		#define DECLARE_TEXTURE_2D_NO_SCALE_ARGS(textureName, samplerName) Texture2D<half4> textureName, SamplerState samplerName

		#define PASS_TEXTURE_2D(textureName, samplerName) textureName, samplerName
	#else
		#define UNIFORM_TEXTURE_2D(textureName) uniform sampler2D textureName;
		#define UNIFORM_SAMPLER_AND_TEXTURE_2D(textureName) uniform sampler2D textureName;
		#define DECLARE_TEXTURE_2D_ARGS(textureName, samplerName) sampler2D textureName

		#define UNIFORM_TEXTURE_2D_NO_SCALE(textureName) UNIFORM_TEXTURE_2D(textureName)
		#define UNIFORM_SAMPLER_AND_TEXTURE_2D_NO_SCALE(textureName) UNIFORM_SAMPLER_AND_TEXTURE_2D(textureName)
		#define DECLARE_TEXTURE_2D_NO_SCALE_ARGS(textureName, samplerName) DECLARE_TEXTURE_2D_ARGS(textureName, samplerName)

		#define PASS_TEXTURE_2D(textureName, samplerName) textureName
	#endif

	#ifdef COMPUTE_SHADER
		#if defined(MK_TEXTURE_2D_AS_ARRAY)
			#define UNIFORM_RWTEXTURE_2D(textureName) uniform RWTexture2DArray<half4> textureName; uniform SamplerState sampler_linear_clamp##textureName;
		#else
			#define UNIFORM_RWTEXTURE_2D(textureName) uniform RWTexture2D<half4> textureName; uniform SamplerState sampler_linear_clamp##textureName;
		#endif
	#endif

	#ifdef UNITY_SINGLE_PASS_STEREO
		static const float4 _DEFAULT_SCALE_TRANSFORM = float4(0.5,1,0,0);
	#else
		static const float4 _DEFAULT_SCALE_TRANSFORM = float4(1,1,0,0);
	#endif

	/////////////////////////////////////////////////////////////////////////////////////////////
	// Cross compile macros direct compute & shader
	/////////////////////////////////////////////////////////////////////////////////////////////
	#ifdef COMPUTE_SHADER
		//Other
		#define SCREEN_SIZE ComputeFloat2FromBuffer(_CArgBuffer, 0)
		#define SINGLE_PASS_STEREO_TEXEL_SCALE ComputeFloatFromBuffer(_CArgBuffer, 65)
		#define SOURCE_TEXEL_SIZE AutoScaleTexelSize(ComputeTexelSize(_SourceTex))
		#define COPY_RENDER_TARGET _CopyTargetTex[id]
		#define RETURN_TARGET_TEX _TargetTex[id] =
		#define SAMPLE_SOURCE _SourceTex[id]
		#define RESOLUTION_SCALE ComputeFloat2FromBuffer(_CArgBuffer, 38)
		#define UV_0 ComputeTexcoord(id, AutoScaleTexelSize(ComputeTexelSize(_TargetTex)))
		#define LUMA_SCALE ComputeFloatFromBuffer(_CArgBuffer, 2)
		#define VIEW_MATRIX ComputeFloat4x4FromBuffer(_CArgBuffer, 49) //needs to be updated once computeshaders use it again

		//Bloom
		#define BLOOM_UV ComputeTexcoord(id, AutoScaleTexelSize(ComputeTexelSize(_BloomTargetTex)))
		#define BLOOM_RENDER_TARGET _BloomTargetTex[id]
		#define BLOOM_THRESHOLD ComputeFloat2FromBuffer(_CArgBuffer, 0)
		#define BLOOM_TEXEL_SIZE AutoScaleTexelSize(ComputeTexelSize(_BloomTex))
		#define HIGHER_MIP_BLOOM_TEXEL_SIZE AutoScaleTexelSize(ComputeTexelSize(_HigherMipBloomTex))
		#define BLOOM_UPSAMPLE_SPREAD AutoScaleTexelSize(ComputeTexelSize(_BloomTex)) * ComputeFloatFromBuffer(_CArgBuffer, 3)
		#define BLOOM_COMPOSITE_SPREAD AutoScaleTexelSize(ComputeTexelSize(_BloomTex)) * ComputeFloatFromBuffer(_CArgBuffer, 3)
		#define BLOOM_INTENSITY ComputeFloatFromBuffer(_CArgBuffer, 4)
		#define BLOOMING ComputeFloatFromBuffer(_CArgBuffer, 5)

		//Lens Surface
		#define LENS_SURFACE_DIRT_INTENSITY ComputeFloatFromBuffer(_CArgBuffer, 6)
		#define LENS_SURFACE_DIFFRACTION_INTENSITY ComputeFloatFromBuffer(_CArgBuffer, 7)
		#define LENS_SURFACE_DIRT_UV UV_0 * ComputeFloat2FromBuffer(_CArgBuffer, 44) + ComputeFloat2FromBuffer(_CArgBuffer, 46)
		#define LENS_DIFFRACTION_UV LensSurfaceDiffractionUV(UV_0)
		
		//Lens Flare
		#define LENS_FLARE_UV ComputeTexcoord(id, AutoScaleTexelSize(ComputeTexelSize(_LensFlareTargetTex)))
		#define LENS_FLARE_RENDER_TARGET _LensFlareTargetTex[id]
		#define LENS_FLARE_THRESHOLD ComputeFloat2FromBuffer(_CArgBuffer, 8)
		#define LENS_FLARE_GHOST_COUNT ComputeFloatFromBuffer(_CArgBuffer, 10)
		#define LENS_FLARE_GHOST_DISPERSAL ComputeFloatFromBuffer(_CArgBuffer, 11)
		#define LENS_FLARE_GHOST_FADE ComputeFloatFromBuffer(_CArgBuffer, 12)
		#define LENS_FLARE_GHOST_INTENSITY ComputeFloatFromBuffer(_CArgBuffer, 13)
		#define LENS_FLARE_HALO_SIZE ComputeFloatFromBuffer(_CArgBuffer, 14)
		#define LENS_FLARE_HALO_FADE ComputeFloatFromBuffer(_CArgBuffer, 15)
		#define LENS_FLARE_HALO_INTENSITY ComputeFloatFromBuffer(_CArgBuffer, 16)
		#define LENS_FLARE_TEXEL_SIZE AutoScaleTexelSize(ComputeTexelSize(_LensFlareTex))
		#define LENS_FLARE_UPSAMPLE_SPREAD LENS_FLARE_TEXEL_SIZE * ComputeFloatFromBuffer(_CArgBuffer, 17)
		#define LENS_FLARE_CHROMATIC_ABERRATION AutoScaleTexelSize(ComputeTexelSize(_LensFlareTex)) * ComputeFloatFromBuffer(_CArgBuffer, 18) * RESOLUTION_SCALE

		//Glare
		#define GLARE_UV ComputeTexcoord(id, AutoScaleTexelSize(ComputeTexelSize(_Glare0TargetTex)))
		#define GLARE0_RENDER_TARGET _Glare0TargetTex[id]
		#define GLARE_THRESHOLD ComputeFloat2FromBuffer(_CArgBuffer, 19)
		#define GLARE0_SCATTERING ComputeFloatFromBuffer(_CArgBuffer, 21)
		#define GLARE1_SCATTERING ComputeFloatFromBuffer(_CArgBuffer, 22)
		#define GLARE2_SCATTERING ComputeFloatFromBuffer(_CArgBuffer, 23)
		#define GLARE3_SCATTERING ComputeFloatFromBuffer(_CArgBuffer, 24)
		#define GLARE0_DIRECTION ComputeFloat2FromBuffer(_CArgBuffer, 25)
		#define GLARE1_DIRECTION ComputeFloat2FromBuffer(_CArgBuffer, 27)
		#define GLARE2_DIRECTION ComputeFloat2FromBuffer(_CArgBuffer, 29)
		#define GLARE3_DIRECTION ComputeFloat2FromBuffer(_CArgBuffer, 31)
		#define GLARE0_OFFSET ComputeFloatFromBuffer(_CArgBuffer, 40)
		#define GLARE1_OFFSET ComputeFloatFromBuffer(_CArgBuffer, 41)
		#define GLARE2_OFFSET ComputeFloatFromBuffer(_CArgBuffer, 42)
		#define GLARE3_OFFSET ComputeFloatFromBuffer(_CArgBuffer, 43)
		#define GLARE0_RENDER_TARGET _Glare0TargetTex[id]
		#define GLARE1_RENDER_TARGET _Glare1TargetTex[id]
		#define GLARE2_RENDER_TARGET _Glare2TargetTex[id]
		#define GLARE3_RENDER_TARGET _Glare3TargetTex[id]
		#define GLARE0_TEXEL_SIZE AutoScaleTexelSize(ComputeTexelSize(_Glare0Tex))
		#define GLARE0_INTENSITY ComputeFloatFromBuffer(_CArgBuffer, 34)
		#define GLARE1_INTENSITY ComputeFloatFromBuffer(_CArgBuffer, 35)
		#define GLARE2_INTENSITY ComputeFloatFromBuffer(_CArgBuffer, 36)
		#define GLARE3_INTENSITY ComputeFloatFromBuffer(_CArgBuffer, 37)
		#define GLARE_BLEND ComputeFloatFromBuffer(_CArgBuffer, 33)
		#define GLARE0_TEX_TEXEL_SIZE AutoScaleTexelSize(ComputeTexelSize(_Glare0Tex))
		#define GLARE_GLOBAL_INTENSITY ComputeFloatFromBuffer(_CArgBuffer, 48)
	#else
		//Other
		#define SCREEN_SIZE _RenderTargetSize
		#define SINGLE_PASS_STEREO_TEXEL_SCALE _SinglePassStereoScale
		#define UV_COPY o.uv0.xy
		#define SOURCE_TEXEL_SIZE AutoScaleTexelSize(_SourceTex_TexelSize)
		#define COPY_RENDER_TARGET fO.GET_COPY_RT
		#define SOURCE_UV o.uv0.xy
		#define RETURN_TARGET_TEX return
		#define SAMPLE_SOURCE LoadTex2D(PASS_TEXTURE_2D(_SourceTex, sampler_linear_clamp_SourceTex), SOURCE_UV, _RenderTargetSize)
		#define RESOLUTION_SCALE _ResolutionScale
		#define UV_0 o.uv0.xy
		#define LUMA_SCALE _LumaScale
		#define VIEW_MATRIX _ViewMatrix
		
		//Bloom
		#define BLOOM_UV o.uv0.xy
		#define BLOOM_RENDER_TARGET fO.GET_BLOOM_RT
		#define BLOOM_THRESHOLD _BloomThreshold
		#define BLOOM_TEXEL_SIZE AutoScaleTexelSize(_BloomTex_TexelSize)
		#define HIGHER_MIP_BLOOM_TEXEL_SIZE AutoScaleTexelSize(_HigherMipBloomTex_TexelSize)
		#define BLOOM_UPSAMPLE_SPREAD o.BLOOM_SPREAD
		#define BLOOM_COMPOSITE_SPREAD o.uv0.zw
		#define BLOOM_INTENSITY _BloomIntensity
		#define BLOOMING _Blooming 

		//Lens Surface
		#define LENS_SURFACE_DIRT_INTENSITY _LensSurfaceDirtIntensity
		#define LENS_SURFACE_DIFFRACTION_INTENSITY _LensSurfaceDiffractionIntensity
		#define LENS_SURFACE_DIRT_UV o.uv0.xy * _LensSurfaceDirtTex_ST.xy + _LensSurfaceDirtTex_ST.zw
		#define LENS_DIFFRACTION_UV o.MK_LENS_SURFACE_DIFFRACTION_UV

		//Lens Flare
		#define LENS_FLARE_UV o.uv0.xy
		#define LENS_FLARE_RENDER_TARGET fO.GET_LENS_FLARE_RT
		#define LENS_FLARE_THRESHOLD _LensFlareThreshold
		#define LENS_FLARE_GHOST_COUNT _LensFlareGhostParams.x
		#define LENS_FLARE_GHOST_DISPERSAL _LensFlareGhostParams.y
		#define LENS_FLARE_GHOST_FADE _LensFlareGhostParams.z
		#define LENS_FLARE_GHOST_INTENSITY _LensFlareGhostParams.w
		#define LENS_FLARE_HALO_SIZE _LensFlareHaloParams.x
		#define LENS_FLARE_HALO_FADE _LensFlareHaloParams.y
		#define LENS_FLARE_HALO_INTENSITY _LensFlareHaloParams.z
		#define LENS_FLARE_TEXEL_SIZE AutoScaleTexelSize(_LensFlareTex_TexelSize)
		#define LENS_FLARE_UPSAMPLE_SPREAD o.LENS_FLARE_SPREAD
		#define LENS_FLARE_CHROMATIC_ABERRATION o.LENS_FLARE_SPREAD

		//Glare
		#define GLARE_UV o.uv0.xy
		#define GLARE0_RENDER_TARGET fO.GET_GLARE0_RT
		#define GLARE_THRESHOLD _GlareThreshold
		#define GLARE0_SCATTERING _GlareScattering.x
		#define GLARE1_SCATTERING _GlareScattering.y
		#define GLARE2_SCATTERING _GlareScattering.z
		#define GLARE3_SCATTERING _GlareScattering.w
		#define GLARE0_DIRECTION _GlareDirection01.xy
		#define GLARE1_DIRECTION _GlareDirection01.zw
		#define GLARE2_DIRECTION _GlareDirection23.xy
		#define GLARE3_DIRECTION _GlareDirection23.zw
		#define GLARE0_OFFSET _GlareOffset.x
		#define GLARE1_OFFSET _GlareOffset.y
		#define GLARE2_OFFSET _GlareOffset.z
		#define GLARE3_OFFSET _GlareOffset.w
		#define GLARE1_RENDER_TARGET fO.GET_GLARE1_RT
		#define GLARE2_RENDER_TARGET fO.GET_GLARE2_RT
		#define GLARE3_RENDER_TARGET fO.GET_GLARE3_RT
		#define GLARE0_TEXEL_SIZE AutoScaleTexelSize(_Glare0Tex_TexelSize)
		#define GLARE_BLEND _GlareBlend
		#define GLARE0_INTENSITY _GlareIntensity.x
		#define GLARE1_INTENSITY _GlareIntensity.y
		#define GLARE2_INTENSITY _GlareIntensity.z
		#define GLARE3_INTENSITY _GlareIntensity.w
		#define GLARE0_TEX_TEXEL_SIZE AutoScaleTexelSize(_Glare0Tex_TexelSize)
		#define GLARE_GLOBAL_INTENSITY _GlareGlobalIntensity
	#endif

	/////////////////////////////////////////////////////////////////////////////////////////////
	// Features
	/////////////////////////////////////////////////////////////////////////////////////////////
	//Bloom
	#ifdef _MK_BLOOM
		#define MK_BLOOM 1
		#define BLOOM_RT 0
	#endif

	#ifdef _MK_NATURAL
		#define MK_NATURAL
	#endif

	//Copy
	#ifdef _MK_COPY
		#define MK_COPY 1
		#define COPY_RT MK_BLOOM
	#endif

	//Lens Surface
	#ifdef _MK_LENS_SURFACE
		#define MK_LENS_SURFACE 1
	#endif

	//Lens Flare
	#if defined(_MK_LENS_FLARE) && (SHADER_TARGET >= 30 || defined(COMPUTE_SHADER))
		#define MK_LENS_FLARE 1
		#define LENS_FLARE_RT MK_BLOOM + MK_COPY
	#endif

	//Glare
	#if (defined(_MK_GLARE_1) || defined(_MK_GLARE_2) || defined(_MK_GLARE_3) || defined(_MK_GLARE_4)) && (SHADER_TARGET >= 35 || defined(COMPUTE_SHADER))
		#ifdef _MK_GLARE_1
			#define MK_GLARE 1
			#define MK_GLARE_1
		#endif
		#ifdef _MK_GLARE_2
			#define MK_GLARE 2
			#define MK_GLARE_1
			#define MK_GLARE_2
		#endif
		#ifdef _MK_GLARE_3
			#define MK_GLARE 3
			#define MK_GLARE_1
			#define MK_GLARE_2
			#define MK_GLARE_3
		#endif
		#ifdef _MK_GLARE_4
			#define MK_GLARE 4
			#define MK_GLARE_1
			#define MK_GLARE_2
			#define MK_GLARE_3
			#define MK_GLARE_4
		#endif
		#define GLARE_RT MK_BLOOM + MK_COPY + MK_LENS_FLARE
	#endif

	//Debug Raw Bloom
	#ifdef _MK_DEBUG_RAW_BLOOM
		#define MK_DEBUG_RAW_BLOOM
	#endif

	//Debug Raw LensFlare
	#ifdef _MK_DEBUG_RAW_LENS_FLARE
		#define MK_DEBUG_RAW_LENS_FLARE
	#endif

	//Debug Raw Glare
	#ifdef _MK_DEBUG_RAW_GLARE
		#define MK_DEBUG_RAW_GLARE
	#endif

	//Debug Bloom
	#ifdef _MK_DEBUG_BLOOM
		#define MK_DEBUG_BLOOM
	#endif

	//Debug LensFlare
	#ifdef _MK_DEBUG_LENS_FLARE
		#define MK_DEBUG_LENS_FLARE
	#endif

	//Debug Glare
	#ifdef _MK_DEBUG_GLARE
		#define MK_DEBUG_GLARE
	#endif

	//Debug Composite
	#ifdef _MK_DEBUG_COMPOSITE
		#define MK_DEBUG_COMPOSITE
	#endif

	#if defined(UNITY_COMPILER_HLSL) || defined(SHADER_API_PSSL) || defined(UNITY_COMPILER_HLSLCC)
		#define INITIALIZE_STRUCT(type, name) name = (type)0;
	#else
		#define INITIALIZE_STRUCT(type, name)
	#endif

	/////////////////////////////////////////////////////////////////////////////////////////////
	// Sampling
	/////////////////////////////////////////////////////////////////////////////////////////////
	static const half3 REL_LUMA = half3(0.2126h, 0.7152h, 0.0722h);
	#define PI 3.14159265
	#define EPSILON 1.0e-4

	#ifdef COMPUTE_SHADER
		inline float ComputeFloatFromBuffer(in StructuredBuffer<float> buffer, in int index)
		{
			return buffer[index];
		}
		inline float2 ComputeFloat2FromBuffer(in StructuredBuffer<float> buffer, in int index)
		{
			return float2(buffer[index], buffer[index + 1]);
		}
		inline float3 ComputeFloat3FromBuffer(in StructuredBuffer<float> buffer, in int index)
		{
			return float3(buffer[index], buffer[index + 1], buffer[index + 2]);
		}
		inline float4 ComputeFloat4FromBuffer(in StructuredBuffer<float> buffer, in int index)
		{
			return float4(buffer[index], buffer[index + 1], buffer[index + 2], buffer[index + 3]);
		}
		inline float4x4 ComputeFloat4x4FromBuffer(in StructuredBuffer<float> buffer, in int index)
		{
			return float4x4
			(
				buffer[index], buffer[index + 1], buffer[index + 2], buffer[index + 3],
				buffer[index + 4], buffer[index + 5], buffer[index + 6], buffer[index + 7],
				buffer[index + 8], buffer[index + 9], buffer[index + 10], buffer[index + 11],
				buffer[index + 12], buffer[index + 13], buffer[index + 14], buffer[index + 15]
			);
		}

		inline float2 ComputeTexelSize(Texture2D<half4> tex)
		{
			uint width, height;
			tex.GetDimensions(width, height);
			return float2(1.0 / width, 1.0 / height);
		}

		inline float2 ComputeTexelSize(RWTexture2D<half4> tex)
		{
			uint width, height;
			tex.GetDimensions(width, height);
			return float2(1.0 / width, 1.0 / height);
		}

		inline float2 ComputeTexcoord(uint2 id, float2 texelSize)
		{
			return texelSize * id + 0.5 * texelSize;
		}
	#endif

	#ifdef HDRP
		inline half4 SampleTex2D(DECLARE_TEXTURE_2D_ARGS(tex, samplerTex), float2 uv)
		{
			return tex.SampleLevel(samplerTex, float3(uv.xy, unity_StereoEyeIndex), 0);
		}

		//Wrap around bicubic sampling - TexelSize unused
		inline half4 SampleTex2D(DECLARE_TEXTURE_2D_ARGS(tex, samplerTex), float2 uv, float2 texelSize)
		{
			return SampleTex2D(PASS_TEXTURE_2D(tex, samplerTex), uv);
		}

		inline half4 LoadTex2D(DECLARE_TEXTURE_2D_ARGS(tex, samplerTex), float2 uv, float2 size)
		{
			return tex.Load(int4(uv * size, unity_StereoEyeIndex, 0));
			//return SampleTex2D(PASS_TEXTURE_2D(tex, samplerTex), uv);
		}
	#else
		inline half4 SampleTex2D(DECLARE_TEXTURE_2D_ARGS(tex, samplerTex), float2 uv)
		{
			#if defined(COMPUTE_SHADER) || SHADER_TARGET >= 35
				#if defined(MK_TEXTURE_2D_AS_ARRAY)
					return tex.Sample(samplerTex, float3((uv).xy, (float)unity_StereoEyeIndex), 0);
				#else
					return tex.Sample(samplerTex, float3(UnityStereoTransformScreenSpaceTex(uv), (float)unity_StereoEyeIndex), 0);
				#endif
			#else
				return tex2D(tex, UnityStereoTransformScreenSpaceTex(uv));
			#endif
		}

		//Wrap around bicubic sampling - TexelSize unused
		inline half4 SampleTex2D(DECLARE_TEXTURE_2D_ARGS(tex, samplerTex), float2 uv, float2 texelSize)
		{
			return SampleTex2D(PASS_TEXTURE_2D(tex, samplerTex), uv);
		}

		inline half4 LoadTex2D(DECLARE_TEXTURE_2D_ARGS(tex, samplerTex), float2 uv, float2 size)
		{
			return SampleTex2D(PASS_TEXTURE_2D(tex, samplerTex), uv);
		}
	#endif

	inline half4 SampleTex2DNoScale(DECLARE_TEXTURE_2D_NO_SCALE_ARGS(tex, samplerTex), float2 uv)
	{
		#if defined(COMPUTE_SHADER) || SHADER_TARGET >= 35
			#if defined(MK_TEXTURE_2D_AS_ARRAY)
				return tex.Sample(samplerTex, float3(uv,0));
			#else
				return tex.Sample(samplerTex, uv);
			#endif
		#else
			return tex2D(tex, uv);
		#endif
	}

	#ifdef HDRP
		inline half4 LoadTex2D(DECLARE_TEXTURE_2D_ARGS(tex, samplerTex), float2 uv)
		{
			#if defined(MK_TEXTURE_2D_AS_ARRAY)
				return tex.Load(int4(UnityStereoTransformScreenSpaceTex(uv) * _RenderTargetSize.xy, unity_StereoEyeIndex, 0));
				//return tex.SampleLevel(samplerTex, float3((uv).xy, (float)unity_StereoEyeIndex), 0);
			#else
				return tex.Load(int4(uv * _RenderTargetSize.xy, unity_StereoEyeIndex, 0));
				//return tex.SampleLevel(samplerTex, float3(UnityStereoTransformScreenSpaceTex(uv).xy, (float)unity_StereoEyeIndex), 0);
			#endif
		}
	#else
		inline half4 LoadTex2D(DECLARE_TEXTURE_2D_ARGS(tex, samplerTex), float2 uv)
		{
			return SampleTex2D(PASS_TEXTURE_2D(tex, samplerTex), uv);
		}
	#endif

	inline float2 AutoScaleTexelSize(float2 texelSize)
	{
		texelSize.x *= SINGLE_PASS_STEREO_TEXEL_SCALE;
		return texelSize;
	}

	inline half4 Cubic(half value)
	{
		half4 n = pow(half4(1.0, 2.0, 3.0, 4.0) - value, 3);
		half4 cubic;
		cubic.x = n.x;
		cubic.y = n.y - 4.0 * n.x;
		cubic.z = n.z - 4.0 * n.y + 6.0 * n.x;
		cubic.w = 6.0 - cubic.x - cubic.y - cubic.z;
		return cubic * (1.0/6.0);
	}

	//Based on: http://www.java-gaming.org/index.php?topic=35123.0
	half4 SampleTex2DBicubic(DECLARE_TEXTURE_2D_ARGS(tex, samplerTex), float2 texCoords, float2 texelSize)
	{
		float2 textureSize = 1.0 / texelSize;
		texCoords = texCoords * textureSize - 0.5;

		float2 fxy = frac(texCoords);
		texCoords -= fxy;

		half4 xcubic = Cubic(fxy.x);
		half4 ycubic = Cubic(fxy.y);

		half4 s = half4(xcubic.xz + xcubic.yw, ycubic.xz + ycubic.yw);
		half4 offset = (texCoords.xxyy + float2(-0.5, + 1.5).xyxy) + half4(xcubic.yw, ycubic.yw) / s;
		offset *= texelSize.xxyy;

		half4 sample0 = SampleTex2D(PASS_TEXTURE_2D(tex, samplerTex), offset.xz);
		half4 sample1 = SampleTex2D(PASS_TEXTURE_2D(tex, samplerTex), offset.yz);
		half4 sample2 = SampleTex2D(PASS_TEXTURE_2D(tex, samplerTex), offset.xw);
		half4 sample3 = SampleTex2D(PASS_TEXTURE_2D(tex, samplerTex), offset.yw);

		half sx = s.x / (s.x + s.y);
		half sy = s.z / (s.z + s.w);

		return lerp(lerp(sample3, sample2, sx), lerp(sample1, sample0, sx), sy);
	}

	half4 LoadTex2DBicubic(DECLARE_TEXTURE_2D_ARGS(tex, samplerTex), float2 texCoords, float2 size, float2 texelSize)
	{
		float2 textureSize = 1.0 / texelSize;
		texCoords = texCoords * textureSize - 0.5;

		float2 fxy = frac(texCoords);
		texCoords -= fxy;

		half4 xcubic = Cubic(fxy.x);
		half4 ycubic = Cubic(fxy.y);

		half4 s = half4(xcubic.xz + xcubic.yw, ycubic.xz + ycubic.yw);
		half4 offset = (texCoords.xxyy + float2(-0.5, + 1.5).xyxy) + half4(xcubic.yw, ycubic.yw) / s;
		offset *= texelSize.xxyy;

		half4 sample0 = LoadTex2D(PASS_TEXTURE_2D(tex, samplerTex), offset.xz, size);
		half4 sample1 = LoadTex2D(PASS_TEXTURE_2D(tex, samplerTex), offset.yz, size);
		half4 sample2 = LoadTex2D(PASS_TEXTURE_2D(tex, samplerTex), offset.xw, size);
		half4 sample3 = LoadTex2D(PASS_TEXTURE_2D(tex, samplerTex), offset.yw, size);

		half sx = s.x / (s.x + s.y);
		half sy = s.z / (s.z + s.w);

		return lerp(lerp(sample3, sample2, sx), lerp(sample1, sample0, sx), sy);
	}

	inline half Gaussian(float x)
	{
		return 1.0f / (2.0 * sqrt(2.0 * PI)) * exp(-(pow(x, 2)) / (2.0f * pow(2.0, 2)));
	}

	inline half3 Blooming(half3 color, half blooming)
	{
		return lerp(color.rgb, 0.5.xxx*(color.rgb+sqrt(color.rgb)), blooming);
	}

	inline half4 ConvertToColorSpace(half4 color)
	{
		#ifdef COLORSPACE_GAMMA
			color.rgb = LinearToGammaSpace(color.rgb);
			return color;
		#else
			return color;
		#endif
	}

	inline half3 LumaScale(half3 color, half scale)
	{
		return color * lerp(0.909.xxx, 1.0.xxx / (1.0.xxx + REL_LUMA), scale);
	}

	inline half3 LuminanceThreshold(half3 c, half2 threshold, half lumaScale)
	{		
		//brightness is defined by the relative luminance combined with the brightest color part to make it nicer to deal with the shader for artists
		//based on unity builtin brightpass thresholding
		//if any color part exceeds a value of 10 (builtin HDR max) then clamp it as a normalized vector to keep the color balance
		c = clamp(c, 0, normalize(c) * threshold.y);
		c = LumaScale(c, lumaScale);
		//half brightness = lerp(max(dot(c.r, REL_LUMA.r), max(dot(c.g, REL_LUMA.g), dot(c.b, REL_LUMA.b))), max(c.r, max(c.g, c.b)), REL_LUMA);
		//picking just the brightest color part isn´t physically correct at all, but gives nices artistic results
		half brightness = max(c.r, max(c.g, c.b));
		//forcing a hard threshold to only extract really bright parts
		half sP = EPSILON;//threshold.x * 0.0 + EPSILON;
		return max(0, c * max(pow(clamp(brightness - threshold.x + sP, 0, 2 * sP), 2) / (4 * sP + EPSILON), brightness - threshold.x) / max(brightness, EPSILON));
	}

	inline half4 GammaToLinearSpace4(half4 color)
	{
		color.rgb = GammaToLinearSpace(color.rgb);
		return color;
	}

	inline half4 LinearToGammaSpace4(half4 color)
	{
		color.rgb = LinearToGammaSpace(color.rgb);
		return color;
	}

	inline half3 NaturalRel(half3 c, half lumaScale)
	{
		return max(0, LumaScale(c, lumaScale));
	}

	inline half ScreenFade(float2 uv, half x, half y)
	{
		return smoothstep(x, y, distance(uv, float2(0.5, 0.5)));
	}

	static const float2 ANTI_FLICKER_DIRECTION = float2(1, -1);
	half4 Presample(DECLARE_TEXTURE_2D_ARGS(tex, samplerTex), float2 uv, float2 texelSize)
	{
		#ifdef MK_HQ_ANTI_FLICKER
			/*
			//Karis tend to remove bright flickering on far distanced objecte very good, however on close distance some kind of stronger flickering is visible
			//For now a bicubic sample does a good job!
			
			half3 sample0 = LoadTex2D(PASS_TEXTURE_2D(tex, samplerTex), uv + texelSize * ANTI_FLICKER_DIRECTION.xx);
			half3 sample1 = LoadTex2D(PASS_TEXTURE_2D(tex, samplerTex), uv + texelSize * ANTI_FLICKER_DIRECTION.xy);
			half3 sample2 = LoadTex2D(PASS_TEXTURE_2D(tex, samplerTex), uv + texelSize * ANTI_FLICKER_DIRECTION.yy);
			half3 sample3 = LoadTex2D(PASS_TEXTURE_2D(tex, samplerTex), uv + texelSize * ANTI_FLICKER_DIRECTION.yx);

			//brightness based averae based on karis luma average
			half weightM = 1.0 / (max(sampleM.r, max(sampleM.g, sampleM.b)) + 1.0);
			half weightBC = 1.0 / (max(sampleBC.r, max(sampleBC.g, sampleBC.b)) + 1.0);
			half weight0 = 1.0 / (max(sample0.r, max(sample0.g, sample0.b)) + 1.0);
			half weight1 = 1.0 / (max(sample1.r, max(sample1.g, sample1.b)) + 1.0);
			half weight2 = 1.0 / (max(sample2.r, max(sample2.g, sample2.b)) + 1.0);
			half weight3 = 1.0 / (max(sample3.r, max(sample3.g, sample3.b)) + 1.0);
			half weightSum = 1.0 / (weight0 + weight1 + weight2 + weight3);

			//return half4((sample0 * weight0 + sample1 * weight1 + sample2 * weight2 + sample3 * weight3) * weightSum, 1);
			*/
			return LoadTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv, _RenderTargetSize, texelSize);
		#else
			return LoadTex2D(PASS_TEXTURE_2D(tex, samplerTex), uv, _RenderTargetSize);
		#endif
	}

	#if defined(MK_RENDER_PRIORITY_QUALITY)
		//For now used lower quality to save performance
		#define GAUSSIAN_BLUR_SAMPLE_1D SampleTex2D
		//#define GAUSSIAN_BLUR_SAMPLE_1D SampleTex2DBicubic
	#else
		#define GAUSSIAN_BLUR_SAMPLE_1D SampleTex2D
	#endif
	inline half4 GaussianBlur1D(DECLARE_TEXTURE_2D_ARGS(tex, samplerTex), float2 uv, float2 texelSize, float blurWidth, half2 direction, float offset)
	{
		half4 color = half4(0,0,0,1);
		float sum = 0;
		float w = 0;

		for(int i0 = 1; i0 <= 3; i0++)
		{
			w = Gaussian(i0);
			sum += w;
			color.rgb += GAUSSIAN_BLUR_SAMPLE_1D(PASS_TEXTURE_2D(tex, samplerTex), uv + blurWidth * direction * i0 * texelSize + blurWidth * direction * texelSize * offset, texelSize) * w;

			w = Gaussian(-i0);
			sum += w;
			color.rgb += GAUSSIAN_BLUR_SAMPLE_1D(PASS_TEXTURE_2D(tex, samplerTex), uv - blurWidth * direction * i0 * texelSize + blurWidth * direction * texelSize * offset, texelSize) * w;
		}

		w = Gaussian(0);
		sum += w;
		color.rgb += GAUSSIAN_BLUR_SAMPLE_1D(PASS_TEXTURE_2D(tex, samplerTex), uv, texelSize).rgb * w;

		color.rgb /= sum;

		return color;
	}

	inline half4 SampleLine(DECLARE_TEXTURE_2D_ARGS(tex, samplerTex), float2 uv, float2 texelSize)
	{
		float3 d = texelSize.xyx * float3(1.0, -1.0, 0);

		float2 uvIn = uv;

		half4 s;
		s.rgb =  SampleTex2D(PASS_TEXTURE_2D(tex, samplerTex), uv + d.xz).rgb;
		s.rgb += SampleTex2D(PASS_TEXTURE_2D(tex, samplerTex), uv + d.yz).rgb;
		s.a = 1;

		return s * 0.5;
	}

	inline half4 SampleBox(DECLARE_TEXTURE_2D_ARGS(tex, samplerTex), float2 uv, float2 texelSize)
	{
		float4 d = texelSize.xyxy * float4(-1.0, -1.0, 1.0, 1.0);

		half4 s;
		s.rgb =  SampleTex2D(PASS_TEXTURE_2D(tex, samplerTex), uv + d.xy).rgb;
		s.rgb += SampleTex2D(PASS_TEXTURE_2D(tex, samplerTex), uv + d.zy).rgb;
		s.rgb += SampleTex2D(PASS_TEXTURE_2D(tex, samplerTex), uv + d.xw).rgb;
		s.rgb += SampleTex2D(PASS_TEXTURE_2D(tex, samplerTex), uv + d.zw).rgb;
		s.a = 1;

		return s * 0.25;
	}

	static const half2 DOWNSAMPLE_LQ_WEIGHT = half2(0.125, 0.03125);
	static const float4 DOWNSAMPLE_LQ_DIRECTION0 = float4(1.0, -1.0, 0.5, -0.5);
	static const float3 DOWNSAMPLE_LQ_DIRECTION1 = float3(1.0, 0.5, 0);
	//0 X 1 X 2
	//X 3 X 4 X
	//5 X 6 X 7
	//X 8 X 9 X
	//0 X 1 X 2
	inline half4 DownsampleMQ(DECLARE_TEXTURE_2D_ARGS(tex, samplerTex), float2 uv, float2 texelSize)
	{	
		#if defined(MK_RENDER_PRIORITY_QUALITY) || defined(MK_RENDER_PRIORITY_BALANCED)
			half3 sample0 = SampleTex2D(PASS_TEXTURE_2D(tex, samplerTex), uv + texelSize * DOWNSAMPLE_LQ_DIRECTION0.yy, texelSize).rgb;
			half3 sample1 = SampleTex2D(PASS_TEXTURE_2D(tex, samplerTex), uv - texelSize * DOWNSAMPLE_LQ_DIRECTION1.zx, texelSize).rgb;
			half3 sample2 = SampleTex2D(PASS_TEXTURE_2D(tex, samplerTex), uv + texelSize * DOWNSAMPLE_LQ_DIRECTION0.xy, texelSize).rgb;
			half3 sample3 = SampleTex2D(PASS_TEXTURE_2D(tex, samplerTex), uv + texelSize * DOWNSAMPLE_LQ_DIRECTION0.ww, texelSize).rgb;
			half3 sample4 = SampleTex2D(PASS_TEXTURE_2D(tex, samplerTex), uv + texelSize * DOWNSAMPLE_LQ_DIRECTION0.zw, texelSize).rgb;
			half3 sample5 = SampleTex2D(PASS_TEXTURE_2D(tex, samplerTex), uv - texelSize * DOWNSAMPLE_LQ_DIRECTION1.xz, texelSize).rgb;
			half3 sample6 = SampleTex2D(PASS_TEXTURE_2D(tex, samplerTex), uv, texelSize).rgb;
			half3 sample7 = SampleTex2D(PASS_TEXTURE_2D(tex, samplerTex), uv + texelSize * DOWNSAMPLE_LQ_DIRECTION1.xz, texelSize).rgb;
			half3 sample8 = SampleTex2D(PASS_TEXTURE_2D(tex, samplerTex), uv + texelSize * DOWNSAMPLE_LQ_DIRECTION0.wz, texelSize).rgb;
			half3 sample9 = SampleTex2D(PASS_TEXTURE_2D(tex, samplerTex), uv + texelSize * DOWNSAMPLE_LQ_DIRECTION0.zz, texelSize).rgb;
			half3 sample10 = SampleTex2D(PASS_TEXTURE_2D(tex, samplerTex), uv + texelSize * DOWNSAMPLE_LQ_DIRECTION0.yx, texelSize).rgb;
			half3 sample11 = SampleTex2D(PASS_TEXTURE_2D(tex, samplerTex), uv + texelSize * DOWNSAMPLE_LQ_DIRECTION1.zx, texelSize).rgb;
			half3 sample12 = SampleTex2D(PASS_TEXTURE_2D(tex, samplerTex), uv + texelSize * DOWNSAMPLE_LQ_DIRECTION0.xx, texelSize).rgb;

			half4 o = half4((sample3 + sample4 + sample8 + sample9) * DOWNSAMPLE_LQ_WEIGHT.x, 1);
			o.rgb += (sample0 + sample1 + sample6 + sample5).rgb * DOWNSAMPLE_LQ_WEIGHT.y;
			o.rgb += (sample1 + sample2 + sample7 + sample6).rgb * DOWNSAMPLE_LQ_WEIGHT.y;
			o.rgb += (sample5 + sample6 + sample11 + sample10).rgb * DOWNSAMPLE_LQ_WEIGHT.y;
			o.rgb += (sample6 + sample7 + sample12 + sample11).rgb * DOWNSAMPLE_LQ_WEIGHT.y;
			return o;
		#else
			return SampleBox(PASS_TEXTURE_2D(tex, samplerTex), uv, texelSize);
			//return SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv, texelSize);
		#endif
	}
	
	#if defined(MK_RENDER_PRIORITY_QUALITY)
		#define DOWNSAMPLE SampleTex2DBicubic
	#else
		#define DOWNSAMPLE SampleTex2D
	#endif
	static const half3 DOWNSAMPLE_HQ_WEIGHT = half3(0.0833333, 0.0208333, 0.0092333);
	//static const float4 DOWNSAMPLE_HQ_DIRECTION0 = float4(1.45, -1.45, 1.0, -1.0);
	//static const float4 DOWNSAMPLE_HQ_DIRECTION1 = float4(1.45, -1.45, 0.5, -0.5);
	//static const float2 DOWNSAMPLE_HQ_DIRECTION2 = float2(1.0, 0);
	// 0 X 1 X 2 X 3
	// X 4 X 5 X 6 X
	// 7 X 8 X 9 X 0
	// X 1 X 2 X 3 X
	// 4 X 5 X 6 X 7
	// X 8 X 9 X 0 X
	// 1 X 2 X 3 X 4
	inline half4 DownsampleHQ(DECLARE_TEXTURE_2D_ARGS(tex, samplerTex), float2 uv, float2 texelSize)
	{	
		#if defined(MK_RENDER_PRIORITY_QUALITY) || defined(MK_RENDER_PRIORITY_BALANCED)
			/*
			half3 sample0 = SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv + texelSize * DOWNSAMPLE_HQ_DIRECTION0.yx, texelSize).rgb;
			half3 sample1 = SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv + texelSize * DOWNSAMPLE_HQ_DIRECTION1.wx, texelSize).rgb;
			half3 sample2 = SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv + texelSize * DOWNSAMPLE_HQ_DIRECTION1.zx, texelSize).rgb;
			half3 sample3 = SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv + texelSize * DOWNSAMPLE_HQ_DIRECTION0.xx, texelSize).rgb;

			half3 sample4 = SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv + texelSize * DOWNSAMPLE_HQ_DIRECTION0.wz, texelSize).rgb;
			half3 sample5 = SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv + texelSize * DOWNSAMPLE_HQ_DIRECTION2.yx, texelSize).rgb;
			half3 sample6 = SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv + texelSize * DOWNSAMPLE_HQ_DIRECTION0.zz, texelSize).rgb;

			half3 sample7 = SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv + texelSize * DOWNSAMPLE_HQ_DIRECTION1.yz, texelSize).rgb;
			half3 sample8 = SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv + texelSize * DOWNSAMPLE_HQ_DIRECTION1.wz, texelSize).rgb;
			half3 sample9 = SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv + texelSize * DOWNSAMPLE_HQ_DIRECTION1.zz, texelSize).rgb;
			half3 sample10 = SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv + texelSize * DOWNSAMPLE_HQ_DIRECTION1.xz, texelSize).rgb;

			half3 sample11 = SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv - texelSize * DOWNSAMPLE_HQ_DIRECTION2.xy, texelSize).rgb;
			half3 sample12 = SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv, texelSize).rgb;
			half3 sample13 = SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv + texelSize * DOWNSAMPLE_HQ_DIRECTION2.xy, texelSize).rgb;

			half3 sample14 = SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv + texelSize * DOWNSAMPLE_HQ_DIRECTION1.yw, texelSize).rgb;
			half3 sample15 = SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv + texelSize * DOWNSAMPLE_HQ_DIRECTION1.ww, texelSize).rgb;
			half3 sample16 = SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv + texelSize * DOWNSAMPLE_HQ_DIRECTION1.zw, texelSize).rgb;
			half3 sample17 = SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv + texelSize * DOWNSAMPLE_HQ_DIRECTION1.xw, texelSize).rgb;

			half3 sample18 = SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv + texelSize * DOWNSAMPLE_HQ_DIRECTION0.ww, texelSize).rgb;
			half3 sample19 = SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv - texelSize * DOWNSAMPLE_HQ_DIRECTION2.yx, texelSize).rgb;
			half3 sample20 = SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv + texelSize * DOWNSAMPLE_HQ_DIRECTION0.zw, texelSize).rgb;

			half3 sample21 = SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv + texelSize * DOWNSAMPLE_HQ_DIRECTION0.yy, texelSize).rgb;
			half3 sample22 = SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv + texelSize * DOWNSAMPLE_HQ_DIRECTION1.wy, texelSize).rgb;
			half3 sample23 = SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv + texelSize * DOWNSAMPLE_HQ_DIRECTION1.zy, texelSize).rgb;
			half3 sample24 = SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv + texelSize * DOWNSAMPLE_HQ_DIRECTION0.xy, texelSize).rgb;

			half4 color = half4((sample8 + sample9 + sample15 + sample16) * DOWNSAMPLE_HQ_WEIGHT.x, 1);

			color.rgb += (sample4 + sample5 + sample11 + sample12) * DOWNSAMPLE_HQ_WEIGHT.y;
			color.rgb += (sample5 + sample6 + sample12 + sample13) * DOWNSAMPLE_HQ_WEIGHT.y;
			color.rgb += (sample11 + sample12 + sample18 + sample19) * DOWNSAMPLE_HQ_WEIGHT.y;
			color.rgb += (sample12 + sample13 + sample19 + sample20) * DOWNSAMPLE_HQ_WEIGHT.y;

			color.rgb += (sample0 + sample1 + sample7 + sample8) * DOWNSAMPLE_HQ_WEIGHT.z;
			color.rgb += (sample1 + sample2 + sample8 + sample9) * DOWNSAMPLE_HQ_WEIGHT.z;
			color.rgb += (sample2 + sample3 + sample9 + sample10) * DOWNSAMPLE_HQ_WEIGHT.z;
			color.rgb += (sample7 + sample8 + sample14 + sample15) * DOWNSAMPLE_HQ_WEIGHT.z;
			color.rgb += (sample8 + sample9 + sample15 + sample16) * DOWNSAMPLE_HQ_WEIGHT.z;
			color.rgb += (sample9 + sample10 + sample16 + sample17) * DOWNSAMPLE_HQ_WEIGHT.z;
			color.rgb += (sample14 + sample15 + sample21 + sample22) * DOWNSAMPLE_HQ_WEIGHT.z;
			color.rgb += (sample15 + sample16 + sample22 + sample23) * DOWNSAMPLE_HQ_WEIGHT.z;
			color.rgb += (sample16 + sample17 + sample23 + sample24) * DOWNSAMPLE_HQ_WEIGHT.z;

			return color;
			*/

			half3 sample0 = DOWNSAMPLE(PASS_TEXTURE_2D(tex, samplerTex), uv + texelSize * DOWNSAMPLE_LQ_DIRECTION0.yy, texelSize).rgb;
			half3 sample1 = DOWNSAMPLE(PASS_TEXTURE_2D(tex, samplerTex), uv - texelSize * DOWNSAMPLE_LQ_DIRECTION1.zx, texelSize).rgb;
			half3 sample2 = DOWNSAMPLE(PASS_TEXTURE_2D(tex, samplerTex), uv + texelSize * DOWNSAMPLE_LQ_DIRECTION0.xy, texelSize).rgb;
			half3 sample3 = DOWNSAMPLE(PASS_TEXTURE_2D(tex, samplerTex), uv + texelSize * DOWNSAMPLE_LQ_DIRECTION0.ww, texelSize).rgb;
			half3 sample4 = DOWNSAMPLE(PASS_TEXTURE_2D(tex, samplerTex), uv + texelSize * DOWNSAMPLE_LQ_DIRECTION0.zw, texelSize).rgb;
			half3 sample5 = DOWNSAMPLE(PASS_TEXTURE_2D(tex, samplerTex), uv - texelSize * DOWNSAMPLE_LQ_DIRECTION1.xz, texelSize).rgb;
			half3 sample6 = DOWNSAMPLE(PASS_TEXTURE_2D(tex, samplerTex), uv, texelSize).rgb;
			half3 sample7 = DOWNSAMPLE(PASS_TEXTURE_2D(tex, samplerTex), uv + texelSize * DOWNSAMPLE_LQ_DIRECTION1.xz, texelSize).rgb;
			half3 sample8 = DOWNSAMPLE(PASS_TEXTURE_2D(tex, samplerTex), uv + texelSize * DOWNSAMPLE_LQ_DIRECTION0.wz, texelSize).rgb;
			half3 sample9 = DOWNSAMPLE(PASS_TEXTURE_2D(tex, samplerTex), uv + texelSize * DOWNSAMPLE_LQ_DIRECTION0.zz, texelSize).rgb;
			half3 sample10 = DOWNSAMPLE(PASS_TEXTURE_2D(tex, samplerTex), uv + texelSize * DOWNSAMPLE_LQ_DIRECTION0.yx, texelSize).rgb;
			half3 sample11 = DOWNSAMPLE(PASS_TEXTURE_2D(tex, samplerTex), uv + texelSize * DOWNSAMPLE_LQ_DIRECTION1.zx, texelSize).rgb;
			half3 sample12 = DOWNSAMPLE(PASS_TEXTURE_2D(tex, samplerTex), uv + texelSize * DOWNSAMPLE_LQ_DIRECTION0.xx, texelSize).rgb;

			half4 o = half4((sample3 + sample4 + sample8 + sample9) * DOWNSAMPLE_LQ_WEIGHT.x, 1);
			o.rgb += (sample0 + sample1 + sample6 + sample5).rgb * DOWNSAMPLE_LQ_WEIGHT.y;
			o.rgb += (sample1 + sample2 + sample7 + sample6).rgb * DOWNSAMPLE_LQ_WEIGHT.y;
			o.rgb += (sample5 + sample6 + sample11 + sample10).rgb * DOWNSAMPLE_LQ_WEIGHT.y;
			o.rgb += (sample6 + sample7 + sample12 + sample11).rgb * DOWNSAMPLE_LQ_WEIGHT.y;

			return o;
		#else
			return SampleBox(PASS_TEXTURE_2D(tex, samplerTex), uv, texelSize);
			//return SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv, texelSize);
		#endif
	}

	static const half DOWNSAMPLE_LINE_LQ_WEIGHT = 0.5;
	static const float3 DOWNSAMPLE_LINE_LQ_DIRECTION0 = float3(1.0, -1.0, 0.0);
	static const float3 DOWNSAMPLE_LINE_LQ_DIRECTION1 = float3(3.0, -3.0, 0.0);
	static const float3 DOWNSAMPLE_LINE_LQ_DIRECTION2 = float3(5.0, -5.0, 0.0);
	//X X X X X X X X X X X
	//0 X 1 X 2 X 3 X 4 X 5
	//X X X X X X X X X X X
	inline half4 DownsampleLineMQ(DECLARE_TEXTURE_2D_ARGS(tex, samplerTex), float2 uv, float2 texelSize, float2 dir, float offset)
	{
		#if defined(MK_RENDER_PRIORITY_QUALITY) || defined(MK_RENDER_PRIORITY_BALANCED)
			half3 sample1 = SampleTex2D(PASS_TEXTURE_2D(tex, samplerTex), uv + dir * offset * texelSize * DOWNSAMPLE_LINE_LQ_DIRECTION1.yz + dir * texelSize * DOWNSAMPLE_LINE_LQ_DIRECTION1.yz, texelSize).rgb;
			half3 sample2 = SampleTex2D(PASS_TEXTURE_2D(tex, samplerTex), uv + dir * offset * texelSize * DOWNSAMPLE_LINE_LQ_DIRECTION0.yz + dir * texelSize * DOWNSAMPLE_LINE_LQ_DIRECTION0.yz, texelSize).rgb;
			half3 sample3 = SampleTex2D(PASS_TEXTURE_2D(tex, samplerTex), uv + dir * offset * texelSize * DOWNSAMPLE_LINE_LQ_DIRECTION0.xz + dir * texelSize * DOWNSAMPLE_LINE_LQ_DIRECTION0.xz, texelSize).rgb;
			half3 sample4 = SampleTex2D(PASS_TEXTURE_2D(tex, samplerTex), uv + dir * offset * texelSize * DOWNSAMPLE_LINE_LQ_DIRECTION1.xz + dir * texelSize * DOWNSAMPLE_LINE_LQ_DIRECTION1.xz, texelSize).rgb;

			half4 o = half4((sample1 + sample2) * DOWNSAMPLE_LINE_LQ_WEIGHT, 1);
			o.rgb += (sample3 + sample4).rgb * DOWNSAMPLE_LINE_LQ_WEIGHT;
			o.rgb *= DOWNSAMPLE_LINE_LQ_WEIGHT;

			return o;
		#else
			return SampleLine(PASS_TEXTURE_2D(tex, samplerTex), uv, texelSize);
			//return SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv, texelSize);
		#endif
	}
	
	#if defined(MK_RENDER_PRIORITY_QUALITY)
		#define DOWNSAMPLE_LINE SampleTex2DBicubic
	#else
		#define DOWNSAMPLE_LINE SampleTex2D
	#endif
	//X X X X X X X X X X X X X X X
	//0 X 1 X 2 X 3 X 4 X 5 X 6 X 7
	//X X X X X X X X X X X X X X X
	inline half4 DownsampleLineHQ(DECLARE_TEXTURE_2D_ARGS(tex, samplerTex), float2 uv, float2 texelSize, float2 dir, float offset)
	{
		#if defined(MK_RENDER_PRIORITY_QUALITY) || defined(MK_RENDER_PRIORITY_BALANCED)
			half3 sample1 = DOWNSAMPLE_LINE(PASS_TEXTURE_2D(tex, samplerTex), uv + dir * offset * texelSize * DOWNSAMPLE_LINE_LQ_DIRECTION1.yz + dir * texelSize * DOWNSAMPLE_LINE_LQ_DIRECTION1.yz, texelSize).rgb;
			half3 sample2 = DOWNSAMPLE_LINE(PASS_TEXTURE_2D(tex, samplerTex), uv + dir * offset * texelSize * DOWNSAMPLE_LINE_LQ_DIRECTION0.yz + dir * texelSize * DOWNSAMPLE_LINE_LQ_DIRECTION0.yz, texelSize).rgb;
			half3 sample3 = DOWNSAMPLE_LINE(PASS_TEXTURE_2D(tex, samplerTex), uv + dir * offset * texelSize * DOWNSAMPLE_LINE_LQ_DIRECTION0.xz + dir * texelSize * DOWNSAMPLE_LINE_LQ_DIRECTION0.xz, texelSize).rgb;
			half3 sample4 = DOWNSAMPLE_LINE(PASS_TEXTURE_2D(tex, samplerTex), uv + dir * offset * texelSize * DOWNSAMPLE_LINE_LQ_DIRECTION1.xz + dir * texelSize * DOWNSAMPLE_LINE_LQ_DIRECTION1.xz, texelSize).rgb;

			half4 o = half4((sample1 + sample2) * DOWNSAMPLE_LINE_LQ_WEIGHT, 1);
			o.rgb += (sample3 + sample4).rgb * DOWNSAMPLE_LINE_LQ_WEIGHT;
			o.rgb *= DOWNSAMPLE_LINE_LQ_WEIGHT;

			return o;
		#else
			return SampleLine(PASS_TEXTURE_2D(tex, samplerTex), uv, texelSize);
			//return SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv, texelSize);
		#endif
	}

	static const half3 UPSAMPLE_LQ_WEIGHT = half3(0.25, 0.125, 0.0625);
	static const float3 UPSAMPLE_LQ_DIRECTION = float3(1, -1, 0);
	//012
	//345
	//678
	inline half4 UpsampleMQ(DECLARE_TEXTURE_2D_ARGS(tex, samplerTex), float2 uv, float2 texelSize)
	{	
		#if defined(MK_RENDER_PRIORITY_QUALITY) || defined(MK_RENDER_PRIORITY_BALANCED)
			half4 s = half4(0,0,0,1);
			s.rgb += SampleTex2D(PASS_TEXTURE_2D(tex, samplerTex), uv, texelSize).rgb * UPSAMPLE_LQ_WEIGHT.x;

			s.rgb += SampleTex2D(PASS_TEXTURE_2D(tex, samplerTex), uv - UPSAMPLE_LQ_DIRECTION.zx * texelSize, texelSize).rgb * UPSAMPLE_LQ_WEIGHT.y;
			s.rgb += SampleTex2D(PASS_TEXTURE_2D(tex, samplerTex), uv - UPSAMPLE_LQ_DIRECTION.xz * texelSize, texelSize).rgb * UPSAMPLE_LQ_WEIGHT.y;
			s.rgb += SampleTex2D(PASS_TEXTURE_2D(tex, samplerTex), uv + UPSAMPLE_LQ_DIRECTION.xz * texelSize, texelSize).rgb * UPSAMPLE_LQ_WEIGHT.y;
			s.rgb += SampleTex2D(PASS_TEXTURE_2D(tex, samplerTex), uv + UPSAMPLE_LQ_DIRECTION.zx * texelSize, texelSize).rgb * UPSAMPLE_LQ_WEIGHT.y;

			s.rgb += SampleTex2D(PASS_TEXTURE_2D(tex, samplerTex), uv - UPSAMPLE_LQ_DIRECTION.xx * texelSize, texelSize).rgb * UPSAMPLE_LQ_WEIGHT.z;
			s.rgb += SampleTex2D(PASS_TEXTURE_2D(tex, samplerTex), uv + UPSAMPLE_LQ_DIRECTION.xy * texelSize, texelSize).rgb * UPSAMPLE_LQ_WEIGHT.z;
			s.rgb += SampleTex2D(PASS_TEXTURE_2D(tex, samplerTex), uv + UPSAMPLE_LQ_DIRECTION.yx * texelSize, texelSize).rgb * UPSAMPLE_LQ_WEIGHT.z;
			s.rgb += SampleTex2D(PASS_TEXTURE_2D(tex, samplerTex), uv + UPSAMPLE_LQ_DIRECTION.xx * texelSize, texelSize).rgb * UPSAMPLE_LQ_WEIGHT.z;

			return s;
		#else
			return SampleBox(PASS_TEXTURE_2D(tex, samplerTex), uv, texelSize);
			//return SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv, texelSize);
		#endif
	}

	#if defined(MK_RENDER_PRIORITY_QUALITY)
		#define UPSAMPLE SampleTex2DBicubic
	#else
		#define UPSAMPLE SampleTex2D
	#endif
	static const half UPSAMPLE_HQ_WEIGHT[5] = {0.16, 0.08, 0.04, 0.02, 0.01};
	//static const float4 UPSAMPLE_HQ_DIRECTION0 = float4(1, -1, 2, -2);
	//static const float3 UPSAMPLE_HQ_DIRECTION1 = float3(2, -2, 0);
	//static const float3 UPSAMPLE_HQ_DIRECTION2 = float3(1, -1, 0);
	//01234
	//56789
	//01234
	//56789
	//01234
	inline half4 UpsampleHQ(DECLARE_TEXTURE_2D_ARGS(tex, samplerTex), float2 uv, float2 texelSize)
	{	
		#if defined(MK_RENDER_PRIORITY_QUALITY) || defined(MK_RENDER_PRIORITY_BALANCED)
			/*
			half4 s = half4(0,0,0,1);
			s.rgb += SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv, texelSize).rgb * UPSAMPLE_HQ_WEIGHT[0];
			
			s.rgb += SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv - UPSAMPLE_HQ_DIRECTION2.zx * texelSize, texelSize).rgb * UPSAMPLE_HQ_WEIGHT[1];
			s.rgb += SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv - UPSAMPLE_HQ_DIRECTION2.xz * texelSize, texelSize).rgb * UPSAMPLE_HQ_WEIGHT[1];
			s.rgb += SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv + UPSAMPLE_HQ_DIRECTION2.xz * texelSize, texelSize).rgb * UPSAMPLE_HQ_WEIGHT[1];
			s.rgb += SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv + UPSAMPLE_HQ_DIRECTION2.zx * texelSize, texelSize).rgb * UPSAMPLE_HQ_WEIGHT[1];

			s.rgb += SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv - UPSAMPLE_HQ_DIRECTION2.xx * texelSize, texelSize).rgb * UPSAMPLE_HQ_WEIGHT[2];
			s.rgb += SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv + UPSAMPLE_HQ_DIRECTION2.xy * texelSize, texelSize).rgb * UPSAMPLE_HQ_WEIGHT[2];
			s.rgb += SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv + UPSAMPLE_HQ_DIRECTION2.yx * texelSize, texelSize).rgb * UPSAMPLE_HQ_WEIGHT[2];
			s.rgb += SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv + UPSAMPLE_HQ_DIRECTION2.xx * texelSize, texelSize).rgb * UPSAMPLE_HQ_WEIGHT[2];
			s.rgb += SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv + UPSAMPLE_HQ_DIRECTION1.zx * texelSize, texelSize).rgb * UPSAMPLE_HQ_WEIGHT[2];
			s.rgb += SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv + UPSAMPLE_HQ_DIRECTION1.xz * texelSize, texelSize).rgb * UPSAMPLE_HQ_WEIGHT[2];
			s.rgb += SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv + UPSAMPLE_HQ_DIRECTION1.zy * texelSize, texelSize).rgb * UPSAMPLE_HQ_WEIGHT[2];
			s.rgb += SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv + UPSAMPLE_HQ_DIRECTION1.yz * texelSize, texelSize).rgb * UPSAMPLE_HQ_WEIGHT[2];

			s.rgb += SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv + UPSAMPLE_HQ_DIRECTION0.wx * texelSize, texelSize).rgb * UPSAMPLE_HQ_WEIGHT[3];
			s.rgb += SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv + UPSAMPLE_HQ_DIRECTION0.yz * texelSize, texelSize).rgb * UPSAMPLE_HQ_WEIGHT[3];
			s.rgb += SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv + UPSAMPLE_HQ_DIRECTION0.xz * texelSize, texelSize).rgb * UPSAMPLE_HQ_WEIGHT[3];
			s.rgb += SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv + UPSAMPLE_HQ_DIRECTION0.zx * texelSize, texelSize).rgb * UPSAMPLE_HQ_WEIGHT[3];
			s.rgb += SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv + UPSAMPLE_HQ_DIRECTION0.zy * texelSize, texelSize).rgb * UPSAMPLE_HQ_WEIGHT[3];
			s.rgb += SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv + UPSAMPLE_HQ_DIRECTION0.xw * texelSize, texelSize).rgb * UPSAMPLE_HQ_WEIGHT[3];
			s.rgb += SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv + UPSAMPLE_HQ_DIRECTION0.yw * texelSize, texelSize).rgb * UPSAMPLE_HQ_WEIGHT[3];
			s.rgb += SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv + UPSAMPLE_HQ_DIRECTION0.wy * texelSize, texelSize).rgb * UPSAMPLE_HQ_WEIGHT[3];

			s.rgb += SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv + UPSAMPLE_HQ_DIRECTION0.wz * texelSize, texelSize).rgb * UPSAMPLE_HQ_WEIGHT[4];
			s.rgb += SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv + UPSAMPLE_HQ_DIRECTION0.zz * texelSize, texelSize).rgb * UPSAMPLE_HQ_WEIGHT[4];
			s.rgb += SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv + UPSAMPLE_HQ_DIRECTION0.zw * texelSize, texelSize).rgb * UPSAMPLE_HQ_WEIGHT[4];
			s.rgb += SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv + UPSAMPLE_HQ_DIRECTION0.ww * texelSize, texelSize).rgb * UPSAMPLE_HQ_WEIGHT[4];
			
			return s;
			*/

			half4 s = half4(0,0,0,1);
			s.rgb += UPSAMPLE(PASS_TEXTURE_2D(tex, samplerTex), uv, texelSize).rgb * UPSAMPLE_LQ_WEIGHT.x;

			s.rgb += UPSAMPLE(PASS_TEXTURE_2D(tex, samplerTex), uv - UPSAMPLE_LQ_DIRECTION.zx * texelSize, texelSize).rgb * UPSAMPLE_LQ_WEIGHT.y;
			s.rgb += UPSAMPLE(PASS_TEXTURE_2D(tex, samplerTex), uv - UPSAMPLE_LQ_DIRECTION.xz * texelSize, texelSize).rgb * UPSAMPLE_LQ_WEIGHT.y;
			s.rgb += UPSAMPLE(PASS_TEXTURE_2D(tex, samplerTex), uv + UPSAMPLE_LQ_DIRECTION.xz * texelSize, texelSize).rgb * UPSAMPLE_LQ_WEIGHT.y;
			s.rgb += UPSAMPLE(PASS_TEXTURE_2D(tex, samplerTex), uv + UPSAMPLE_LQ_DIRECTION.zx * texelSize, texelSize).rgb * UPSAMPLE_LQ_WEIGHT.y;

			s.rgb += UPSAMPLE(PASS_TEXTURE_2D(tex, samplerTex), uv - UPSAMPLE_LQ_DIRECTION.xx * texelSize, texelSize).rgb * UPSAMPLE_LQ_WEIGHT.z;
			s.rgb += UPSAMPLE(PASS_TEXTURE_2D(tex, samplerTex), uv + UPSAMPLE_LQ_DIRECTION.xy * texelSize, texelSize).rgb * UPSAMPLE_LQ_WEIGHT.z;
			s.rgb += UPSAMPLE(PASS_TEXTURE_2D(tex, samplerTex), uv + UPSAMPLE_LQ_DIRECTION.yx * texelSize, texelSize).rgb * UPSAMPLE_LQ_WEIGHT.z;
			s.rgb += UPSAMPLE(PASS_TEXTURE_2D(tex, samplerTex), uv + UPSAMPLE_LQ_DIRECTION.xx * texelSize, texelSize).rgb * UPSAMPLE_LQ_WEIGHT.z;

			return s;
		#else
			return SampleBox(PASS_TEXTURE_2D(tex, samplerTex), uv, texelSize);
			//return SampleTex2DBicubic(PASS_TEXTURE_2D(tex, samplerTex), uv, texelSize);
		#endif
	}

	inline half4 SampleTex2DCircularChromaticAberration(DECLARE_TEXTURE_2D_ARGS(tex, samplerTex), float2 uv, float2 offset)
	{
		float2 uvOffset = normalize(0.5 - uv) * offset;
		return half4(
				SampleTex2D(PASS_TEXTURE_2D(tex, samplerTex), uv - uvOffset).r,
				SampleTex2D(PASS_TEXTURE_2D(tex, samplerTex), uv).g,
				SampleTex2D(PASS_TEXTURE_2D(tex, samplerTex), uv + uvOffset).b,
				1);
	}

	#if defined(MK_LENS_SURFACE)
		#ifndef COMPUTE_SHADER
			//Common view matrix doesn't work by mixing it with legacy blit
			uniform float4x4 _ViewMatrix;
		#endif

		static half3x3 LensSurfaceDiffractionScale0 = half3x3
		(
			//X and Y of scale matrix has to be doubled to get correct pivot
			2, 0, -1,
			0, 2, -1,
			0, 0,  1
		);

		static half3x3 LensSurfaceDiffractionScale1 = half3x3
		(
			0.5, 0, 0.5,
			0, 0.5, 0.5,
			0, 0, 1
		);

		inline float2 LensSurfaceDiffractionUV(float2 uv)
		{
			float rotationView = dot(float3(VIEW_MATRIX._m00, VIEW_MATRIX._m10, VIEW_MATRIX._m20), float3(0,0,1)) + dot(float3(VIEW_MATRIX._m01, VIEW_MATRIX._m11, VIEW_MATRIX._m21), float3(0,1,0));
			float3x3 rotation = float3x3(
				cos(rotationView), -sin(rotationView), 0,
				sin(rotationView), cos(rotationView),  0,
				0, 0, 1
			);

			rotation = mul(mul(LensSurfaceDiffractionScale1, rotation), LensSurfaceDiffractionScale0);
			return mul(rotation, float3(uv, 1.0)).xy;
		}
	#endif

	/////////////////////////////////////////////////////////////////////////////////////////////
	// Default Shader Includes
	/////////////////////////////////////////////////////////////////////////////////////////////
	#ifndef COMPUTE_SHADER
		const static float4 SCREEN_VERTICES[3] = 
		{
			float4(-1.0, -1.0, 0.0, 1.0),
			float4(3.0, -1.0, 0.0, 1.0),
			float4(-1.0, 3.0, 0.0, 1.0)
		};

		/////////////////////////////////////////////////////////////////////////////////////////////
		// Helpers
		/////////////////////////////////////////////////////////////////////////////////////////////
		inline float4 TransformMeshPos(float4 pos)
		{
			#ifdef MK_LEGACY_BLIT
				return UnityObjectToClipPos(pos);
			#else
				return float4(pos.xy, 0.0, 1.0);
			#endif
		}

		inline float2 SetMeshUV(float2 vertex)
		{
			float2 uv = (vertex + 1.0) * 0.5;
			#ifdef UNITY_UV_STARTS_AT_TOP
				uv = uv * float2(1.0, -1.0) + float2(0.0, 1.0);
			#endif
			return uv;
		}

		/////////////////////////////////////////////////////////////////////////////////////////////
		// In / Out Structs
		/////////////////////////////////////////////////////////////////////////////////////////////
		struct VertexInputOnlyPosition
		{
			float4 vertex : POSITION;
			#ifdef MK_LEGACY_BLIT
				float2 texcoord0 : TEXCOORD0;
			#endif
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		#ifdef GEOMETRY_SHADER
			struct VertexInputEmpty 
			{
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
		#endif

		struct VertGeoOutputSimple
		{
			float4 pos : SV_POSITION;
			float2 uv0 : TEXCOORD0;
			UNITY_VERTEX_OUTPUT_STEREO
		};

		struct VertGeoOutputAdvanced
		{
			float4 pos : SV_POSITION;
			float4 uv0 : TEXCOORD0;
			UNITY_VERTEX_OUTPUT_STEREO
		};

		struct VertGeoOutputPlus
		{
			float4 pos : SV_POSITION;
			float4 uv0 : TEXCOORD0;
			float2 uv1 : TEXCOORD1;
			UNITY_VERTEX_OUTPUT_STEREO
		};

		struct VertGeoOutputDouble
		{
			float4 pos : SV_POSITION;
			float4 uv0 : TEXCOORD0;
			float4 uv1 : TEXCOORD1;
			UNITY_VERTEX_OUTPUT_STEREO
		};

		/////////////////////////////////////////////////////////////////////////////////////////////
		// Vertex
		/////////////////////////////////////////////////////////////////////////////////////////////
		#ifdef GEOMETRY_SHADER
			VertexInputEmpty vertEmpty(VertexInputEmpty i0)
			{
				VertexInputEmpty o;
				UNITY_SETUP_INSTANCE_ID(i0);
				UNITY_TRANSFER_INSTANCE_ID(i0, o);
				INITIALIZE_STRUCT(VertexInputEmpty, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				return o;
			}
		#endif

		VertGeoOutputSimple vertSimple (VertexInputOnlyPosition i0)
		{
			VertGeoOutputSimple o;

			UNITY_SETUP_INSTANCE_ID(i0);
			INITIALIZE_STRUCT(VertGeoOutputSimple, o);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

			o.pos = TransformMeshPos(i0.vertex);
			o.uv0 = SetMeshUV(i0.vertex.xy);
			return o;
		}

		/////////////////////////////////////////////////////////////////////////////////////////////
		// Geometry
		/////////////////////////////////////////////////////////////////////////////////////////////
		#ifdef GEOMETRY_SHADER
			[maxvertexcount(3)]
			void geomSimple(point VertexInputEmpty i0[1], inout TriangleStream<VertGeoOutputSimple> tristream)
			{
				VertGeoOutputSimple o;
				INITIALIZE_STRUCT(VertGeoOutputSimple, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(i0[0], o);

				o.pos = TransformMeshPos(SCREEN_VERTICES[0]);
				o.uv0 = SetMeshUV(o.pos.xy);
				tristream.Append(o);

				o.pos = TransformMeshPos(SCREEN_VERTICES[1]);
				o.uv0 = SetMeshUV(o.pos.xy);
				tristream.Append(o);

				o.pos = TransformMeshPos(SCREEN_VERTICES[2]);
				o.uv0 = SetMeshUV(o.pos.xy);
				tristream.Append(o);
			}
		#endif

		/////////////////////////////////////////////////////////////////////////////////////////////
		// Fragment Output
		/////////////////////////////////////////////////////////////////////////////////////////////
		#define COUNT_ENABLED_TARGETS MK_BLOOM + MK_COPY + MK_LENS_FLARE + MK_GLARE

		#define RENDER_TARGET(target) half4 rt##target : SV_Target##target;
		#define GET_RT(index) rt##index

		#if BLOOM_RT == 0
			#define GET_BLOOM_RT GET_RT(0)
		#endif

		#if COPY_RT == 0
			#define GET_COPY_RT GET_RT(0)
		#elif COPY_RT == 1
			#define GET_COPY_RT GET_RT(1)
		#endif

		#if LENS_FLARE_RT == 0
			#define GET_LENS_FLARE_RT GET_RT(0)
		#elif LENS_FLARE_RT == 1
			#define GET_LENS_FLARE_RT GET_RT(1)
		#elif LENS_FLARE_RT == 2
			#define GET_LENS_FLARE_RT GET_RT(2)
		#endif

		#if GLARE_RT == 0
			#define GET_GLARE0_RT GET_RT(0)
			#define GET_GLARE1_RT GET_RT(1)
			#define GET_GLARE2_RT GET_RT(2)
			#define GET_GLARE3_RT GET_RT(3)
		#elif GLARE_RT == 1
			#define GET_GLARE0_RT rt1
			#define GET_GLARE1_RT GET_RT(2)
			#define GET_GLARE2_RT GET_RT(3)
			#define GET_GLARE3_RT GET_RT(4)
		#elif GLARE_RT == 2
			#define GET_GLARE0_RT GET_RT(2)
			#define GET_GLARE1_RT GET_RT(3)
			#define GET_GLARE2_RT GET_RT(4)
			#define GET_GLARE3_RT GET_RT(5)
		#elif GLARE_RT == 3
			#define GET_GLARE0_RT GET_RT(3)
			#define GET_GLARE1_RT GET_RT(4)
			#define GET_GLARE2_RT GET_RT(5)
			#define GET_GLARE3_RT GET_RT(6)
		#endif
		
		struct FragmentOutputAuto
		{	
			#if COUNT_ENABLED_TARGETS == 2
				RENDER_TARGET(0)
				RENDER_TARGET(1)
			#elif COUNT_ENABLED_TARGETS == 3
				RENDER_TARGET(0)
				RENDER_TARGET(1)
				RENDER_TARGET(2)
			#elif COUNT_ENABLED_TARGETS == 4
				RENDER_TARGET(0)
				RENDER_TARGET(1)
				RENDER_TARGET(2)
				RENDER_TARGET(3)
			#elif COUNT_ENABLED_TARGETS == 5
				RENDER_TARGET(0)
				RENDER_TARGET(1)
				RENDER_TARGET(2)
				RENDER_TARGET(3)
				RENDER_TARGET(4)
			#elif COUNT_ENABLED_TARGETS == 6
				RENDER_TARGET(0)
				RENDER_TARGET(1)
				RENDER_TARGET(2)
				RENDER_TARGET(3)
				RENDER_TARGET(4)
				RENDER_TARGET(5)
			#else
				RENDER_TARGET(0)
			#endif
		};
	#endif
#endif