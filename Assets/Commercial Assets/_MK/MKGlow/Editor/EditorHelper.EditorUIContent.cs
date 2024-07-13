//////////////////////////////////////////////////////
// MK Glow Editor Helper UI Content	           		//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2021 All rights reserved.            //
//////////////////////////////////////////////////////
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

//Disable XRManagement warning for selective workflow
//XRpackage has to be imported...
#pragma warning disable CS0618

namespace MK.Glow.Editor
{
    public static partial class EditorHelper
    {
        public static class EditorUIContent
        {
            public static class Tooltips
            {
                public static readonly GUIContent workmode = new GUIContent("Workmode", "Choose between a Post Processing Volume or Global workmode. \nIf Global is used, then the Post Processing Framework can be skipped.");

                //Main
                public static readonly GUIContent allowGeometryShaders = new GUIContent("Allow Geometry Shaders", "Allowing the pipeline to use geometry shader if available.");
                public static readonly GUIContent allowComputeShaders = new GUIContent("Allow Compute Shaders", "Allowing the pipeline to use compute shader if available.");
                public static readonly GUIContent renderPriority = new GUIContent("Render Priority", "Define if the pipeline should focus on quality or performance. Balanced gives you the best trade-off in terms of performance and visual quality.");
                public static readonly GUIContent debugView = new GUIContent("Debug View", "Displaying of different render steps. \n \n" +
                                                                               "None: Debug view is disabled. \n\n" +
                                                                               "Raw Bloom: Shows extracted bloom map. \n\n" +
                                                                               "Raw Lens Flare: Shows extracted lens flare map. \n\n" +
                                                                               "Raw Glare: Shows extracted glare map. \n\n" +
                                                                               "Bloom: Shows created bloom without lens surface. \n\n" +
                                                                               "Lens Flare: Shows created lens flare without lens surface. \n\n" +
                                                                               "Glare: Shows created glare without lens surface. \n\n" +
                                                                               "Composite: Shows combined bloom, lensflare, glare and lens surface, just without source image. \n");
                public static readonly GUIContent quality = new GUIContent("Quality", "General rendered quality of the glow. Higher setting results in better looking and less aliasing. High/Medium is recommend in most cases.");
                public static readonly GUIContent antiFlickerMode = new GUIContent("AntiFlicker", "Type of antiflicker filter. Balanced should be fine for most cases.");
                public static readonly GUIContent workflow = new GUIContent("Workflow", "Basic definition of the workflow. \n\n" +
                                                                              "Luminance: Glow map is defined by the pixels brightness and a threshold value. Just use the emission of the shaders and raise it up. Performs significantly faster than selective workflow.\n\n" +
                                                                               "Selective: Glow map is created by using separate shaders (MK/Glow/Selective).\n\n" +
                                                                               "Natural: Glow map is based on a realistic behavior. No colors gets cut off. Performs significantly faster than selective workflow");
                public static readonly GUIContent selectiveRenderLayerMask = new GUIContent("Render Layer", "In most cases 'Everything' should be chosen to avoid Z issues.");
                public static readonly GUIContent anamorphicRatio = new GUIContent("Anamorphic", "Anamorphic scaling. \n\n" +
                                                                                     "> 0: scaling horizontally\n" +
                                                                                     "< 0: scaling vertically\n" +
                                                                                     "0: no scaling");
                public static readonly GUIContent lumaScale = new GUIContent("Luma Scale", "Mixing of the relative luminance (Luminosity function) into the glow map. Each color part contributes a different amount of light.");
                public static readonly GUIContent blooming = new GUIContent("Blooming", "Blooming increases glowing parts even further. Increasing this value value gives the scene a more bloomy look.");

                //Bloom
                public static readonly GUIContent bloomThreshold = new GUIContent("Threshold", "Threshold in gamma space for extraction of bright areas. \n\n Min: Minimum brightness until the bloom starts. \n Max: Maximum brightness for cutting off colors.");
                public static readonly GUIContent bloomScattering = new GUIContent("Scattering", "Scattering of the bloom. A higher value increases the scattered area.");
                public static readonly GUIContent bloomIntensity = new GUIContent("Intensity", "Intensity of the bloom in gamma space.");

