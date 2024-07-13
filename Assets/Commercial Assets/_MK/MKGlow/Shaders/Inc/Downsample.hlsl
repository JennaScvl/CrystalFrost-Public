//////////////////////////////////////////////////////
// MK Glow Downsample		 						//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2021 All rights reserved.            //
//////////////////////////////////////////////////////
#ifndef MK_GLOW_DOWNSAMPLE
	#define MK_GLOW_DOWNSAMPLE

	#include "../Inc/Common.hlsl"

	#ifdef MK_BLOOM
		UNIFORM_SAMPLER_AND_TEXTURE_2D(_BloomTex)
		#ifdef COMPUTE_SHADER
			UNIFORM_RWTEXTURE_2D(_BloomTargetTex)
		#else
			uniform float2 _BloomTex_TexelSize;
		#endif
	#endif

	#ifdef MK_LENS_FLARE
		UNIFORM_SAMPLER_AND_TEXTURE_2D(_LensFlareTex)
		#ifdef COMPUTE_SHADER
			UNIFORM_RWTEXTURE_2D(_LensFlareTargetTex)
		#else
			uniform float2 _LensFlareTex_TexelSize;
		#endif
	#endif

	#ifdef MK_GLARE
		#ifdef COMPUTE_SHADER
			#ifdef MK_GLARE_1
				UNIFORM_RWTEXTURE_2D(_Glare0TargetTex)
			#endif
			#ifdef MK_GLARE_2
				UNIFORM_RWTEXTURE_2D(_Glare1TargetTex)
			#endif
			#ifdef MK_GLARE_3
				UNIFORM_RWTEXTURE_2D(_Glare2TargetTex)
			#endif
			#ifdef MK_GLARE_4
				UNIFORM_RWTEXTURE_2D(_Glare3TargetTex)
			#endif
		#else
			uniform float2 _Glare0Tex_TexelSize;
			uniform float2 _ResolutionScale;

			uniform half4 _GlareScattering; // 0 1 2 3
			uniform half4 _GlareDirection01; // 0 1
			uniform half4 _GlareDirection23; // 2 3
			uniform float4 _GlareOffset; // 0 1 2 3
		#endif

		#ifdef MK_GLARE_1
			UNIFORM_SAMPLER_AND_TEXTURE_2D(_Glare0Tex)
		#endif
		#ifdef MK_GLARE_2
			UNIFORM_SAMPLER_AND_TEXTURE_2D(_Glare1Tex)
		#endif
		#ifdef MK_GLARE_3
			UNIFORM_SAMPLER_AND_TEXTURE_2D(_Glare2Tex)
		#endif
		#ifdef MK_GLARE_4
			UNIFORM_SAMPLER_AND_TEXTURE_2D(_Glare3Tex)
		#endif
	#endif

	#ifdef COMPUTE_SHADER
		#define HEADER [numthreads(8,8,1)] void Downsample (uint2 id : SV_DispatchThreadID)
	#else
		#define HEADER FragmentOutputAuto frag (VertGeoOutputSimple o)
	#endif

	HEADER
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(o);
		#ifndef COMPUTE_SHADER
			FragmentOutputAuto fO;
			INITIALIZE_STRUCT(FragmentOutputAuto, fO);
		#endif

		#ifdef MK_BLOOM
			BLOOM_RENDER_TARGET = DownsampleHQ(PASS_TEXTURE_2D(_BloomTex, sampler_linear_clamp_BloomTex), BLOOM_UV, BLOOM_TEXEL_SIZE);
		#endif

		#ifdef MK_LENS_FLARE
			LENS_FLARE_RENDER_TARGET = DownsampleMQ(PASS_TEXTURE_2D(_LensFlareTex, sampler_linear_clamp_LensFlareTex), LENS_FLARE_UV, LENS_FLARE_TEXEL_SIZE);
		#endif

		#ifdef MK_GLARE
			#ifdef MK_GLARE_1
				GLARE0_RENDER_TARGET = DownsampleLineMQ(PASS_TEXTURE_2D(_Glare0Tex, sampler_linear_clamp_Glare0Tex), GLARE_UV, GLARE0_TEXEL_SIZE * RESOLUTION_SCALE, GLARE0_DIRECTION, GLARE0_OFFSET);
			#endif
			#ifdef MK_GLARE_2
				GLARE1_RENDER_TARGET = DownsampleLineMQ(PASS_TEXTURE_2D(_Glare1Tex, sampler_linear_clamp_Glare1Tex), GLARE_UV, GLARE0_TEXEL_SIZE * RESOLUTION_SCALE, GLARE1_DIRECTION, GLARE1_OFFSET);
			#endif
			#ifdef MK_GLARE_3
				GLARE2_RENDER_TARGET = DownsampleLineMQ(PASS_TEXTURE_2D(_Glare2Tex, sampler_linear_clamp_Glare2Tex), GLARE_UV, GLARE0_TEXEL_SIZE * RESOLUTION_SCALE, GLARE2_DIRECTION, GLARE2_OFFSET);
			#endif
			#ifdef MK_GLARE_4
				GLARE3_RENDER_TARGET = DownsampleLineMQ(PASS_TEXTURE_2D(_Glare3Tex, sampler_linear_clamp_Glare3Tex), GLARE_UV, GLARE0_TEXEL_SIZE * RESOLUTION_SCALE, GLARE3_DIRECTION, GLARE3_OFFSET);
			#endif
		#endif

		#ifndef COMPUTE_SHADER
			return fO;
		#endif
	}
#endif