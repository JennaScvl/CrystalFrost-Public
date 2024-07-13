//////////////////////////////////////////////////////
// MK Glow Pre Sample								//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2021 All rights reserved.            //
//////////////////////////////////////////////////////

#ifndef MK_GLOW_PRE_SAMPLE
	#define MK_GLOW_PRE_SAMPLE

	#include "../Inc/Common.hlsl"

	UNIFORM_SAMPLER_AND_TEXTURE_2D(_SourceTex)
	#ifndef COMPUTE_SHADER
		uniform float2 _SourceTex_TexelSize;
		uniform half _LumaScale;
	#endif
	#ifdef MK_BLOOM
		#ifdef COMPUTE_SHADER
			UNIFORM_RWTEXTURE_2D(_BloomTargetTex)
		#else
			#ifndef MK_NATURAL
				uniform half2 _BloomThreshold;
			#endif
		#endif
	#endif

	#ifdef MK_COPY
		#ifdef COMPUTE_SHADER
			UNIFORM_RWTEXTURE_2D(_CopyTargetTex)
		#endif
	#endif

	#ifdef MK_LENS_FLARE
		static const float2 UV_HALF = half2(0.5, 0.5);

		UNIFORM_SAMPLER_AND_TEXTURE_2D(_LensFlareColorRamp)
		
		#ifdef COMPUTE_SHADER
			UNIFORM_RWTEXTURE_2D(_LensFlareTargetTex)
		#else
			#ifndef MK_NATURAL
				uniform half2 _LensFlareThreshold;
			#endif
			uniform half4 _LensFlareGhostParams; //count, dispersal, fade, _intensity
			uniform half3 _LensFlareHaloParams; //size, fade, _intensity
		#endif
	#endif

	#ifdef MK_GLARE
		#ifdef COMPUTE_SHADER
			UNIFORM_RWTEXTURE_2D(_Glare0TargetTex)
		#else
			#ifndef MK_NATURAL
				uniform half2 _GlareThreshold;
			#endif
		#endif
	#endif
		
	#ifdef COMPUTE_SHADER
		#define HEADER [numthreads(8,8,1)] void Presample (uint2 id : SV_DispatchThreadID)
	#else
		#define HEADER FragmentOutputAuto frag (VertGeoOutputSimple o)
	#endif
	
	#ifdef MK_LENS_FLARE
		inline float CreateHalo(float2 uv, float2 offset, float2 size, half intensity, half fade)
		{
			return pow(1.0 - length(UV_HALF - frac(uv + offset + size)) / length(UV_HALF), fade) * intensity;
		}
	#endif

	#ifdef UNITY_SINGLE_PASS_STEREO
		#define STEREO_OFFSET float2(0.1875,0)
	#endif

	HEADER
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(o);
		#ifndef COMPUTE_SHADER
			FragmentOutputAuto fO;
			INITIALIZE_STRUCT(FragmentOutputAuto, fO);
		#endif
		
		half4 source = Presample(PASS_TEXTURE_2D(_SourceTex, sampler_linear_clamp_SourceTex), BLOOM_UV, SOURCE_TEXEL_SIZE); //Bloom is always presampled
		
		#ifdef MK_BLOOM
			half4 bloom = source;

			#ifdef MK_NATURAL
				bloom = half4(NaturalRel(bloom.rgb, LUMA_SCALE), 1);
			#else
				bloom = half4(LuminanceThreshold(bloom.rgb, BLOOM_THRESHOLD, LUMA_SCALE), 1);
			#endif
			#ifdef COLORSPACE_GAMMA
				bloom = GammaToLinearSpace4(bloom);
			#endif
			BLOOM_RENDER_TARGET = bloom;
		#endif

		#ifdef MK_COPY
			#ifdef COMPUTE_SHADER
				COPY_RENDER_TARGET = _SourceTex[id];
			#else
				COPY_RENDER_TARGET = LoadTex2D(PASS_TEXTURE_2D(_SourceTex, sampler_linear_clamp_SourceTex), UV_COPY);
			#endif
		#endif

		#ifdef MK_LENS_FLARE
			half4 lensFlare = 0;

			[unroll(5)]
			for (int i = 1; i <= LENS_FLARE_GHOST_COUNT; i++)
			{ 
				float2 offset = frac(LENS_FLARE_UV + (UV_HALF - LENS_FLARE_UV) * LENS_FLARE_GHOST_DISPERSAL * i);

				half weight = pow(1.0 - length(UV_HALF - offset) / length(UV_HALF), LENS_FLARE_GHOST_FADE);

				#ifdef MK_NATURAL
					lensFlare += half4(NaturalRel(LoadTex2D(PASS_TEXTURE_2D(_SourceTex, sampler_linear_clamp_SourceTex), offset).rgb, LUMA_SCALE).rgb * weight, 0) * LENS_FLARE_GHOST_INTENSITY;
				#else
					lensFlare += half4(LuminanceThreshold(LoadTex2D(PASS_TEXTURE_2D(_SourceTex, sampler_linear_clamp_SourceTex), offset).rgb, LENS_FLARE_THRESHOLD, LUMA_SCALE).rgb * weight, 0) * LENS_FLARE_GHOST_INTENSITY;
				#endif
			}
			
			half2 haloSize;
			float weight;
			#ifdef UNITY_SINGLE_PASS_STEREO
				haloSize = normalize(UV_HALF - LENS_FLARE_UV) * LENS_FLARE_HALO_SIZE * 0.5;
				float4 haloSizeSP = float4(normalize(UV_HALF - LENS_FLARE_UV - STEREO_OFFSET),normalize(UV_HALF - LENS_FLARE_UV + STEREO_OFFSET)) * LENS_FLARE_HALO_SIZE;
				weight = CreateHalo(LENS_FLARE_UV, STEREO_OFFSET, haloSizeSP.xy * 0.5, LENS_FLARE_HALO_INTENSITY * 0.5, LENS_FLARE_HALO_FADE * 2.0);
				weight += CreateHalo(LENS_FLARE_UV, -STEREO_OFFSET, haloSizeSP.zw * 0.5, LENS_FLARE_HALO_INTENSITY * 0.5, LENS_FLARE_HALO_FADE * 2.0);
			#else
				haloSize = normalize(UV_HALF - LENS_FLARE_UV) * LENS_FLARE_HALO_SIZE;
				half2 haloSizeSP = normalize(UV_HALF - LENS_FLARE_UV) * LENS_FLARE_HALO_SIZE;
				weight = CreateHalo(LENS_FLARE_UV, 0, haloSizeSP, LENS_FLARE_HALO_INTENSITY, LENS_FLARE_HALO_FADE);
			#endif

			#ifdef MK_NATURAL
				lensFlare += half4(NaturalRel(LoadTex2D(PASS_TEXTURE_2D(_SourceTex, sampler_linear_clamp_SourceTex), LENS_FLARE_UV + haloSize).rgb * weight, LUMA_SCALE), 0);
			#else
				lensFlare += half4(LuminanceThreshold(LoadTex2D(PASS_TEXTURE_2D(_SourceTex, sampler_linear_clamp_SourceTex), LENS_FLARE_UV + haloSize).rgb * weight, LENS_FLARE_THRESHOLD, LUMA_SCALE), 0);
			#endif

			#ifdef UNITY_SINGLE_PASS_STEREO
				lensFlare *= SampleTex2D(PASS_TEXTURE_2D(_LensFlareColorRamp, sampler_linear_clamp_LensFlareColorRamp), abs(length(UV_HALF - LENS_FLARE_UV - STEREO_OFFSET)) / length(UV_HALF));
			#else
				lensFlare *= SampleTex2D(PASS_TEXTURE_2D(_LensFlareColorRamp, sampler_linear_clamp_LensFlareColorRamp), abs(length(UV_HALF - LENS_FLARE_UV)) / length(UV_HALF));
			#endif
			#ifdef COLORSPACE_GAMMA
				lensFlare = GammaToLinearSpace4(lensFlare);
			#endif

			LENS_FLARE_RENDER_TARGET = lensFlare;
		#endif

		#ifdef MK_GLARE
			half4 glare = source;

			#ifdef MK_NATURAL
				glare = half4(NaturalRel(glare.rgb, LUMA_SCALE), 1);
			#else
				glare = half4(LuminanceThreshold(glare.rgb, GLARE_THRESHOLD, LUMA_SCALE), 1);
			#endif
			#ifdef COLORSPACE_GAMMA
				glare = GammaToLinearSpace4(glare);
			#endif

			half screenFade = ScreenFade(SOURCE_UV, 0.75, 0.0);
			#ifdef MK_GLARE_1
				GLARE0_RENDER_TARGET = glare * screenFade;
			#endif
			#ifdef MK_GLARE_2
				GLARE1_RENDER_TARGET = glare * screenFade;
			#endif
			#ifdef MK_GLARE_3
				GLARE2_RENDER_TARGET = glare * screenFade;
			#endif
			#ifdef MK_GLARE_4
				GLARE3_RENDER_TARGET = glare * screenFade;
			#endif
		#endif

		#ifndef COMPUTE_SHADER
			return fO;
		#endif
	}
#endif