                //Lens Surface
                public static readonly GUIContent allowLensSurface = new GUIContent("", "");
                public static readonly GUIContent lensSurfaceDirtTexture = new GUIContent("Dirt", "Dirt overlay which will be applied to the glow (RGB). Best results if texture is tileable.");
                public static readonly GUIContent lensSurfaceDirtIntensity = new GUIContent("Intensity", "Intensity of the dirt effect. Value is in gamma space.");
                public static readonly GUIContent lensSurfaceDiffractionTexture = new GUIContent("Diffraction", "Diffraction overlay which will be applied to the glow (RGB). Texture is rotating based on view.");
                public static readonly GUIContent lensSurfaceDiffractionIntensity = new GUIContent("Intensity", "Intensity of the diffraction effect in gamma space.");

                //Lens flare
                public static readonly GUIContent allowLensFlare = new GUIContent("", "");
                public static readonly GUIContent lensFlareStyle = new GUIContent("Style", "Style of the lens flare effect. Switch between presets or a custom style to mix up ghosts and halo.");
                public static readonly GUIContent lensFlareGhostFade = new GUIContent("Fade", "Fading of the ghosts. A higher value make the ghosts less visible");
                public static readonly GUIContent lensFlareGhostIntensity = new GUIContent("Intensity", "Intensity of the ghosts in gamma space.");
                public static readonly GUIContent lensFlareThreshold = new GUIContent("Threshold", "Threshold in gamma space for extraction of bright areas. \n\n Min: Minimum brightness until the lens flare starts. \n Max: Maximum brightness for cutting off colors.");
                public static readonly GUIContent lensFlareScattering = new GUIContent("Scattering", "Scattering of the lens flare. A higher value increases the scattered area and blurs the flare stronger.");
                public static readonly GUIContent lensFlareColorRamp = new GUIContent("Color Ramp", "Color ramp of the lens flare. Tint will applied radially (RGB).");
                public static readonly GUIContent lensFlareChromaticAberration = new GUIContent("Chromatic Aberration", "Strength of the chromatic aberration. A higher / lower value spread the color parts further away.");
                public static readonly GUIContent lensFlareGhostCount = new GUIContent("Count", "Count of the ghosts which are created.");
                public static readonly GUIContent lensFlareGhostDispersal = new GUIContent("Dispersal", "Dispersion between the ghosts.");
                public static readonly GUIContent lensFlareHaloFade = new GUIContent("Fade", "Fading of the halo. A higher value make the halo less visible.");
                public static readonly GUIContent lensFlareHaloIntensity = new GUIContent("Intensity", "Intensity of the halo in gamma space.");
                public static readonly GUIContent lensFlareHaloSize = new GUIContent("Size", "Overall radius of the halo");

                //Glare
                public static readonly GUIContent allowGlare = new GUIContent("", "");
                public static readonly GUIContent glareStyle = new GUIContent("Style", "Style of the glare effect. Switch between presets or a custom style up to 4 light streaks.");
                public static readonly GUIContent glareStreaks = new GUIContent("Streaks", "Amount of visible glare streaks (up to a maximum of 4).");
                public static readonly GUIContent glareBlend = new GUIContent("Blend", "Blending between bloom and glare. 0: more/only bloom visible, 1: more/only glare visible.");
                public static readonly GUIContent glareIntensity = new GUIContent("Intensity", "Global intensity of the final glare effect. (linear)");
                public static readonly GUIContent glareThreshold = new GUIContent("Threshold", "Threshold in gamma space for extraction of bright areas. \n\n Min: Minimum brightness until the glare starts. \n Max: Maximum brightness for cutting off colors.");
                public static readonly GUIContent glareScattering = new GUIContent("Scattering", "Global scattering of the glare. A higher value increases the scattered area.");
                public static readonly GUIContent glareAngle = new GUIContent("Angle", "Global angle of the glare.");
                public static readonly GUIContent glareSample0Scattering = new GUIContent("Scattering", "Scattering of the glare sample. A higher value increases the scattered area.");
                public static readonly GUIContent glareSample0Intensity = new GUIContent("Intensity", "Intensity of the glare sample in gamma space.");
                public static readonly GUIContent glareSample0Angle = new GUIContent("Angle", "Angle of the glare sample in degree.");
                public static readonly GUIContent glareSample0Offset = new GUIContent("Offset", "Offset of the sample based on the center.");
                public static readonly GUIContent glareSample1Scattering = new GUIContent("Scattering", "Scattering of the glare sample. A higher value increases the scattered area.");
                public static readonly GUIContent glareSample1Intensity = new GUIContent("Intensity", "Intensity of the glare sample in gamma space.");
                public static readonly GUIContent glareSample1Angle = new GUIContent("Angle", "Angle of the glare sample in degree.");
                public static readonly GUIContent glareSample1Offset = new GUIContent("Offset", "Offset of the sample based on the center.");
                public static readonly GUIContent glareSample2Scattering = new GUIContent("Scattering", "Scattering of the glare sample. A higher value increases the scattered area.");
                public static readonly GUIContent glareSample2Intensity = new GUIContent("Intensity", "Intensity of the glare sample in gamma space.");
                public static readonly GUIContent glareSample2Angle = new GUIContent("Angle", "Angle of the glare sample in degree.");
                public static readonly GUIContent glareSample2Offset = new GUIContent("Offset", "Offset of the sample based on the center.");
                public static readonly GUIContent glareSample3Scattering = new GUIContent("Scattering", "Scattering of the glare sample. A higher value increases the scattered area.");
                public static readonly GUIContent glareSample3Intensity = new GUIContent("Intensity", "Intensity of the glare sample in gamma space.");
                public static readonly GUIContent glareSample3Angle = new GUIContent("Angle", "Angle of the glare sample in degree.");
                public static readonly GUIContent glareSample3Offset = new GUIContent("Offset", "Offset of the sample based on the center.");
            }

