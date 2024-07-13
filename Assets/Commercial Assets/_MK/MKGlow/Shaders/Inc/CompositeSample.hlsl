 //////////////////////////////////////////////////////
// MK Glow Composite Sample							//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2021 All rights reserved.            //
//////////////////////////////////////////////////////
#ifndef MK_GLOW_COMPOSITE_SAMPLE
	#define MK_GLOW_COMPOSITE_SAMPLE
	
	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(o);
	
	#ifndef COMPUTE_SHADER
		FragmentOutputAuto fO;
		INITIALIZE_STRUCT(FragmentOutputAuto, fO);
	#endif

	half4 g = SampleTex2D(PASS_TEXTURE_2D(_BloomTex, sampler_linear_clamp_BloomTex), UV_0);
	//g = 0;
	
	#if defined(MK_LENS_SURFACE) && defined(MK_NATURAL)
		half4 gs = g;
	#endif

	half4 source = SAMPLE_SOURCE;
	g.a = source.a;
	#ifdef COLORSPACE_GAMMA
		half3 src = GammaToLinearSpace(source.rgb);
	#else
		half3 src = source.rgb;
	#endif

	//return SAMPLE_SOURCE + lerp(half4(0.1,0,0,0), half4(0,0.1,0,0), unity_StereoEyeIndex);
	//return lerp(half4(0.1,0,0,0), half4(0,0.1,0,0), unity_StereoEyeIndex);

	#ifdef MK_GLOW_DEBUG
		src = 0;
	#endif

	#ifdef MK_NATURAL
		g.rgb = lerp(src, g.rgb, BLOOM_INTENSITY);
	#else
		g.rgb *= BLOOM_INTENSITY;
		#if defined(MK_RENDER_PRIORITY_BALANCED) || defined(MK_RENDER_PRIORITY_QUALITY)
			g.rgb = Blooming(g.rgb, BLOOMING);
		#endif
	#endif

	#ifdef MK_GLARE
		half4 glare = 0;
		#ifdef MK_GLARE_1
			glare = SampleTex2D(PASS_TEXTURE_2D(_Glare0Tex, sampler_linear_clamp_Glare0Tex), UV_0) * GLARE0_INTENSITY;
		#endif
		#ifdef MK_GLARE_2
			glare += SampleTex2D(PASS_TEXTURE_2D(_Glare1Tex, sampler_linear_clamp_Glare1Tex), UV_0) * GLARE1_INTENSITY;
		#endif
		#ifdef MK_GLARE_3
			glare += SampleTex2D(PASS_TEXTURE_2D(_Glare2Tex, sampler_linear_clamp_Glare2Tex), UV_0) * GLARE2_INTENSITY;
		#endif
		#ifdef MK_GLARE_4
			glare += SampleTex2D(PASS_TEXTURE_2D(_Glare3Tex, sampler_linear_clamp_Glare3Tex), UV_0) * GLARE3_INTENSITY;
		#endif
		#ifdef MK_NATURAL
			glare.rgb = max(0, lerp(src.rgb, glare.rgb * 0.25, GLARE_GLOBAL_INTENSITY));
		#else
			glare *= GLARE_GLOBAL_INTENSITY;
		#endif

		g.rgb = max(0, lerp(g.rgb, glare.rgb, GLARE_BLEND));
	#endif

	#ifdef MK_LENS_FLARE
		g.rgb += SampleTex2DCircularChromaticAberration(PASS_TEXTURE_2D(_LensFlareTex, sampler_linear_clamp_LensFlareTex), UV_0, LENS_FLARE_CHROMATIC_ABERRATION).rgb;
	#endif

	#ifdef MK_LENS_SURFACE
		half3 dirt = SampleTex2DNoScale(PASS_TEXTURE_2D(_LensSurfaceDirtTex, sampler_linear_clamp_LensSurfaceDirtTex), LENS_SURFACE_DIRT_UV).rgb;
		half3 diffraction = SampleTex2DNoScale(PASS_TEXTURE_2D(_LensSurfaceDiffractionTex, sampler_linear_clamp_LensSurfaceDiffractionTex), LENS_DIFFRACTION_UV).rgb;

		#ifdef COLORSPACE_GAMMA
			dirt = GammaToLinearSpace(dirt);
			diffraction = GammaToLinearSpace(diffraction);
		#endif

		#ifdef MK_NATURAL
			g.rgb = lerp(g.rgb, g.rgb + gs.rgb * LENS_SURFACE_DIRT_INTENSITY, dirt);
			g.rgb = lerp(g.rgb, g.rgb + gs.rgb * LENS_SURFACE_DIFFRACTION_INTENSITY, diffraction);
		#else
			dirt *= LENS_SURFACE_DIRT_INTENSITY;
			diffraction *= LENS_SURFACE_DIFFRACTION_INTENSITY;
			g.rgb = lerp(g.rgb * 3, g.rgb + g.rgb * dirt + g.rgb * diffraction, 0.5) * 0.3333h;
		#endif
	#endif

	//When using gamma space at least try to get a nice looking result by adding the glow in the linear space of the src even if the base color space is gamma
	#ifdef MK_GLOW_COMPOSITE
		#ifdef COLORSPACE_GAMMA
			#ifndef MK_NATURAL
				g.rgb += src.rgb;
			#endif
			RETURN_TARGET_TEX ConvertToColorSpace(g);
		#else
			#ifndef MK_NATURAL
				g.rgb += src.rgb;
			#endif
			RETURN_TARGET_TEX g;
		#endif
	#else
		RETURN_TARGET_TEX ConvertToColorSpace(g);
	#endif
#endif