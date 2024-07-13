//////////////////////////////////////////////////////
// MK Glow Debug									//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2021 All rights reserved.            //
//////////////////////////////////////////////////////

#ifndef MK_GLOW_DEBUG
	#define MK_GLOW_DEBUG

	#include "../Inc/Common.hlsl"

	#ifdef COMPUTE_SHADER
		UNIFORM_RWTEXTURE_2D(_TargetTex)
	#endif
	#if defined(MK_DEBUG_RAW_BLOOM)
		UNIFORM_SAMPLER_AND_TEXTURE_2D(_SourceTex)
		#ifndef COMPUTE_SHADER
			uniform float2 _SourceTex_TexelSize;
			#ifndef MK_NATURAL
				uniform half2 _BloomThreshold;
			#endif
			uniform half _LumaScale;
		#endif
	#elif defined(MK_DEBUG_RAW_LENS_FLARE)
		UNIFORM_SAMPLER_AND_TEXTURE_2D(_SourceTex)
		#ifndef COMPUTE_SHADER
			uniform float2 _SourceTex_TexelSize;
			uniform half2 _LensFlareThreshold;
			uniform half _LumaScale;
		#endif
	#elif defined(MK_DEBUG_RAW_GLARE)
		UNIFORM_SAMPLER_AND_TEXTURE_2D(_SourceTex)
		#ifndef COMPUTE_SHADER
			uniform float2 _SourceTex_TexelSize;
			uniform half2 _GlareThreshold;
			uniform half _LumaScale;
		#endif
	#elif defined(MK_DEBUG_LENS_FLARE)
		#ifndef COMPUTE_SHADER
			uniform float2 _ResolutionScale;
			uniform float _LensFlareChromaticAberration;
			uniform float2 _LensFlareTex_TexelSize;
		#endif
		UNIFORM_SAMPLER_AND_TEXTURE_2D(_LensFlareTex)
	#elif defined(MK_DEBUG_GLARE)
		#ifndef COMPUTE_SHADER
			uniform float2 _ResolutionScale;
			uniform float2 _Glare0Tex_TexelSize;

			uniform half _GlareBlend;
			uniform half _GlareGlobalIntensity;
			uniform half4 _GlareIntensity; // 0 1 2 3
			uniform half4 _GlareScattering; // 0 1 2 3
			uniform half4 _GlareDirection01; // 0 1
			uniform half4 _GlareDirection23; // 2 3
			uniform float4 _GlareOffset; // 0 1 2 3
			uniform float _Blooming;
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
	#elif defined(MK_DEBUG_COMPOSITE)
		UNIFORM_SAMPLER_AND_TEXTURE_2D(_SourceTex)
		UNIFORM_SAMPLER_AND_TEXTURE_2D(_BloomTex)
		#ifndef COMPUTE_SHADER
			uniform float2 _BloomTex_TexelSize;
			uniform half _BloomSpread;
			uniform half _BloomIntensity;
			uniform float _Blooming;
		#endif

		#ifdef MK_LENS_FLARE
			UNIFORM_SAMPLER_AND_TEXTURE_2D(_LensFlareTex)
			#ifndef COMPUTE_SHADER
				uniform float2 _LensFlareTex_TexelSize;
				uniform float _LensFlareChromaticAberration;
			#endif
		#endif

		#ifdef MK_LENS_SURFACE
			#ifndef COMPUTE_SHADER
				uniform half _LensSurfaceDirtIntensity;
				uniform half _LensSurfaceDiffractionIntensity;
				uniform float4 _LensSurfaceDirtTex_ST;
			#endif
			UNIFORM_SAMPLER_AND_TEXTURE_2D_NO_SCALE(_LensSurfaceDirtTex)
			UNIFORM_SAMPLER_AND_TEXTURE_2D_NO_SCALE(_LensSurfaceDiffractionTex)
		#endif

		#if defined(MK_LENS_FLARE) || defined(MK_GLARE)
			#ifndef COMPUTE_SHADER
				uniform float2 _ResolutionScale;
			#endif
		#endif

		#ifdef MK_GLARE
			#ifndef COMPUTE_SHADER
				uniform float2 _Glare0Tex_TexelSize;
				uniform half _GlareBlend;
				uniform half _GlareGlobalIntensity;
				uniform half4 _GlareIntensity; // 0 1 2 3
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
	#else
		#ifndef COMPUTE_SHADER
			uniform float2 _BloomTex_TexelSize;
		#endif
		UNIFORM_SAMPLER_AND_TEXTURE_2D(_BloomTex)
		uniform half _BloomSpread;
		uniform half _BloomIntensity;
		uniform float _Blooming;
	#endif

	#ifndef COMPUTE_SHADER
		#ifdef MK_DEBUG_COMPOSITE
			#if defined(MK_LENS_SURFACE) && defined(MK_LENS_FLARE)
				#define VertGeoOutput VertGeoOutputDouble
				#define MK_LENS_SURFACE_DIFFRACTION_UV uv1.xy
				#define LENS_FLARE_SPREAD uv1.zw
			#elif defined(MK_LENS_SURFACE) || defined(MK_LENS_FLARE)
				#define VertGeoOutput VertGeoOutputPlus
				#define MK_LENS_SURFACE_DIFFRACTION_UV uv1.xy
				#define LENS_FLARE_SPREAD uv1.xy
			#else
				#define VertGeoOutput VertGeoOutputAdvanced
			#endif
		#else
			#if defined(MK_LENS_FLARE) || defined(MK_DEBUG_LENS_FLARE)
				#define LENS_FLARE_SPREAD uv0.zw
				#define VertGeoOutput VertGeoOutputAdvanced
			#else
				#define VertGeoOutput VertGeoOutputAdvanced
			#endif
		#endif

		VertGeoOutput vert (VertexInputOnlyPosition i0)
		{
			VertGeoOutput o;
			UNITY_SETUP_INSTANCE_ID(i0);
			INITIALIZE_STRUCT(VertGeoOutput, o);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

			o.pos = TransformMeshPos(i0.vertex);

			#ifdef MK_LEGACY_BLIT
				o.uv0.xy = i0.texcoord0;
				#if UNITY_UV_STARTS_AT_TOP
					#if defined(MK_DEBUG_RAW_BLOOM) || defined(MK_DEBUG_RAW_LENS_FLARE) || defined(MK_DEBUG_RAW_GLARE)
						if (_SourceTex_TexelSize.y < 0)
							o.uv0.xy = 1-o.uv0.xy;
					#elif defined(MK_DEBUG_LENS_FLARE)
						if (_LensFlareTex_TexelSize.y < 0)
							o.uv0.xy = 1-o.uv0.xy;
					#elif defined(MK_DEBUG_GLARE)
						if (_Glare0Tex_TexelSize.y < 0)
							o.uv0.xy = 1-o.uv0.xy;
					#elif defined(MK_DEBUG_COMPOSITE)
						if (_BloomTex_TexelSize.y < 0)
							o.uv0.xy = 1-o.uv0.xy;
					#else //MK_DEBUG_BLOOM
						if (_BloomTex_TexelSize.y < 0)
							o.uv0.xy = 1-o.uv0.xy;
					#endif
				#endif
			#else
				o.uv0.xy = SetMeshUV(o.pos.xy);
			#endif

			#if defined(MK_DEBUG_BLOOM) || defined(MK_DEBUG_COMPOSITE)
				o.uv0.zw = BLOOM_TEXEL_SIZE * _BloomSpread;
			#endif

			#if defined(MK_DEBUG_COMPOSITE) && defined(MK_LENS_SURFACE)
				o.MK_LENS_SURFACE_DIFFRACTION_UV = LensSurfaceDiffractionUV(o.uv0.xy);
			#endif
			#if defined(MK_LENS_FLARE) && defined(MK_DEBUG_COMPOSITE) || defined(MK_DEBUG_LENS_FLARE)
				o.LENS_FLARE_SPREAD = LENS_FLARE_TEXEL_SIZE * _LensFlareChromaticAberration * _ResolutionScale;
			#endif

			return o;
		}

		#ifdef GEOMETRY_SHADER
			[maxvertexcount(3)]
			void geom(point VertexInputEmpty i0[1], inout TriangleStream<VertGeoOutput> tristream)
			{
				VertGeoOutput o;
				INITIALIZE_STRUCT(VertGeoOutput, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(i0[0], o);

				#if defined(MK_DEBUG_BLOOM) || defined(MK_DEBUG_COMPOSITE)
					float2 bloomSpread = BLOOM_TEXEL_SIZE * _BloomSpread;
				#endif
				#if defined(MK_LENS_FLARE) && defined(MK_DEBUG_COMPOSITE) || defined(MK_DEBUG_LENS_FLARE)
					float2 lensFlareSpread = LENS_FLARE_TEXEL_SIZE * _LensFlareChromaticAberration * _ResolutionScale;
				#endif

				o.pos = TransformMeshPos(SCREEN_VERTICES[0]);
				o.uv0.xy = SetMeshUV(o.pos.xy);
				#if defined(MK_DEBUG_BLOOM) || defined(MK_DEBUG_COMPOSITE)
					o.uv0.zw = bloomSpread;
				#endif
				#if defined(MK_DEBUG_COMPOSITE) && defined(MK_LENS_SURFACE)
					o.MK_LENS_SURFACE_DIFFRACTION_UV = LensSurfaceDiffractionUV(o.uv0.xy);
				#endif
				#if defined(MK_LENS_FLARE) && defined(MK_DEBUG_COMPOSITE) || defined(MK_DEBUG_LENS_FLARE)
					o.LENS_FLARE_SPREAD = lensFlareSpread;
				#endif
				tristream.Append(o);

				o.pos = TransformMeshPos(SCREEN_VERTICES[1]);
				o.uv0.xy = SetMeshUV(o.pos.xy);
				#if defined(MK_DEBUG_BLOOM) || defined(MK_DEBUG_COMPOSITE)
					o.uv0.zw = bloomSpread;
				#endif
				#if defined(MK_DEBUG_COMPOSITE) && defined(MK_LENS_SURFACE)
					o.MK_LENS_SURFACE_DIFFRACTION_UV = LensSurfaceDiffractionUV(o.uv0.xy);
				#endif
				#if defined(MK_LENS_FLARE) && defined(MK_DEBUG_COMPOSITE) || defined(MK_DEBUG_LENS_FLARE)
					o.LENS_FLARE_SPREAD = lensFlareSpread;
				#endif
				tristream.Append(o);

				o.pos = TransformMeshPos(SCREEN_VERTICES[2]);
				o.uv0.xy = SetMeshUV(o.pos.xy);
				#if defined(MK_DEBUG_BLOOM) || defined(MK_DEBUG_COMPOSITE)
					o.uv0.zw = bloomSpread;
				#endif
				#if defined(MK_DEBUG_COMPOSITE) && defined(MK_LENS_SURFACE)
					o.MK_LENS_SURFACE_DIFFRACTION_UV = LensSurfaceDiffractionUV(o.uv0.xy);
				#endif
				#if defined(MK_LENS_FLARE) && defined(MK_DEBUG_COMPOSITE) || defined(MK_DEBUG_LENS_FLARE)
					o.LENS_FLARE_SPREAD = lensFlareSpread;
				#endif
				tristream.Append(o);
			}
		#endif
	#endif

	#if defined(MK_DEBUG_RAW_BLOOM) || defined(MK_DEBUG_RAW_LENS_FLARE) || defined(MK_DEBUG_RAW_GLARE)
		#ifndef COMPUTE_SHADER
			#define HEADER half4 frag (VertGeoOutput o) : SV_Target
		#endif
	#endif
	#ifdef MK_DEBUG_RAW_BLOOM
		#ifdef COMPUTE_SHADER
			#define HEADER [numthreads(8,8,1)] void DebugRawBloom (uint2 id : SV_DispatchThreadID)
		#endif
		HEADER
		{
			UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(o);
			#ifndef MK_NATURAL
				RETURN_TARGET_TEX ConvertToColorSpace(half4(LuminanceThreshold(Presample(PASS_TEXTURE_2D(_SourceTex, sampler_linear_clamp_SourceTex), UV_0, SOURCE_TEXEL_SIZE).rgb, BLOOM_THRESHOLD, LUMA_SCALE), 1));
			#else
				RETURN_TARGET_TEX ConvertToColorSpace(half4(NaturalRel(Presample(PASS_TEXTURE_2D(_SourceTex, sampler_linear_clamp_SourceTex), UV_0, SOURCE_TEXEL_SIZE).rgb, LUMA_SCALE), 1));
			#endif
		}
	#elif defined(MK_DEBUG_RAW_LENS_FLARE)
		#ifdef COMPUTE_SHADER
			#define HEADER [numthreads(8,8,1)] void DebugRawLensFlare (uint2 id : SV_DispatchThreadID)
		#endif
		HEADER
		{
			UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(o);
			#ifdef MK_NATURAL
				RETURN_TARGET_TEX ConvertToColorSpace(half4(NaturalRel(Presample(PASS_TEXTURE_2D(_SourceTex, sampler_linear_clamp_SourceTex), UV_0, SOURCE_TEXEL_SIZE).rgb, LUMA_SCALE), 1));
			#else
				RETURN_TARGET_TEX ConvertToColorSpace(half4(LuminanceThreshold(Presample(PASS_TEXTURE_2D(_SourceTex, sampler_linear_clamp_SourceTex), UV_0, SOURCE_TEXEL_SIZE).rgb, LENS_FLARE_THRESHOLD, LUMA_SCALE), 1));
			#endif
		}
	#elif defined(MK_DEBUG_RAW_GLARE)
		#ifdef COMPUTE_SHADER
			#define HEADER [numthreads(8,8,1)] void DebugRawGlare (uint2 id : SV_DispatchThreadID)
		#endif
		HEADER
		{
			UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(o);
			half screenFade = ScreenFade(UV_0, 0.75, 0.0);
			#ifdef MK_NATURAL
				RETURN_TARGET_TEX ConvertToColorSpace(half4(NaturalRel(Presample(PASS_TEXTURE_2D(_SourceTex, sampler_linear_clamp_SourceTex), UV_0, SOURCE_TEXEL_SIZE).rgb, LUMA_SCALE), 1) * screenFade);
			#else
				RETURN_TARGET_TEX ConvertToColorSpace(half4(LuminanceThreshold(Presample(PASS_TEXTURE_2D(_SourceTex, sampler_linear_clamp_SourceTex), UV_0, SOURCE_TEXEL_SIZE).rgb, GLARE_THRESHOLD, LUMA_SCALE), 1) * screenFade);
			#endif
		}
	#elif defined(MK_DEBUG_GLARE)
		#ifdef COMPUTE_SHADER	
			#define HEADER [numthreads(8,8,1)] void DebugGlare (uint2 id : SV_DispatchThreadID)
		#else
			#define HEADER half4 frag (VertGeoOutputSimple o) : SV_Target
		#endif
		HEADER
		{
			UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(o);
			half4 glare = 0;
			#ifdef MK_GLARE_1
				glare = GaussianBlur1D(PASS_TEXTURE_2D(_Glare0Tex, sampler_linear_clamp_Glare0Tex), UV_0, GLARE0_TEX_TEXEL_SIZE * RESOLUTION_SCALE, GLARE0_SCATTERING, GLARE0_DIRECTION, GLARE0_OFFSET) * GLARE0_INTENSITY;
			#endif
			#ifdef MK_GLARE_2
				glare += GaussianBlur1D(PASS_TEXTURE_2D(_Glare1Tex, sampler_linear_clamp_Glare1Tex), UV_0, GLARE0_TEX_TEXEL_SIZE * RESOLUTION_SCALE, GLARE1_SCATTERING, GLARE1_DIRECTION, GLARE1_OFFSET) * GLARE1_INTENSITY;
			#endif
			#ifdef MK_GLARE_3
				glare += GaussianBlur1D(PASS_TEXTURE_2D(_Glare2Tex, sampler_linear_clamp_Glare2Tex), UV_0, GLARE0_TEX_TEXEL_SIZE * RESOLUTION_SCALE, GLARE2_SCATTERING, GLARE2_DIRECTION, GLARE2_OFFSET) * GLARE2_INTENSITY;
			#endif
			#ifdef MK_GLARE_4
				glare += GaussianBlur1D(PASS_TEXTURE_2D(_Glare3Tex, sampler_linear_clamp_Glare3Tex), UV_0, GLARE0_TEX_TEXEL_SIZE * RESOLUTION_SCALE, GLARE3_SCATTERING, GLARE3_DIRECTION, GLARE3_OFFSET) * GLARE3_INTENSITY;
			#endif

			#ifdef MK_NATURAL
				glare.rgb = max(0, lerp(half3(0,0,0), glare.rgb * 0.25, GLARE_GLOBAL_INTENSITY));
			#else
				glare *= GLARE_GLOBAL_INTENSITY;
			#endif

			glare.rgb = lerp(half3(0,0,0), glare.rgb, GLARE_BLEND);

			#if defined(MK_RENDER_PRIORITY_BALANCED) || defined(MK_RENDER_PRIORITY_QUALITY)
				glare.rgb = Blooming(glare.rgb, BLOOMING);
			#endif

			RETURN_TARGET_TEX ConvertToColorSpace(glare);
		}
	#elif defined(MK_DEBUG_LENS_FLARE)
		#ifdef COMPUTE_SHADER
			#define HEADER [numthreads(8,8,1)] void DebugLensFlare (uint2 id : SV_DispatchThreadID)
		#else
			#define HEADER half4 frag (VertGeoOutput o) : SV_Target
		#endif
		HEADER
		{
			UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(o);
			RETURN_TARGET_TEX ConvertToColorSpace(SampleTex2DCircularChromaticAberration(PASS_TEXTURE_2D(_LensFlareTex, sampler_linear_clamp_LensFlareTex), UV_0, LENS_FLARE_CHROMATIC_ABERRATION));
		}
	#elif defined(MK_DEBUG_COMPOSITE)
		#ifdef COMPUTE_SHADER
			#define HEADER [numthreads(8,8,1)] void DebugComposite (uint2 id : SV_DispatchThreadID)
		#else
			#define HEADER half4 frag (VertGeoOutput o) : SV_Target
		#endif

		HEADER
		{
			#include "CompositeSample.hlsl"
		}
	#else
		#ifdef COMPUTE_SHADER
			#define HEADER [numthreads(8,8,1)] void DebugBloom (uint2 id : SV_DispatchThreadID)
		#else
			#define HEADER half4 frag (VertGeoOutput o) : SV_Target
		#endif
		HEADER
		{
			UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(o);

			#ifdef MK_RENDER_PRIORITY_QUALITY
				half4 g = UpsampleHQ(PASS_TEXTURE_2D(_BloomTex, sampler_linear_clamp_BloomTex), UV_0, BLOOM_TEXEL_SIZE * _BloomSpread);
			#else
				half4 g = SampleTex2D(PASS_TEXTURE_2D(_BloomTex, sampler_linear_clamp_BloomTex), UV_0);
			#endif

			#ifdef MK_NATURAL
				g.rgb = lerp(half3(0,0,0), g.rgb, BLOOM_INTENSITY);
			#else
				g.rgb *= BLOOM_INTENSITY;
			#endif

			g.rgb = Blooming(g.rgb, BLOOMING);

			RETURN_TARGET_TEX ConvertToColorSpace(g);
		}
	#endif
#endif