            public static readonly string mainTitle = "Main";
            public static readonly string bloomTitle = "Bloom";
            public static readonly string lensSurfaceTitle = "Lens Surface";
            public static readonly string dirtTitle = "Dirt:";
            public static readonly string diffractionTitle = "Diffraction:";
            public static readonly string lensFlareTitle = "Lens Flare (SM 3.0+)";
            public static readonly string ghostsTitle = "Ghosts:";
            public static readonly string haloTitle = "Halo:";
            public static readonly string glareTitle = "Glare (SM 4.0+)";
            public static readonly string sample0Title = "Sample 0:";
            public static readonly string sample1Title = "Sample 1:";
            public static readonly string sample2Title = "Sample 2:";
            public static readonly string sample3Title = "Sample 3:";

            public static void LensFlareFeatureNotSupportedWarning()
            {
                EditorGUILayout.HelpBox("Lens flare feature is not supported on your active graphics api / render setup.", MessageType.Warning);
            }

            public static void GlareFeatureNotSupportedWarning()
            {
                EditorGUILayout.HelpBox("Glare feature is not supported on your active graphics api / render setup.", MessageType.Warning);
            }

            public static void OptimalSetupWarning(UnityEngine.Camera camera, bool warningAllowed)
            {
                if(warningAllowed)
                {
                    string msg = "";
                    if(!camera.allowHDR && PlayerSettings.colorSpace != ColorSpace.Linear)
                    {
                        msg = "linear color space and hdr";
                    }
                    else if(PlayerSettings.colorSpace != ColorSpace.Linear)
                    {
                        msg = "linear color space";
                    }
                    else if(!camera.allowHDR)
                    {
                        msg = "hdr";
                    }
                    if(!camera.allowHDR || PlayerSettings.colorSpace != ColorSpace.Linear)
                        EditorGUILayout.HelpBox("For best looking results its recommend to use " + msg, MessageType.Warning);
                }
            }

            public static void XRUnityVersionWarning()
            {
                #if UNITY_2018_3_OR_NEWER
                #else
                if(PlayerSettings.virtualRealitySupported)
                {
                    EditorGUILayout.HelpBox("Your are currently targeting XR. For best XR support its recommend to update to unity 2018.3 or higher.", MessageType.Warning);
                }
                #endif
            }

            public static void SelectiveWorkflowVRWarning(MK.Glow.Workflow workflow)
            {
                if(UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset != null && workflow == Workflow.Selective)
                {
                    EditorGUILayout.HelpBox("Selective workflow isn't supported if a scriptable rendering pipeline is active. Please use Threshold or Natural workflow instead.", MessageType.Warning);
                }
                if(PlayerSettings.virtualRealitySupported && workflow == Workflow.Selective)
                {
                    EditorGUILayout.HelpBox("Selective workflow isn't supported in XR. Please use Threshold or Natural workflow instead.", MessageType.Warning);
                }
            }

            public static void IsNotSupportedWarning()
            {
                if(!Compatibility.IsSupported)
                    EditorGUILayout.HelpBox("Plugin is not supported and will be disabled. At least any HDR RenderTexture format should be supported by your hardware.", MessageType.Warning);
            }

            public static void SelectiveWorkflowDeprecated()
            {
                EditorGUILayout.HelpBox("Selective Workflow will be deprecated in a future update, due to engine compatibility issues. Its highly recommend to use Threshold or Natural Workflow.", MessageType.Warning);
            }
        }
    }
}
#endif