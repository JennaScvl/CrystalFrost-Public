//////////////////////////////////////////////////////
// MK Glow Upsample									//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2021 All rights reserved.            //
//////////////////////////////////////////////////////

#ifndef MK_GLOW_UPSAMPLE
	#define MK_GLOW_UPSAMPLE

	#include "../Inc/Common.hlsl"

	#ifdef MK_BLOOM
		UNIFORM_SAMPLER_AND_TEXTURE_2D(_BloomTex)
		UNIFORM_SAMPLER_AND_TEXTURE_2D(_HigherMipBloomTex)
		#ifdef COMPUTE_SHADER
			UNIFORM_RWTEXTURE_2D(_BloomTargetTex)
		#else
			uniform float _BloomSpread;
			uniform float2 _BloomTex_TexelSize;
			uniform float2 _HigherMipBloomTex_TexelSize;
		#endif
	#endif

	#ifdef MK_LENS_FLARE
		UNIFORM_SAMPLER_AND_TEXTURE_2D(_LensFlareTex)
		#ifdef COMPUTE_SHADER
			UNIFORM_RWTEXTURE_2D(_LensFlareTargetTex)
		#else
			uniform float2 _LensFlareTex_TexelSize;
			uniform float _LensFlareSpread;
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

	#ifndef COMPUTE_SHADER
		#if defined(MK_BLOOM) && defined(MK_LENS_FLARE)
			#define VERT_GEO_OUTPUT VertGeoOutputPlus
			#define BLOOM_SPREAD uv0.zw
			#define LENS_FLARE_SPREAD uv1.xy
		#elif defined(MK_BLOOM) || defined(MK_LENS_FLARE)
			#define VERT_GEO_OUTPUT VertGeoOutputAdvanced
			#define BLOOM_SPREAD uv0.zw
			#define LENS_FLARE_SPREAD uv0.zw
		#else
			#define VERT_GEO_OUTPUT VertGeoOutputSimple
		#endif

		VERT_GEO_OUTPUT vert (VertexInputOnlyPosition i0)
		{
			VERT_GEO_OUTPUT o;
			UNITY_SETUP_INSTANCE_ID(i0);
			INITIALIZE_STRUCT(VERT_GEO_OUTPUT, o);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

			o.pos = TransformMeshPos(i0.vertex);
			o.uv0.xy = SetMeshUV(i0.vertex.xy);
			
			#ifdef MK_BLOOM
				o.BLOOM_SPREAD = BLOOM_TEXEL_SIZE * _BloomSpread;
			#endif

			#ifdef MK_LENS_FLARE
				o.LENS_FLARE_SPREAD = LENS_FLARE_TEXEL_SIZE * _LensFlareSpread;
			#endif

			return o;
		}

		#ifdef GEOMETRY_SHADER
			[maxvertexcount(3)]
			void geom(point VertexInputEmpty i0[1], inout TriangleStream<VERT_GEO_OUTPUT> tristream)
			{
				VERT_GEO_OUTPUT o;
				INITIALIZE_STRUCT(VERT_GEO_OUTPUT, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(i0[0], o);

				#ifdef MK_BLOOM
					float2 bloomSpread = BLOOM_TEXEL_SIZE * _BloomSpread;
				#endif
				#ifdef MK_LENS_FLARE
					float2 lensFlareSpread = LENS_FLARE_TEXEL_SIZE * _LensFlareSpread;
				#endif

				o.pos = TransformMeshPos(SCREEN_VERTICES[0]);
				o.uv0.xy = SetMeshUV(o.pos.xy);
				#ifdef MK_BLOOM
					o.BLOOM_SPREAD = bloomSpread;
				#endif
				#ifdef MK_LENS_FLARE
					o.LENS_FLARE_SPREAD = lensFlareSpread;
				#endif
				tristream.Append(o);

				o.pos = TransformMeshPos(SCREEN_VERTICES[1]);
				o.uv0.xy = SetMeshUV(o.pos.xy);
				#ifdef MK_BLOOM
					o.BLOOM_SPREAD = bloomSpread;
				#endif
				#ifdef MK_LENS_FLARE
					o.LENS_FLARE_SPREAD = lensFlareSpread;
				#endif
				tristream.Append(o);

				o.pos = TransformMeshPos(SCREEN_VERTICES[2]);
				o.uv0.xy = SetMeshUV(o.pos.xy);
				#ifdef MK_BLOOM
					o.BLOOM_SPREAD = bloomSpread;
				#endif
				#ifdef MK_LENS_FLARE
					o.LENS_FLARE_SPREAD = lensFlareSpread;
				#endif
				tristream.Append(o);
			}
		#endif
	#endif
		
	#ifdef COMPUTE_SHADER
		#define HEADER [numthreads(8,8,1)] void Upsample (uint2 id : SV_DispatchThreadID)
	#else
		#define HEADER FragmentOutputAuto frag (VERT_GEO_OUTPUT o)
	#endif

	HEADER
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(o);
		
		#ifndef COMPUTE_SHADER
			FragmentOutputAuto fO;
			INITIALIZE_STRUCT(FragmentOutputAuto, fO);
		#endif

		#ifdef MK_BLOOM
			half4 bloom = 0;
			
			bloom = UpsampleHQ(PASS_TEXTURE_2D(_BloomTex, sampler_linear_clamp_BloomTex), BLOOM_UV, BLOOM_UPSAMPLE_SPREAD);
			#if defined(MK_RENDER_PRIORITY_QUALITY)
				bloom += half4(SampleTex2DBicubic(PASS_TEXTURE_2D(_HigherMipBloomTex, sampler_linear_clamp_HigherMipBloomTex), BLOOM_UV, HIGHER_MIP_BLOOM_TEXEL_SIZE).rgb, 0);
			#else
				bloom += half4(SampleTex2D(PASS_TEXTURE_2D(_HigherMipBloomTex, sampler_linear_clamp_HigherMipBloomTex), BLOOM_UV).rgb, 0);
			#endif
			BLOOM_RENDER_TARGET = bloom;
		#endif

		#ifdef MK_LENS_FLARE
			LENS_FLARE_RENDER_TARGET = UpsampleMQ(PASS_TEXTURE_2D(_LensFlareTex, sampler_linear_clamp_LensFlareTex), LENS_FLARE_UV, LENS_FLARE_UPSAMPLE_SPREAD);
		#endif
		
		//TODO: move texcoordcalculation to vertexshader
		#ifdef MK_GLARE
			#ifdef MK_GLARE_1
				GLARE0_RENDER_TARGET = GaussianBlur1D(PASS_TEXTURE_2D(_Glare0Tex, sampler_linear_clamp_Glare0Tex), GLARE_UV, GLARE0_TEXEL_SIZE * RESOLUTION_SCALE, GLARE0_SCATTERING, GLARE0_DIRECTION, GLARE0_OFFSET);
			#endif
			#ifdef MK_GLARE_2
				GLARE1_RENDER_TARGET = GaussianBlur1D(PASS_TEXTURE_2D(_Glare1Tex, sampler_linear_clamp_Glare1Tex), GLARE_UV, GLARE0_TEXEL_SIZE * RESOLUTION_SCALE, GLARE1_SCATTERING, GLARE1_DIRECTION, GLARE1_OFFSET);
			#endif
			#ifdef MK_GLARE_3
				GLARE2_RENDER_TARGET = GaussianBlur1D(PASS_TEXTURE_2D(_Glare2Tex, sampler_linear_clamp_Glare2Tex), GLARE_UV, GLARE0_TEXEL_SIZE * RESOLUTION_SCALE, GLARE2_SCATTERING, GLARE2_DIRECTION, GLARE2_OFFSET);
			#endif
			#ifdef MK_GLARE_4
				GLARE3_RENDER_TARGET = GaussianBlur1D(PASS_TEXTURE_2D(_Glare3Tex, sampler_linear_clamp_Glare3Tex), GLARE_UV, GLARE0_TEXEL_SIZE * RESOLUTION_SCALE, GLARE3_SCATTERING, GLARE3_DIRECTION, GLARE3_OFFSET);
			#endif
		#endif

		#ifndef COMPUTE_SHADER
			return fO;
		#endif
	}
#endif