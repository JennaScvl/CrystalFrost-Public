//////////////////////////////////////////////////////
// MK Glow Shader SM40								//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2021 All rights reserved.            //
//////////////////////////////////////////////////////
Shader "Hidden/MK/Glow/MKGlowSM45"
{
	SubShader
	{
		Tags {"LightMode" = "Always" "RenderType"="Opaque" "PerformanceChecks"="False"}
		Cull Off ZWrite Off ZTest Always

		/////////////////////////////////////////////////////////////////////////////////////////////
        // Presample - 0
        /////////////////////////////////////////////////////////////////////////////////////////////
		Pass
		{
			HLSLPROGRAM
			#pragma exclude_renderers gles gles3 glcore d3d11_9x wiiu n3ds switch
			#pragma target 4.5
			#pragma vertex vertSimple
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest

			#pragma require mrt8

			#define _MK_PPSV2
			#pragma multi_compile __ _MK_BLOOM
			#pragma multi_compile __ _MK_LENS_FLARE
			#pragma multi_compile __ _MK_GLARE_1 _MK_GLARE_2 _MK_GLARE_3 _MK_GLARE_4
			#pragma multi_compile __ _MK_RENDER_PRIORITY_BALANCED _MK_RENDER_PRIORITY_QUALITY
			#pragma multi_compile __ _MK_NATURAL
			#pragma multi_compile __ _MK_HQ_ANTI_FLICKER

			#include "../Inc/Presample.hlsl"
			ENDHLSL
		}

		/////////////////////////////////////////////////////////////////////////////////////////////
        // Downsample - 1
        /////////////////////////////////////////////////////////////////////////////////////////////
		Pass
		{
			HLSLPROGRAM
			#pragma exclude_renderers gles gles3 glcore d3d11_9x wiiu n3ds switch
			#pragma target 4.5
			#pragma vertex vertSimple
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest

			#pragma require mrt8

			#define _MK_PPSV2
			#pragma multi_compile __ _MK_BLOOM
			#pragma multi_compile __ _MK_LENS_FLARE
			#pragma multi_compile __ _MK_GLARE_1 _MK_GLARE_2 _MK_GLARE_3 _MK_GLARE_4
			#pragma multi_compile __ _MK_RENDER_PRIORITY_BALANCED _MK_RENDER_PRIORITY_QUALITY

			#include "../Inc/Downsample.hlsl"
			ENDHLSL
		}

		/////////////////////////////////////////////////////////////////////////////////////////////
        // Upsample - 2
        /////////////////////////////////////////////////////////////////////////////////////////////
		Pass
		{
			HLSLPROGRAM
			#pragma exclude_renderers gles gles3 glcore d3d11_9x wiiu n3ds switch
			#pragma target 4.5
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest

			#pragma require mrt8

			#define _MK_PPSV2
			#pragma multi_compile __ _MK_BLOOM
			#pragma multi_compile __ _MK_LENS_FLARE
			#pragma multi_compile __ _MK_GLARE_1 _MK_GLARE_2 _MK_GLARE_3 _MK_GLARE_4
			#pragma multi_compile __ _MK_RENDER_PRIORITY_BALANCED _MK_RENDER_PRIORITY_QUALITY

			#include "../Inc/Upsample.hlsl"
			ENDHLSL
		}

		/////////////////////////////////////////////////////////////////////////////////////////////
        // Composite - 3
        /////////////////////////////////////////////////////////////////////////////////////////////
		Pass
		{
			HLSLPROGRAM
			#pragma exclude_renderers gles gles3 glcore d3d11_9x wiiu n3ds switch
			#pragma target 4.5
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest

			#define _MK_PPSV2

			#pragma multi_compile __ _MK_LEGACY_BLIT

			#pragma require mrt8
			
			#pragma multi_compile __ _MK_LENS_SURFACE
			#pragma multi_compile __ _MK_LENS_FLARE
			#pragma multi_compile __ _MK_GLARE_1 _MK_GLARE_2 _MK_GLARE_3 _MK_GLARE_4
			#pragma multi_compile __ _MK_RENDER_PRIORITY_BALANCED _MK_RENDER_PRIORITY_QUALITY
			#pragma multi_compile __ _MK_NATURAL

			#include "../Inc/Composite.hlsl"
			ENDHLSL
		}

		/////////////////////////////////////////////////////////////////////////////////////////////
        // Debug - 4
        /////////////////////////////////////////////////////////////////////////////////////////////
		Pass
		{
			HLSLPROGRAM
			#pragma exclude_renderers gles gles3 glcore d3d11_9x wiiu n3ds switch
			#pragma target 4.5
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest

			#define _MK_PPSV2

			#pragma multi_compile __ _MK_LEGACY_BLIT
			
			#pragma require mrt8
			
			#pragma multi_compile __ _MK_DEBUG_RAW_BLOOM _MK_DEBUG_RAW_LENS_FLARE _MK_DEBUG_RAW_GLARE _MK_DEBUG_LENS_FLARE _MK_DEBUG_GLARE _MK_DEBUG_COMPOSITE
			#pragma multi_compile __ _MK_LENS_SURFACE
			#pragma multi_compile __ _MK_LENS_FLARE
			#pragma multi_compile __ _MK_GLARE_1 _MK_GLARE_2 _MK_GLARE_3 _MK_GLARE_4
			#pragma multi_compile __ _MK_RENDER_PRIORITY_BALANCED _MK_RENDER_PRIORITY_QUALITY
			#pragma multi_compile __ _MK_NATURAL
			#pragma multi_compile __ _MK_HQ_ANTI_FLICKER
			
			#include "../Inc/Debug.hlsl"
			ENDHLSL
		}
	}
	FallBack "Hidden/MK/Glow/MKGlowSM35"
}
