//////////////////////////////////////////////////////
// MK Glow Presets 	    	    	       		    //
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
    internal abstract class Presets
    {
        /// <summary>
        /// Set a preset for the glare effect
        /// </summary>
        internal static void SetLensFlarePreset(LensFlareStyle lensFlareStyle, ISettings settings)
        {
            switch(lensFlareStyle)
            {
                case LensFlareStyle.Average:
                    settings.SetLensFlareGhostFade(7.5f);
                    settings.SetLensFlareGhostCount(3);
                    settings.SetLensFlareGhostDispersal(0.67f);

                    settings.SetLensFlareHaloFade(7.5f);
                    settings.SetLensFlareHaloSize(0.5f);
                break;
                case LensFlareStyle.MultiAverage:
                    settings.SetLensFlareGhostFade(7.5f);
                    settings.SetLensFlareGhostCount(4);
                    settings.SetLensFlareGhostDispersal(0.4f);

                    settings.SetLensFlareHaloFade(7.5f);
                    settings.SetLensFlareHaloSize(0.5f);
                break;
                case LensFlareStyle.Old:
                    settings.SetLensFlareGhostFade(7.5f);
                    settings.SetLensFlareGhostCount(3);
                    settings.SetLensFlareGhostDispersal(-1);

                    settings.SetLensFlareHaloFade(7.5f);
                    settings.SetLensFlareHaloSize(0.5f);
                break;
                case LensFlareStyle.OldFocused:
                    settings.SetLensFlareGhostFade(7.5f);
                    settings.SetLensFlareGhostCount(3);
                    settings.SetLensFlareGhostDispersal(-0.75f);

                    settings.SetLensFlareHaloFade(7.5f);
                    settings.SetLensFlareHaloSize(0.2f);
                break;
                case LensFlareStyle.Distorted:
                    settings.SetLensFlareGhostFade(7.5f);
                    settings.SetLensFlareGhostCount(3);
                    settings.SetLensFlareGhostDispersal(0.62f);

                    settings.SetLensFlareHaloFade(7.5f);
                    settings.SetLensFlareHaloSize(0.56f);
                break;
                default:
                    //Custom no change at all
                break;
            }
        }

        /// <summary>
        /// Set a preset for the glare effect
        /// </summary>
        internal static void SetGlarePreset(GlareStyle glareStyle, ISettings settings)
        {
            switch(glareStyle)
            {
                case GlareStyle.Line:
                    settings.SetGlareStreaks(1);
                    settings.SetGlareSample0Angle(90);
                    settings.SetGlareSample0Scattering(5);
                    settings.SetGlareSample0Offset(0);
                    settings.SetGlareSample0Intensity(1);
                break;
                case GlareStyle.Tri:
                    settings.SetGlareStreaks(3);

                    settings.SetGlareSample0Angle(0);
                    settings.SetGlareSample0Scattering(2.5f);
                    settings.SetGlareSample0Offset(2.5f);
                    settings.SetGlareSample0Intensity(1);

                    settings.SetGlareSample1Angle(120);
                    settings.SetGlareSample1Scattering(2.5f);
                    settings.SetGlareSample1Offset(2.5f);
                    settings.SetGlareSample1Intensity(1);

                    settings.SetGlareSample2Angle(240);
                    settings.SetGlareSample2Scattering(2.5f);
                    settings.SetGlareSample2Offset(2.5f);
                    settings.SetGlareSample2Intensity(1);
                break;
                case GlareStyle.Cross:
                    settings.SetGlareStreaks(2);

                    settings.SetGlareSample0Angle(45);
                    settings.SetGlareSample0Scattering(5f);
                    settings.SetGlareSample0Offset(0f);
                    settings.SetGlareSample0Intensity(1);

                    settings.SetGlareSample1Angle(135);
                    settings.SetGlareSample1Scattering(5f);
                    settings.SetGlareSample1Offset(0f);
                    settings.SetGlareSample1Intensity(1);

                break;
                default:
                case GlareStyle.DistortedCross:
                    settings.SetGlareStreaks(2);

                    settings.SetGlareSample0Angle(60);
                    settings.SetGlareSample0Scattering(5f);
                    settings.SetGlareSample0Offset(0f);
                    settings.SetGlareSample0Intensity(1);

                    settings.SetGlareSample1Angle(120);
                    settings.SetGlareSample1Scattering(5f);
                    settings.SetGlareSample1Offset(0f);
                    settings.SetGlareSample1Intensity(1);

                break;
                case GlareStyle.Star:
                    settings.SetGlareStreaks(3);

                    settings.SetGlareSample0Angle(0);
                    settings.SetGlareSample0Scattering(5f);
                    settings.SetGlareSample0Offset(0f);
                    settings.SetGlareSample0Intensity(1);

                    settings.SetGlareSample1Angle(60);
                    settings.SetGlareSample1Scattering(5f);
                    settings.SetGlareSample1Offset(0f);
                    settings.SetGlareSample1Intensity(1);

                    settings.SetGlareSample2Angle(120);
                    settings.SetGlareSample2Scattering(5f);
                    settings.SetGlareSample2Offset(0f);
                    settings.SetGlareSample2Intensity(1);

                break;
                case GlareStyle.Flake:
                    settings.SetGlareStreaks(4);

                    settings.SetGlareSample0Angle(45);
                    settings.SetGlareSample0Scattering(5f);
                    settings.SetGlareSample0Offset(0f);
                    settings.SetGlareSample0Intensity(1);

                    settings.SetGlareSample1Angle(90);
                    settings.SetGlareSample1Scattering(5f);
                    settings.SetGlareSample1Offset(0f);
                    settings.SetGlareSample1Intensity(1);

                    settings.SetGlareSample2Angle(135);
                    settings.SetGlareSample2Scattering(5f);
                    settings.SetGlareSample2Offset(0f);
                    settings.SetGlareSample2Intensity(1);

                    settings.SetGlareSample3Angle(180);
                    settings.SetGlareSample3Scattering(5f);
                    settings.SetGlareSample3Offset(0f);
                    settings.SetGlareSample3Intensity(1);
                break;
                case GlareStyle.Custom:
                    //no change
                break;
            }
        }
    }
}
