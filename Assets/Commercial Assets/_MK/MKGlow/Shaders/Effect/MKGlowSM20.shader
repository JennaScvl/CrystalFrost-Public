//////////////////////////////////////////////////////
// MK Glow Shader SM20								//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2021 All rights reserved.            //
//////////////////////////////////////////////////////
Shader "Hidden/MK/Glow/MKGlowSM20"
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
			#pragma target 2.0
			#pragma vertex vertSimple
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest

			#define _MK_PPSV2
			#define _MK_BLOOM
			#pragma multi_compile __ _MK_RENDER_PRIORITY_BALANCED _MK_RENDER_PRIORITY_QUALITY
			#pragma multi_compile __ _MK_NATURAL

			#include "../Inc/Presample.hlsl"
			ENDHLSL
		}

		/////////////////////////////////////////////////////////////////////////////////////////////
        // Downsample - 1
        /////////////////////////////////////////////////////////////////////////////////////////////
		Pass
		{
			HLSLPROGRAM
			#pragma target 2.0
			#pragma vertex vertSimple
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest

			#define _MK_PPSV2
			#define _MK_BLOOM
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
			#pragma target 2.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest

			#define _MK_PPSV2
			#define _MK_BLOOM
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
			#pragma target 2.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest

			#define _MK_PPSV2

			#pragma multi_compile __ _MK_LEGACY_BLIT

			#pragma multi_compile __ _MK_LENS_SURFACE
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
			#pragma target 2.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest

			#define _MK_PPSV2

			#pragma multi_compile __ _MK_LEGACY_BLIT
			
			#pragma multi_compile __ _MK_DEBUG_RAW_BLOOM _MK_DEBUG_COMPOSITE
			#pragma multi_compile __ _MK_LENS_SURFACE
			#pragma multi_compile __ _MK_RENDER_PRIORITY_BALANCED _MK_RENDER_PRIORITY_QUALITY
			#pragma multi_compile __ _MK_NATURAL

			#include "../Inc/Debug.hlsl"
			ENDHLSL
		}
	}
	//Shadermodel 2 should be the lowest possible hardware level
	FallBack Off
}
