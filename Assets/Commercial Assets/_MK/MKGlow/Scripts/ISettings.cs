//////////////////////////////////////////////////////
// MK Glow ISettings 	    	    	       		//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2021 All rights reserved.            //
//////////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MK.Glow
{
    internal interface ISettings
    {
        //Main
        bool GetAllowGeometryShaders();
        bool GetAllowComputeShaders ();
        RenderPriority GetRenderPriority ();
        MK.Glow.DebugView GetDebugView();
        MK.Glow.Quality GetQuality();
        MK.Glow.AntiFlickerMode GetAntiFlickerMode();
        MK.Glow.Workflow GetWorkflow();
        LayerMask GetSelectiveRenderLayerMask();
        float GetAnamorphicRatio();
        float GetLumaScale();
		float GetBlooming();

        //Bloom
		MK.Glow.MinMaxRange GetBloomThreshold();
		float GetBloomScattering();
		float GetBloomIntensity();

        //LensSurface
		bool GetAllowLensSurface();
		Texture2D GetLensSurfaceDirtTexture();
		float GetLensSurfaceDirtIntensity();
		Texture2D GetLensSurfaceDiffractionTexture();
		float GetLensSurfaceDiffractionIntensity();

        //LensFlare
		bool GetAllowLensFlare();
        LensFlareStyle GetLensFlareStyle();
		float GetLensFlareGhostFade();
		float GetLensFlareGhostIntensity();
		MK.Glow.MinMaxRange GetLensFlareThreshold();
		float GetLensFlareScattering();
		Texture2D GetLensFlareColorRamp();

		float GetLensFlareChromaticAberration();
		int GetLensFlareGhostCount();
		float GetLensFlareGhostDispersal();
		float GetLensFlareHaloFade();
		float GetLensFlareHaloIntensity();
		float GetLensFlareHaloSize();

        void SetLensFlareGhostFade(float fade);
        void SetLensFlareGhostCount(int count);
        void SetLensFlareGhostDispersal(float dispersal);
        void SetLensFlareHaloFade(float fade);
        void SetLensFlareHaloSize(float size);

        //Glare
		bool GetAllowGlare();
        
        float GetGlareBlend();
        float GetGlareIntensity();
        float GetGlareAngle();
		MK.Glow.MinMaxRange GetGlareThreshold();
		int GetGlareStreaks();
        void SetGlareStreaks(int count);
        float GetGlareScattering();
        GlareStyle GetGlareStyle();

        //Sample0
        float GetGlareSample0Scattering();
        float GetGlareSample0Angle();
        float GetGlareSample0Intensity();
        float GetGlareSample0Offset();

        void SetGlareSample0Scattering(float scattering);
        void SetGlareSample0Angle(float angle);
        void SetGlareSample0Intensity(float intensity);
        void SetGlareSample0Offset(float offset);

        //Sample1
        float GetGlareSample1Scattering();
        float GetGlareSample1Angle();
        float GetGlareSample1Intensity();
        float GetGlareSample1Offset();

        void SetGlareSample1Scattering(float scattering);
        void SetGlareSample1Angle(float angle);
        void SetGlareSample1Intensity(float intensity);
        void SetGlareSample1Offset(float offset);

        //Sample2
        float GetGlareSample2Scattering();
        float GetGlareSample2Angle();
        float GetGlareSample2Intensity();
        float GetGlareSample2Offset();

        void SetGlareSample2Scattering(float scattering);
        void SetGlareSample2Angle(float angle);
        void SetGlareSample2Intensity(float intensity);
        void SetGlareSample2Offset(float offset);

        //Sample3
        float GetGlareSample3Scattering();
        float GetGlareSample3Angle();
        float GetGlareSample3Intensity();
        float GetGlareSample3Offset();

        void SetGlareSample3Scattering(float scattering);
        void SetGlareSample3Angle(float angle);
        void SetGlareSample3Intensity(float intensity);
        void SetGlareSample3Offset(float offset);
    }
}