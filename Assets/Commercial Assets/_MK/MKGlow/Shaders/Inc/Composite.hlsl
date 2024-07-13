//////////////////////////////////////////////////////
// MK Glow Composite								//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2021 All rights reserved.            //
//////////////////////////////////////////////////////
#ifndef MK_GLOW_COMPOSITE
	#define MK_GLOW_COMPOSITE
	
	#include "../Inc/Common.hlsl"

	UNIFORM_SAMPLER_AND_TEXTURE_2D(_SourceTex)
	#ifdef COMPUTE_SHADER
		UNIFORM_RWTEXTURE_2D(_TargetTex)
	#else
		uniform float2 _SourceTex_TexelSize;
	#endif

	UNIFORM_SAMPLER_AND_TEXTURE_2D(_BloomTex)
	#ifndef COMPUTE_SHADER
		uniform float2 _BloomTex_TexelSize;
		uniform half _BloomSpread;
		uniform half _BloomIntensity;
		uniform float _Blooming;
	#endif

	#ifdef MK_LENS_FLARE
		UNIFORM_SAMPLER_AND_TEXTURE_2D(_LensFlareTex)
		uniform float2 _LensFlareTex_TexelSize;
		uniform float _LensFlareChromaticAberration;
	#endif

	#ifdef MK_LENS_SURFACE
		UNIFORM_SAMPLER_AND_TEXTURE_2D_NO_SCALE(_LensSurfaceDirtTex)
		UNIFORM_SAMPLER_AND_TEXTURE_2D_NO_SCALE(_LensSurfaceDiffractionTex)
		#ifndef COMPUTE_SHADER
			uniform half _LensSurfaceDirtIntensity;
			uniform half _LensSurfaceDiffractionIntensity;
			uniform float4 _LensSurfaceDirtTex_ST;
		#endif
	#endif

	#if (defined(MK_LENS_FLARE) || defined(MK_GLARE)) && !defined(COMPUTE_SHADER)
		uniform float2 _ResolutionScale;
	#endif

	#ifdef MK_GLARE
		#ifdef MK_GLARE_1
			UNIFORM_SAMPLER_AND_TEXTURE_2D(_Glare0Tex)
			uniform float2 _Glare0Tex_TexelSize;
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

		#ifndef COMPUTE_SHADER
			uniform half _GlareBlend;
			uniform half4 _GlareIntensity; // 0 1 2 3
			uniform half4 _GlareScattering; // 0 1 2 3
			uniform half4 _GlareDirection01; // 0 1
			uniform half4 _GlareDirection23; // 2 3
			uniform float4 _GlareOffset; // 0 1 2 3
			uniform half _GlareGlobalIntensity;
		#endif
	#endif

	#ifndef COMPUTE_SHADER
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
					if (_SourceTex_TexelSize.y < 0)
						o.uv0.xy = 1-o.uv0.xy;
				#endif
			#else
				o.uv0.xy = SetMeshUV(o.pos.xy);
			#endif
			o.uv0.zw = BLOOM_TEXEL_SIZE * _BloomSpread;

			#ifdef MK_LENS_SURFACE
				o.MK_LENS_SURFACE_DIFFRACTION_UV = LensSurfaceDiffractionUV(o.uv0.xy);
			#endif
			#ifdef MK_LENS_FLARE
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

				float2 bloomSpread = BLOOM_TEXEL_SIZE * _BloomSpread;
				#ifdef MK_LENS_FLARE
					float2 lensFlareSpread = LENS_FLARE_TEXEL_SIZE * _LensFlareChromaticAberration * _ResolutionScale;
				#endif

				o.pos = TransformMeshPos(SCREEN_VERTICES[0]);

				o.uv0.xy = SetMeshUV(o.pos.xy);
				o.uv0.zw = bloomSpread;
				#ifdef MK_LENS_SURFACE
					o.MK_LENS_SURFACE_DIFFRACTION_UV = LensSurfaceDiffractionUV(o.uv0.xy);
				#endif
				#ifdef MK_LENS_FLARE
					o.LENS_FLARE_SPREAD = lensFlareSpread;
				#endif
				tristream.Append(o);

				o.pos = TransformMeshPos(SCREEN_VERTICES[1]);
				o.uv0.xy = SetMeshUV(o.pos.xy);
				o.uv0.zw = bloomSpread;
				#ifdef MK_LENS_SURFACE
					o.MK_LENS_SURFACE_DIFFRACTION_UV = LensSurfaceDiffractionUV(o.uv0.xy);
				#endif
				#ifdef MK_LENS_FLARE
					o.LENS_FLARE_SPREAD = lensFlareSpread;
				#endif
				tristream.Append(o);

				o.pos = TransformMeshPos(SCREEN_VERTICES[2]);
				o.uv0.xy = SetMeshUV(o.pos.xy);
				o.uv0.zw = bloomSpread;
				#ifdef MK_LENS_SURFACE
					o.MK_LENS_SURFACE_DIFFRACTION_UV = LensSurfaceDiffractionUV(o.uv0.xy);
				#endif
				#ifdef MK_LENS_FLARE
					o.LENS_FLARE_SPREAD = lensFlareSpread;
				#endif
				tristream.Append(o);
			}
		#endif
	#endif

	#ifdef COMPUTE_SHADER
		#define HEADER [numthreads(8,8,1)] void Composite (uint2 id : SV_DispatchThreadID)
	#else
		#define HEADER half4 frag (VertGeoOutput o) : SV_Target
	#endif

	HEADER
	{
		#include "CompositeSample.hlsl"
	}
#endif