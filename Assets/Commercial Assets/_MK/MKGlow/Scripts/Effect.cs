//////////////////////////////////////////////////////
// MK Glow Effect	    			                //
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2021 All rights reserved.            //
//////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.Linq;

namespace MK.Glow
{
    using ShaderProperties = PipelineProperties.ShaderProperties;

    internal sealed class Effect
    {

        internal Effect()
        {
            //_cArgsComputeBuffer = new ComputeBuffer(_cArgBufferSize, 4, ComputeBufferType.Default);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////
        // Members
        /////////////////////////////////////////////////////////////////////////////////////////////
        //always needed parameters - static
        private static MK.Glow.Resources _resources;
        private static readonly Vector2 _referenceResolution = new Vector2(3840, 2160);
        private static readonly float _referenceAspectRatio = 0.5625f;
        private static readonly Vector2 _selectiveWorkflowThreshold = new Vector2(0.1f, 10);
        private static readonly int _cArgBufferSize = 66;
        private static readonly int _glareIterationsBase = 3;
        private static readonly RenderDimension _directComputeSize = new RenderDimension(8, 7);
        private static readonly float naturalIntensityMult = 0.1f;

        //Selective rendering objects
        private static readonly string _selectiveReplacementTag = "RenderType";
        private static readonly string _selectiveGlowCameraObjectName = "selectiveGlowCameraObject";
        private GameObject _selectiveGlowCameraObject;
        private UnityEngine.Camera _selectiveGlowCamera;

        //Compute Shader Feature Matrices
        //Copy variant is always 0
        //private ComputeShaderVariants _presampleComputeVariants = new ComputeShaderVariants(0);
        //private ComputeShaderVariants _downsampleComputeVariants = new ComputeShaderVariants(240);
        //private ComputeShaderVariants _upsampleComputeVariants = new ComputeShaderVariants(480);
        //Debug and composite variants skipped for now because its always the final blit to show up the render context

        //Renderbuffers
        private CommandBuffer _commandBuffer;
        private bool _finalBlit = true;
        private RenderTarget _selectiveRenderTarget;
		private MipBuffer _bloomDownsampleBuffer, _bloomUpsampleBuffer;
        private MipBuffer _lensFlareDownsampleBuffer, _lensFlareUpsampleBuffer;
        private MipBuffer _glareDownsampleBuffer0, _glareDownsampleBuffer1, _glareDownsampleBuffer2, _glareDownsampleBuffer3, _glareUpsampleBuffer0, _glareUpsampleBuffer1, _glareUpsampleBuffer2, _glareUpsampleBuffer3;

        private RenderTarget _sourceFrameBuffer, _destinationFrameBuffer;
        private RenderTarget sourceFrameBuffer
        {
            get 
            {
                return _settings.GetWorkflow() == Workflow.Selective && _debugView != DebugView.None ? _selectiveRenderTarget : _sourceFrameBuffer;
            }
        }

        //Runtime needed
        private Keyword[] _shaderKeywords = new Keyword[] 
        {
            new Keyword("_MK_BLOOM", false),
            new Keyword("_MK_LENS_SURFACE", false),
            new Keyword("_MK_LENS_FLARE", false),
            new Keyword("_MK_GLARE_1", false),
            new Keyword("_MK_DEBUG_RAW_BLOOM", false),
            new Keyword("_MK_DEBUG_RAW_LENS_FLARE", false),
            new Keyword("_MK_DEBUG_RAW_GLARE", false),
            new Keyword("_MK_DEBUG_BLOOM", false), //No Keyword will be set
            new Keyword("_MK_DEBUG_LENS_FLARE", false),
            new Keyword("_MK_DEBUG_GLARE", false),
            new Keyword("_MK_DEBUG_COMPOSITE", false),
            new Keyword("_MK_LEGACY_BLIT", false),
            new Keyword("_MK_RENDER_PRIORITY_QUALITY", false),
            new Keyword("_MK_NATURAL", false),
            new Keyword("_MK_GLARE_2", false),
            new Keyword("_MK_GLARE_3", false),
            new Keyword("_MK_GLARE_4", false),
            new Keyword("", false),
            new Keyword("_MK_RENDER_PRIORITY_BALANCED", false),
            new Keyword("_MK_HQ_ANTI_FLICKER", false)
        };

        //Used features
        private bool _useGeometryShaders, _useComputeShaders, _useLensSurface, _useLensFlare, _useGlare;

        //Lists
        private List<RenderTarget> _renderTargetsBundle;
        private List<MaterialKeywords> _renderKeywordsBundle;

        //Rendering dependent
        private int _bloomIterations, _lensFlareIterations, _minIterations, _glareIterations, _currentRenderIndex;
        internal int currentRenderIndex { get { return _currentRenderIndex; }}
        private float bloomUpsampleSpread, _lensFlareUpsampleSpread, _glareScatteringMult;
        private Vector2 _resolutionScale;
        private Vector2[] glareAngles = new Vector2[4];
        private RenderTextureFormat _renderTextureFormat;
        internal RenderTextureFormat renderTextureFormat { get{ return _renderTextureFormat; } }
        private ComputeShaderVariants.KeywordState computeShaderFeatures = new ComputeShaderVariants.KeywordState(0, 0, 0, 0, 0, 0);
        private RenderContext[] _sourceContext, _renderContext;
        private RenderContext _selectiveRenderContext;
        private UnityEngine.Camera _renderingCamera;
        private ICameraData _cameraData;
        private RenderPipeline _renderPipeline;
        private DebugView _debugView;

        //Materials
        private Material _renderMaterialNoGeometry;
        internal Material renderMaterialNoGeometry { get { return _renderMaterialNoGeometry; } }
        private Material _renderMaterialGeometry;

        //Direct compute dependent
        private float[] _cArgArray = new float[_cArgBufferSize];
        private ComputeBuffer _cArgsComputeBuffer;
        private RenderDimension _computeThreadGroups = new RenderDimension();

        //Settings
        private ISettings _settings;

        /////////////////////////////////////////////////////////////////////////////////////////////
        // Unity MonoBehavior Messages
        /////////////////////////////////////////////////////////////////////////////////////////////
        //shaderoverwrites should both null or referenced
        internal void Enable(RenderPipeline renderPipeline)
        {
            _resources = MK.Glow.Resources.LoadResourcesAsset();

            _renderTextureFormat = Compatibility.CheckSupportedRenderTextureFormat();

            _renderPipeline = renderPipeline;
            _sourceContext = new RenderContext[1]{new RenderContext()};
            _renderContext = new RenderContext[PipelineProperties.renderBufferSize];
            for(int i = 0; i < PipelineProperties.renderBufferSize; i++)
                _renderContext[i] = new RenderContext();
            _selectiveRenderContext = new RenderContext();

            _renderMaterialNoGeometry = new Material(_resources.sm45Shader) { hideFlags = HideFlags.HideAndDontSave };
            _renderMaterialGeometry = new Material(_resources.sm45Shader) { hideFlags = HideFlags.HideAndDontSave };
            
            _renderTargetsBundle = new List<RenderTarget>();
            _renderKeywordsBundle = new List<MaterialKeywords>();

            //create buffers
            _bloomDownsampleBuffer = new MipBuffer(PipelineProperties.CommandBufferProperties.bloomDownsampleBuffer, _renderPipeline);
            _bloomUpsampleBuffer = new MipBuffer(PipelineProperties.CommandBufferProperties.bloomUpsampleBuffer, _renderPipeline);
            _lensFlareDownsampleBuffer = new MipBuffer(PipelineProperties.CommandBufferProperties.lensFlareDownsampleBuffer, _renderPipeline);
            _lensFlareUpsampleBuffer = new MipBuffer(PipelineProperties.CommandBufferProperties.lensFlareUpsampleBuffer, _renderPipeline);
            _glareDownsampleBuffer0 = new MipBuffer(PipelineProperties.CommandBufferProperties.glareDownsampleBuffer0, _renderPipeline);
            _glareDownsampleBuffer1 = new MipBuffer(PipelineProperties.CommandBufferProperties.glareDownsampleBuffer1, _renderPipeline);
            _glareDownsampleBuffer2 = new MipBuffer(PipelineProperties.CommandBufferProperties.glareDownsampleBuffer2, _renderPipeline);
            _glareDownsampleBuffer3 = new MipBuffer(PipelineProperties.CommandBufferProperties.glareDownsampleBuffer3, _renderPipeline);
            _glareUpsampleBuffer0 = new MipBuffer(PipelineProperties.CommandBufferProperties.glareUpsampleBuffer0, _renderPipeline);
            _glareUpsampleBuffer1 = new MipBuffer(PipelineProperties.CommandBufferProperties.glareUpsampleBuffer1, _renderPipeline);
            _glareUpsampleBuffer2 = new MipBuffer(PipelineProperties.CommandBufferProperties.glareUpsampleBuffer2, _renderPipeline);
            _glareUpsampleBuffer3 = new MipBuffer(PipelineProperties.CommandBufferProperties.glareUpsampleBuffer3, _renderPipeline);
        }

        ~Effect()
        {
            //_cArgsComputeBuffer.Release();
            _cArgsComputeBuffer = null;
        }

        internal void Disable()
        {            
            _currentRenderIndex = 0;
            _renderTargetsBundle.Clear();
            _renderKeywordsBundle.Clear();

            MonoBehaviour.DestroyImmediate(_selectiveGlowCamera);
            MonoBehaviour.DestroyImmediate(_selectiveGlowCameraObject);
            MonoBehaviour.DestroyImmediate(_renderMaterialNoGeometry);
            MonoBehaviour.DestroyImmediate(_renderMaterialGeometry);

            MK.Glow.Resources.UnLoadResourcesAsset(_resources);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////
        // RenderBuffers
        /////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Prepare Scattering parameters fora given Scattering value
        /// </summary>
        /// <param name="Scattering"></param>
        /// <param name="scale"></param>
        /// <param name="iterations"></param>
        /// <param name="spread"></param>
        private void PrepareScattering(float Scattering, float scale, ref int iterations, ref float spread)
        {
            /*
            float lit = Mathf.Log(scale, 2f) + Mathf.Min(Scattering, 10f) - 10f;
            int litF = Mathf.FloorToInt(lit); 
            iterations = Mathf.Clamp(litF, 1, 15);
            spread = 0.5f + lit - litF;
            */
            
            float scaledIterations = scale + Mathf.Clamp(Scattering, 1f, 10.0f) - 10.0f;
            iterations = Mathf.Max(Mathf.FloorToInt(scaledIterations), 1);
            spread = scaledIterations > 1 ? 0.5f + scaledIterations - iterations : 0.5f;
            
        }

        /// <summary>
        /// Create renderbuffers
        /// </summary>
        private void UpdateRenderBuffers()
        {
            RenderDimension renderDimension = new RenderDimension(_cameraData.GetCameraWidth(), _cameraData.GetCameraHeight());
            _sourceContext[0].UpdateRenderContext(_cameraData, _renderTextureFormat, 0, _useComputeShaders, renderDimension);
            _sourceContext[0].SinglePassStereoAdjustWidth(_cameraData.GetStereoEnabled());
            Vector2 anamorphic = new Vector2(_settings.GetAnamorphicRatio() < 0 ? -_settings.GetAnamorphicRatio() : 0f, _settings.GetAnamorphicRatio() > 0 ?  _settings.GetAnamorphicRatio() : 0f);
            switch(_settings.GetQuality())
            {
                case Quality.Ultra:
                    anamorphic *= 0.5f;
                break;
                case Quality.High:
                    //anamorphic *= 1.0f;
                break;
                case Quality.Medium:
                    anamorphic *= 2.0f;
                break;
                case Quality.Low:
                    anamorphic *= 4.0f;
                break;
                case Quality.VeryLow:
                    anamorphic *= 6.0f;
                break;
            }
            renderDimension = new RenderDimension(Mathf.CeilToInt(_sourceContext[0].width / ((float)_settings.GetQuality() - anamorphic.x)), Mathf.CeilToInt(_sourceContext[0].height / ((float)_settings.GetQuality() - anamorphic.y)));

            float sizeScale = Mathf.Log(Mathf.FloorToInt(Mathf.Max(renderDimension.width, renderDimension.height)), 2.0f);
            //float sizeScale = Mathf.FloorToInt(Mathf.Max(renderDimension.width, renderDimension.height));

            PrepareScattering(_settings.GetBloomScattering(), sizeScale, ref _bloomIterations, ref bloomUpsampleSpread);
            _minIterations = _bloomIterations;
            
            if(_useLensFlare)
            {
                PrepareScattering(_settings.GetLensFlareScattering(), sizeScale, ref _lensFlareIterations, ref _lensFlareUpsampleSpread);
                if(_lensFlareIterations > _minIterations)
                    _minIterations = _lensFlareIterations;
            }

            if(_useGlare)
            {
                 switch(_settings.GetQuality())
                {
                    case Quality.High:
                    case Quality.Medium:
                    case Quality.Low:
                        _glareIterations = _glareIterationsBase;
                        _glareScatteringMult = 1;
                    break;
                    default:
                        _glareIterations = _glareIterationsBase;
                        _glareScatteringMult = 1;
                    break;
                }
                if(_glareIterations > _minIterations)
                    _minIterations = _glareIterations;
            }

            _cameraData.UpdateMipRenderContext(_renderContext, renderDimension, _minIterations + 1, _renderTextureFormat, 0, _useComputeShaders);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////
        // Selective glow setup
        /////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// selective replacement shader rendering camera for the glow
        /// </summary>
        private GameObject selectiveGlowCameraObject
        {
            get
            {
                if(!_selectiveGlowCameraObject)
                {
                    _selectiveGlowCameraObject = new GameObject(_selectiveGlowCameraObjectName);
                    _selectiveGlowCameraObject.AddComponent<UnityEngine.Camera>();
                    _selectiveGlowCameraObject.hideFlags = HideFlags.HideAndDontSave;
                }
                return _selectiveGlowCameraObject;
            }
        }

        /// <summary>
        /// selective replacement shader rendering camera forthe glow
        /// </summary>
        private UnityEngine.Camera selectiveGlowCamera
        {
            get
            {
                if(_selectiveGlowCamera == null)
                {
                    _selectiveGlowCamera = selectiveGlowCameraObject.GetComponent<UnityEngine.Camera>();
                    _selectiveGlowCamera.hideFlags = HideFlags.HideAndDontSave;
                    _selectiveGlowCamera.enabled = false;
                }
                return _selectiveGlowCamera;
            }
        }

        /// <summary>
        /// Prepare replacement rendering camera forthe selective glow
        /// </summary>
        private void SetupSelectiveGlowCamera()
        {
            selectiveGlowCamera.CopyFrom(_renderingCamera);
            selectiveGlowCamera.targetTexture = _selectiveRenderTarget.renderTexture;
            selectiveGlowCamera.clearFlags = CameraClearFlags.SolidColor;
            selectiveGlowCamera.rect = new Rect(0,0, 1,1);
            selectiveGlowCamera.backgroundColor = new Color(0, 0, 0, 1);
            selectiveGlowCamera.cullingMask = _settings.GetSelectiveRenderLayerMask();
            selectiveGlowCamera.renderingPath = RenderingPath.VertexLit;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////
        // CommandBuffer creation
        /////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Enable or disable all supported / unsupported shaders based on the platform
        /// </summary>
        private void CheckFeatureSupport()
        {
            //Check iflens surface is set
            if(_settings.GetAllowLensSurface())
                _useLensSurface = true;
            else
                _useLensSurface = false;
            
            //Check ifLensFlare is supported
            if(_settings.GetAllowLensFlare() && Compatibility.CheckLensFlareFeatureSupport() && (int)_settings.GetQuality() <= 4)
                _useLensFlare = true;
            else
                _useLensFlare = false;
            
            //Check if Glare is supported
            if(_settings.GetAllowGlare() && Compatibility.CheckGlareFeatureSupport() && (int)_settings.GetQuality() <= 4)
                _useGlare = true;
            else
                _useGlare = false;

            /*
            //Check for geometry shader support
            if(_settings.allowGeometryShaders && Compatibility.CheckGeometryShaderSupport())
                _useGeometryShaders = true;
            else
                _useGeometryShaders = false;

            //Check for compute shader support
            //TODO: if single pass stereo enabled compute shaders are turning off because UCG variables are not defined
            // -> more compute shader variants are needed
            //dont allow compute shaders to do lens flare on gles and glcore - dynamic for loop combined with compute shader seems not to work
            if(_settings.allowComputeShaders && Compatibility.CheckComputeShaderSupport() && !_cameraData.GetStereoEnabled())
                _useComputeShaders = true;
            else
                _useComputeShaders = false;
            */

            //If any debug view without depending feature is enabled fallback to default rendering
            if(_debugView != DebugView.None)
            {
                if(!_useLensFlare && (_debugView == DebugView.LensFlare || _debugView == DebugView.RawLensFlare) ||
                   !_useGlare &&(_debugView == DebugView.Glare || _debugView == DebugView.RawGlare))
                    _debugView = DebugView.None;
            }
            
            _useComputeShaders = false;
            _useGeometryShaders = false;
        }

        private void BeginProfileSample(string text)
        {
            if(_renderPipeline == RenderPipeline.SRP)
                _commandBuffer.BeginSample(text);
            else
                UnityEngine.Profiling.Profiler.BeginSample(text);
        }
        private void EndProfileSample(string text)
        {
            if(_renderPipeline == RenderPipeline.SRP)
                _commandBuffer.EndSample(text);
            else
                UnityEngine.Profiling.Profiler.EndSample();
        }

        //Camera is still required for backwards compatibility of selective glow, should be removed in the future
        /// <summary>
        /// Renders the effect from source into destination buffer
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        internal void Build(RenderTarget source, RenderTarget destination, ISettings settings, CommandBuffer cmd, ICameraData cameraData, UnityEngine.Camera renderingCamera = null, bool finalBlit = true)
        {
            _commandBuffer = cmd;
            _finalBlit = finalBlit;
            _settings = settings;
            _renderingCamera = renderingCamera;
            _cameraData = cameraData;
            _debugView = settings.GetDebugView();

            BeginProfileSample(PipelineProperties.CommandBufferProperties.samplePrepare);
            
            CheckFeatureSupport();

            _sourceFrameBuffer = source;
            _destinationFrameBuffer = destination;

            UpdateRenderBuffers();
            EndProfileSample(PipelineProperties.CommandBufferProperties.samplePrepare);

            //Prepare for selective glow
            if(_settings.GetWorkflow() == Workflow.Selective)
            {
                BeginProfileSample(PipelineProperties.CommandBufferProperties.sampleReplacement);
                _selectiveRenderContext.UpdateRenderContext(_cameraData, _renderTextureFormat, 16, false, _sourceContext[0].renderDimension);
                //The allowVerticallyFlip flag seems to break sometimes orientation of the rendered glow map, therefore force the old way.
                _selectiveRenderTarget.renderTexture = RenderTexture.GetTemporary(_cameraData.GetCameraWidth() / (int)_settings.GetQuality(), _cameraData.GetCameraHeight() / (int)_settings.GetQuality(), 16, _renderTextureFormat, RenderTextureReadWrite.Default, 1);//PipelineExtensions.GetTemporary(_selectiveRenderContext, _renderTextureFormat);
                SetupSelectiveGlowCamera();
                selectiveGlowCamera.RenderWithShader(_resources.selectiveRenderShader, _selectiveReplacementTag);
                EndProfileSample(PipelineProperties.CommandBufferProperties.sampleReplacement);
            }

            BeginProfileSample(PipelineProperties.CommandBufferProperties.sampleSetup);
            _resolutionScale = new Vector2(_renderContext[0].width / _referenceResolution.x * _renderContext[0].height / _renderContext[0].width / _referenceAspectRatio, _renderContext[0].height / _referenceResolution.y);
            UpdateConstantBuffers();
            EndProfileSample(PipelineProperties.CommandBufferProperties.sampleSetup);

            PreSample();
            Downsample();
            Upsample();
            Composite();
        }

        /// <summary>
        /// Update the profile based on the user input
        /// </summary>
        private void UpdateConstantBuffers()
        {      
            //Common
            SetVector(PipelineProperties.ShaderProperties.screenSize, new Vector2(_cameraData.GetCameraWidth(), _cameraData.GetCameraHeight()), true);
            SetFloat(PipelineProperties.ShaderProperties.singlePassStereoScale, PipelineProperties.singlePassStereoDoubleWideEnabled ? 2 : 1);
            SetFloat(PipelineProperties.ShaderProperties.lumaScale, _settings.GetLumaScale());
            SetFloat(PipelineProperties.ShaderProperties.blooming, _settings.GetBlooming(), true);
            SetVector(PipelineProperties.ShaderProperties.resolutionScale, _resolutionScale);
            SetVector(PipelineProperties.ShaderProperties.resolutionScale, _resolutionScale, true);
            SetVector(PipelineProperties.ShaderProperties.renderTargetSize, new Vector2(_cameraData.GetCameraWidth(), _cameraData.GetCameraHeight()), true);

            Matrix4x4 viewMatrix = _cameraData.GetWorldToCameraMatrix();
            //Setting 4x4 matrix via vector rows
            if(_useComputeShaders)
            {
                SetVector(ShaderProperties.viewMatrix, viewMatrix.GetRow(0), true);
                SetVector(ShaderProperties.viewMatrix, viewMatrix.GetRow(1), true);
                SetVector(ShaderProperties.viewMatrix, viewMatrix.GetRow(2), true);
                SetVector(ShaderProperties.viewMatrix, viewMatrix.GetRow(3), true);
            }
            else
            {
                Shader.SetGlobalMatrix(ShaderProperties.viewMatrix.id, viewMatrix);
            }

            //Bloom
            SetFloat(PipelineProperties.ShaderProperties.bloomIntensity, ConvertGammaValue(_settings.GetBloomIntensity() * (_settings.GetWorkflow() == Workflow.Natural ? naturalIntensityMult : 1f)), true);
            SetFloat(PipelineProperties.ShaderProperties.bloomSpread, bloomUpsampleSpread);
            SetFloat(PipelineProperties.ShaderProperties.bloomSpread, bloomUpsampleSpread, true);

            SetVector(PipelineProperties.ShaderProperties.bloomThreshold, _settings.GetWorkflow() == Workflow.Selective ? _selectiveWorkflowThreshold : new Vector2(ConvertGammaValue(_settings.GetBloomThreshold().minValue), ConvertGammaValue(_settings.GetBloomThreshold().maxValue)), _debugView == DebugView.RawBloom ? true : false);

            //LensSurface
            if(_useLensSurface)
            {
                SetFloat(PipelineProperties.ShaderProperties.lensSurfaceDirtIntensity, ConvertGammaValue(_settings.GetLensSurfaceDirtIntensity() * (_settings.GetWorkflow() == Workflow.Natural ? naturalIntensityMult : 1f)), true);
                SetFloat(PipelineProperties.ShaderProperties.lensSurfaceDiffractionIntensity, ConvertGammaValue(_settings.GetLensSurfaceDiffractionIntensity() * (_settings.GetWorkflow() == Workflow.Natural ? naturalIntensityMult : 1f)), true);
                float dirtRatio = (float)(_settings.GetLensSurfaceDirtTexture() ? _settings.GetLensSurfaceDirtTexture().width : _resources.lensSurfaceDirtTextureDefault.width) / 
                (float)(_settings.GetLensSurfaceDirtTexture() ? _settings.GetLensSurfaceDirtTexture().height : _resources.lensSurfaceDirtTextureDefault.height);
                float dsRatio = _cameraData.GetAspect() / dirtRatio;
                float sdRatio = dirtRatio / _cameraData.GetAspect();

                SetVector(PipelineProperties.ShaderProperties.lensSurfaceDirtTexST, dirtRatio > _cameraData.GetAspect() ? 
                          new Vector4(dsRatio, 1, (1f - dsRatio) * 0.5f, 0) :
                          new Vector4(1, sdRatio, 0, (1f - sdRatio) * 0.5f), true);
            }

            //LensFlare
            if(_useLensFlare)
            {
                Presets.SetLensFlarePreset(_settings.GetLensFlareStyle(), _settings);
                SetVector(PipelineProperties.ShaderProperties.lensFlareThreshold, _settings.GetWorkflow() == Workflow.Selective ? _selectiveWorkflowThreshold : new Vector2(ConvertGammaValue(_settings.GetLensFlareThreshold().minValue), ConvertGammaValue(_settings.GetLensFlareThreshold().maxValue)), _debugView == DebugView.RawLensFlare ? true : false);
                SetVector(PipelineProperties.ShaderProperties.lensFlareGhostParams, new Vector4(_settings.GetLensFlareGhostCount(), _settings.GetLensFlareGhostDispersal(), _settings.GetLensFlareGhostFade(), ConvertGammaValue(_settings.GetLensFlareGhostIntensity() * (_settings.GetWorkflow() == Workflow.Natural ? naturalIntensityMult : 1f))));
                SetVector(PipelineProperties.ShaderProperties.lensFlareHaloParams, new Vector3(_settings.GetLensFlareHaloSize(), _settings.GetLensFlareHaloFade(), ConvertGammaValue(_settings.GetLensFlareHaloIntensity() * (_settings.GetWorkflow() == Workflow.Natural ? naturalIntensityMult : 1f))));
                SetFloat(PipelineProperties.ShaderProperties.lensFlareSpread, _lensFlareUpsampleSpread);
                SetFloat(PipelineProperties.ShaderProperties.lensFlareChromaticAberration, _settings.GetLensFlareChromaticAberration(), true);
            }

            //Glare
            if(_useGlare)
            {
                Presets.SetGlarePreset(_settings.GetGlareStyle(), _settings);
                SetVector(PipelineProperties.ShaderProperties.glareThreshold, _settings.GetWorkflow() == Workflow.Selective ? _selectiveWorkflowThreshold : new Vector2(ConvertGammaValue(_settings.GetGlareThreshold().minValue), ConvertGammaValue(_settings.GetGlareThreshold().maxValue)), _debugView == DebugView.RawGlare ? true : false);

                SetFloat(PipelineProperties.ShaderProperties.glareBlend, _settings.GetGlareBlend(), true);
                SetVector(PipelineProperties.ShaderProperties.glareIntensity, ConvertGammaValue(new Vector4(_settings.GetGlareSample0Intensity(), _settings.GetGlareSample1Intensity(), _settings.GetGlareSample2Intensity(), _settings.GetGlareSample3Intensity())), true);

                Vector4 Scattering = new Vector4(_settings.GetGlareSample0Scattering() * _glareScatteringMult * _settings.GetGlareScattering(), _settings.GetGlareSample1Scattering() * _glareScatteringMult * _settings.GetGlareScattering(), _settings.GetGlareSample2Scattering() * _glareScatteringMult * _settings.GetGlareScattering(), _settings.GetGlareSample3Scattering() * _glareScatteringMult * _settings.GetGlareScattering());
                glareAngles[0] = AngleToDirection(_settings.GetGlareSample0Angle() + _settings.GetGlareAngle());
                glareAngles[1] = AngleToDirection(_settings.GetGlareSample1Angle() + _settings.GetGlareAngle());
                glareAngles[2] = AngleToDirection(_settings.GetGlareSample2Angle() + _settings.GetGlareAngle());
                glareAngles[3] = AngleToDirection(_settings.GetGlareSample3Angle() + _settings.GetGlareAngle());
                Vector4 direction01 = new Vector4(glareAngles[0].x, glareAngles[0].y, glareAngles[1].x, glareAngles[1].y);
                Vector4 direction02 = new Vector4(glareAngles[2].x, glareAngles[2].y, glareAngles[3].x, glareAngles[3].y);
                Vector4 offset = new Vector4(_settings.GetGlareSample0Offset(), _settings.GetGlareSample1Offset(), _settings.GetGlareSample2Offset(), _settings.GetGlareSample3Offset());

                SetVector(PipelineProperties.ShaderProperties.glareScattering, Scattering);
                SetVector(PipelineProperties.ShaderProperties.glareDirection01, direction01);
                SetVector(PipelineProperties.ShaderProperties.glareDirection23, direction02);
                SetVector(PipelineProperties.ShaderProperties.glareOffset, offset);

                SetVector(PipelineProperties.ShaderProperties.glareScattering, Scattering, true);
                SetVector(PipelineProperties.ShaderProperties.glareDirection01, direction01, true);
                SetVector(PipelineProperties.ShaderProperties.glareDirection23, direction02, true);
                SetVector(PipelineProperties.ShaderProperties.glareOffset, offset, true);
                SetFloat(PipelineProperties.ShaderProperties.glareGlobalIntensity, ConvertGammaValue(_settings.GetGlareIntensity()) * (_settings.GetWorkflow() == Workflow.Natural ? naturalIntensityMult : 1f), true);
            }

            if(_useComputeShaders)
                _cArgsComputeBuffer.SetData(_cArgArray);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////
        // Commandbuffer helpers
        /////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Set a specific keyword for the pixelshader
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="enable"></param>
        private void SetKeyword(MaterialKeywords keyword, bool enable)
        {
            //For now disable check if a keyword is already set
            //to make sure the cmd is always correctly setuped
            //if(_shaderKeywords[(int)keyword].enabled != enable)
            {
                if(_renderPipeline == RenderPipeline.SRP)
                    _commandBuffer.SetKeyword(_shaderKeywords[(int)keyword].name, enable);
                else
                    PipelineExtensions.SetKeyword(_shaderKeywords[(int)keyword].name, enable);
                _shaderKeywords[(int)keyword].enabled = enable;
            }
        }

        /// <summary>
        /// Convert an angle (degree) to a Vector2 direction
        /// </summary>
        /// <returns></returns>
        private Vector2 AngleToDirection(float angleDegree)
        {
            return new Vector2(Mathf.Sin(angleDegree * Mathf.Deg2Rad), Mathf.Cos(angleDegree * Mathf.Deg2Rad));
        }

        /// <summary>
        /// get a threshold value based on current color space
        /// </summary>
        private float ConvertGammaValue(float gammaSpacedValue)
        {
            if(QualitySettings.activeColorSpace == ColorSpace.Linear)
            {
                return Mathf.GammaToLinearSpace(gammaSpacedValue);
            }
            else
                return gammaSpacedValue;
        }

        /// <summary>
        /// get a threshold value based on current color space
        /// </summary>
        private Vector4 ConvertGammaValue(Vector4 gammaSpacedVector)
        {
            if(QualitySettings.activeColorSpace == ColorSpace.Linear)
            {
                gammaSpacedVector.x = ConvertGammaValue(gammaSpacedVector.x);
                gammaSpacedVector.y = ConvertGammaValue(gammaSpacedVector.y);
                gammaSpacedVector.z = ConvertGammaValue(gammaSpacedVector.z);
                gammaSpacedVector.w = ConvertGammaValue(gammaSpacedVector.w);
                return gammaSpacedVector;
            }
            else
                return gammaSpacedVector;
        }

        /// <summary>
        /// Get the needed Threadgroups forcompute shaders
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        private void UpdateComputeShaderThreadGroups(RenderDimension renderDimension)
        {
            _computeThreadGroups.width = Mathf.Max(1, Mathf.FloorToInt((renderDimension.width + _directComputeSize.height) / _directComputeSize.width));
            _computeThreadGroups.height = Mathf.Max(1, Mathf.FloorToInt((renderDimension.height + _directComputeSize.height) / _directComputeSize.width));
        }
        
        /// <summary>
        /// Update the renderindex (pass) forthe next Draw
        /// </summary>
        /// <param name="v"></param>
        private void UpdateRenderIndex(int v)
        {
            _currentRenderIndex = v;
        }

        /// <summary>
        /// Update the renderindex (compute kernel) for the next Draw
        /// </summary>
        /// <param name="variants"></param>
        /// <param name="features"></param>
        private void UpdateRenderIndex(ComputeShaderVariants variants, ComputeShaderVariants.KeywordState features)
        {
            variants.GetVariantNumber(features, out _currentRenderIndex);
        }

        /// <summary>
        /// Attach CArgs to currently used kernel
        /// </summary>
        private void AttachCArgBufferToComputeKernel()
        {
            if(_renderPipeline == RenderPipeline.SRP)
                _commandBuffer.SetComputeBufferParam(_resources.computeShader, _currentRenderIndex, ShaderProperties.cArgBuffer.id, _cArgsComputeBuffer);
            else
                _resources.computeShader.SetBuffer(_currentRenderIndex, ShaderProperties.cArgBuffer.id, _cArgsComputeBuffer);
        }

        /// <summary>
        /// Auto set a float value on the renderpipeline
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        private void SetFloat(ShaderProperties.CBufferProperty property, float value, bool forcePixelShader = false)
        {
            if(_useComputeShaders && !forcePixelShader)
                _cArgArray[property.index] = value;
            else
                if(_renderPipeline == RenderPipeline.SRP)
                    _commandBuffer.SetGlobalFloat(property.id, value);
                else
                    Shader.SetGlobalFloat(property.id, value);
        }

        /// <summary>
        /// Auto set a vector value on the renderpipeline
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        private void SetVector(ShaderProperties.CBufferProperty property, Vector4 value, bool forcePixelShader = false)
        {
            if(_useComputeShaders && !forcePixelShader)
            {
                _cArgArray[property.index] = value.x;
                _cArgArray[property.index + 1] = value.y;
                _cArgArray[property.index + 2] = value.z;
                _cArgArray[property.index + 3] = value.w;
            }
            else
                if(_renderPipeline == RenderPipeline.SRP)
                    _commandBuffer.SetGlobalVector(property.id, value);
                else
                    Shader.SetGlobalVector(property.id, value);
        }

        /// <summary>
        /// Auto set a vector value on the renderpipeline
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        private void SetVector(ShaderProperties.CBufferProperty property, Vector3 value, bool forcePixelShader = false)
        {
            if(_useComputeShaders && !forcePixelShader)
            {
                _cArgArray[property.index] = value.x;
                _cArgArray[property.index + 1] = value.y;
                _cArgArray[property.index + 2] = value.z;
            }
            else
                if(_renderPipeline == RenderPipeline.SRP)
                    _commandBuffer.SetGlobalVector(property.id, value);
                else
                    Shader.SetGlobalVector(property.id, value);
        }

        /// <summary>
        /// Auto set a vector value on the renderpipeline
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        private void SetVector(ShaderProperties.CBufferProperty property, Vector2 value, bool forcePixelShader = false)
        {
            if(_useComputeShaders && !forcePixelShader)
            {
                _cArgArray[property.index] = value.x;
                _cArgArray[property.index + 1] = value.y;
            }
            else
                if(_renderPipeline == RenderPipeline.SRP)
                    _commandBuffer.SetGlobalVector(property.id, value);
                else
                    Shader.SetGlobalVector(property.id, value);
        }

        /// <summary>
        /// Auto set a texture on the renderpipeline, 
        /// always update the computeKernelIndexBuffer before using this to get the correct variant while using compute shaders
        /// </summary>
        /// <param name="property"></param>
        /// <param name="rt"></param>
        /// <param name="forcePixelShader"></param>
        private void SetTexture(ShaderProperties.DefaultProperty property, RenderTarget rt, bool forcePixelShader = false)
        {
            if(_useComputeShaders && !forcePixelShader)
                if(_renderPipeline == RenderPipeline.SRP)
                    _commandBuffer.SetComputeTextureParam(_resources.computeShader, _currentRenderIndex, property.id, rt.renderTargetIdentifier);
                else
                    _resources.computeShader.SetTexture(_currentRenderIndex, property.id, rt.renderTexture);
            else
                if(_renderPipeline == RenderPipeline.SRP)
                    _commandBuffer.SetGlobalTexture(property.id, rt.renderTargetIdentifier);
                else
                    Shader.SetGlobalTexture(property.id, rt.renderTexture);
        }
        private void SetTexture(ShaderProperties.DefaultProperty property, Texture tex, bool forcePixelShader = false)
        {
            if(_useComputeShaders && !forcePixelShader)
                if(_renderPipeline == RenderPipeline.SRP)
                    _commandBuffer.SetComputeTextureParam(_resources.computeShader, _currentRenderIndex, property.id, tex);
                else
                    _resources.computeShader.SetTexture(_currentRenderIndex, property.id, tex);
            else
                if(_renderPipeline == RenderPipeline.SRP)
                    _commandBuffer.SetGlobalTexture(property.id, tex);
                else
                    Shader.SetGlobalTexture(property.id, tex);
        }
        
        /// <summary>
        /// Setup for the next draw command
        /// </summary>
        /// <param name="variant"></param>
        /// <param name="renderDimension"></param>
        /// <param name="forcePixelShader"></param>
        private void PrepareDraw(int variant, RenderDimension renderDimension, bool forcePixelShader = false)
        {
            if(_useComputeShaders && !forcePixelShader)
            {
                UpdateRenderIndex(variant);
                AttachCArgBufferToComputeKernel();
                UpdateComputeShaderThreadGroups(renderDimension);
            }
            else
            {
                SetRenderPriority();
                UpdateRenderIndex(variant);
                DisableRenderKeywords();
                foreach(MaterialKeywords kw in _renderKeywordsBundle)
                    SetKeyword(kw, true);
                _renderKeywordsBundle.Clear();
            }
        }

        /// <summary>
        /// Setup for the next draw command
        /// </summary>
        /// <param name="materialPass"></param>
        /// <param name="variants"></param>
        /// <param name="features"></param>
        /// <param name="renderDimension"></param>
        private void PrepareDraw(int materialPass, ComputeShaderVariants variants, bool enableBloom, bool enableLensflare, bool enableGlare, RenderDimension renderDimension)
        {
            if(_useComputeShaders)
            {
                computeShaderFeatures.bloom = enableBloom ? 1 : 0;
                computeShaderFeatures.lensSurface = _settings.GetAllowLensSurface() ? 1 : 0;
                computeShaderFeatures.lensFlare = enableLensflare ? 1 : 0;
                computeShaderFeatures.glare = enableGlare ? _settings.GetGlareStreaks() : 0;
                computeShaderFeatures.natural = (int)_settings.GetWorkflow() == 2 ? 1 : 0;
                computeShaderFeatures.renderPriority = (int)_settings.GetRenderPriority() <= 0 ? 0 : (int)_settings.GetRenderPriority() == 1 ? 1 : 2;

                UpdateRenderIndex(variants, computeShaderFeatures);
                AttachCArgBufferToComputeKernel();
                UpdateComputeShaderThreadGroups(renderDimension);
            }
            else
            {
                SetRenderPriority();
                UpdateRenderIndex(materialPass);
                DisableRenderKeywords();
                foreach(MaterialKeywords kw in _renderKeywordsBundle)
                    SetKeyword(kw, true);
                _renderKeywordsBundle.Clear();
            }
        }

        /// <summary>
        /// Draw into a destination framebuffer based on shadertype
        /// Always prepare for drawing using the PrepareDraw command
        /// </summary>
        /// <param name="forcePixelShader"></param>
        private void Draw(RenderDimension dimension, bool forcePixelShader = false)
        {
            if(_renderPipeline == RenderPipeline.SRP)
            {
                if(_useComputeShaders && !forcePixelShader)
                    _commandBuffer.Draw(_renderTargetsBundle, _resources.computeShader, _currentRenderIndex, _computeThreadGroups);
                else
                    _commandBuffer.Draw(_renderTargetsBundle, _useGeometryShaders ? _renderMaterialGeometry : _renderMaterialNoGeometry, _useGeometryShaders, _currentRenderIndex, new Rect(0, 0, dimension.width, dimension.height));
            }
            else
            {
                if(_useComputeShaders && !forcePixelShader)
                    PipelineExtensions.Draw(_renderTargetsBundle, _resources.computeShader, _currentRenderIndex, _computeThreadGroups);
                else
                    PipelineExtensions.Draw(_renderTargetsBundle, _useGeometryShaders ? _renderMaterialGeometry : _renderMaterialNoGeometry, _useGeometryShaders, _currentRenderIndex);
            }
            _renderTargetsBundle.Clear();
        } 

        /////////////////////////////////////////////////////////////////////////////////////////////
        // Sampling
        /////////////////////////////////////////////////////////////////////////////////////////////
         
        private MaterialKeywords GetGlareKeyword(int streaks)
        {
            switch(streaks)
            {
                case 1:
                    return MaterialKeywords.Glare1;
                case 2:
                    return MaterialKeywords.Glare2;
                case 3:
                    return MaterialKeywords.Glare3;
                case 4:
                    return MaterialKeywords.Glare4;
                default:
                    return MaterialKeywords.Null;
            }
        }

        /// <summary>
        /// Disable render Keywords
        /// </summary>
        private void DisableRenderKeywords()
        {
            SetKeyword(MaterialKeywords.Bloom, false);
            SetKeyword(MaterialKeywords.LensSurface, false);
            SetKeyword(MaterialKeywords.LensFlare, false);
            SetKeyword(MaterialKeywords.Glare1, false);
            SetKeyword(MaterialKeywords.Glare2, false);
            SetKeyword(MaterialKeywords.Glare3, false);
            SetKeyword(MaterialKeywords.Glare4, false);
            SetKeyword(MaterialKeywords.RenderPriorityBalanced, false);
            SetKeyword(MaterialKeywords.RenderPriorityQuality, false);
            SetKeyword(MaterialKeywords.Natural, false);
            SetKeyword(MaterialKeywords.HQAntiFlickerFilter, false);
        }

        /// <summary>
        /// Disable debug Keywords
        /// </summary>
        private void DisableDebugKeywords()
        {
            SetKeyword(MaterialKeywords.DebugRawBloom, false);
            SetKeyword(MaterialKeywords.DebugRawLensFlare, false);
            SetKeyword(MaterialKeywords.DebugRawGlare, false);
            SetKeyword(MaterialKeywords.DebugBloom, false);
            SetKeyword(MaterialKeywords.DebugLensFlare, false);
            SetKeyword(MaterialKeywords.DebugGlare, false);
            SetKeyword(MaterialKeywords.DebugComposite, false);
        }

        private void SetRenderPriority()
        {
            if(_settings.GetRenderPriority() == RenderPriority.Quality)
            {
                _renderKeywordsBundle.Add(MaterialKeywords.RenderPriorityQuality);
            }
            else if(_settings.GetRenderPriority() == RenderPriority.Balanced)
            {
                _renderKeywordsBundle.Add(MaterialKeywords.RenderPriorityBalanced);
            }
            else
            {
                
            }
        }
        
        /// <summary>
        /// Pre sample the glow map
        /// </summary>
        private void PreSample()
        {
            BeginProfileSample(PipelineProperties.CommandBufferProperties.samplePreSample);

            _bloomDownsampleBuffer.CreateTemporary(_renderContext, 0, _commandBuffer, _renderTextureFormat, _useComputeShaders, _renderPipeline);
            if(_settings.GetAntiFlickerMode() == AntiFlickerMode.Strong)
                _renderKeywordsBundle.Add(MaterialKeywords.HQAntiFlickerFilter);
            _renderKeywordsBundle.Add(MaterialKeywords.Bloom);
            if(_settings.GetWorkflow() == Workflow.Natural)
                _renderKeywordsBundle.Add(MaterialKeywords.Natural);
            _renderTargetsBundle.Add(_bloomDownsampleBuffer.renderTargets[0]);
            
            if(_useLensFlare)
            {
                _lensFlareDownsampleBuffer.CreateTemporary(_renderContext, 0, _commandBuffer, _renderTextureFormat, _useComputeShaders, _renderPipeline);
                _renderKeywordsBundle.Add(MaterialKeywords.LensFlare);
                _renderTargetsBundle.Add(_lensFlareDownsampleBuffer.renderTargets[0]);
            }
            if(_useGlare)
            {
                _glareDownsampleBuffer0.CreateTemporary(_renderContext, 0, _commandBuffer, _renderTextureFormat, _useComputeShaders, _renderPipeline);
                _glareDownsampleBuffer1.CreateTemporary(_renderContext, 0, _commandBuffer, _renderTextureFormat, _useComputeShaders, _renderPipeline);
                _glareDownsampleBuffer2.CreateTemporary(_renderContext, 0, _commandBuffer, _renderTextureFormat, _useComputeShaders, _renderPipeline);
                _glareDownsampleBuffer3.CreateTemporary(_renderContext, 0, _commandBuffer, _renderTextureFormat, _useComputeShaders, _renderPipeline);
                _renderKeywordsBundle.Add(GetGlareKeyword(_settings.GetGlareStreaks()));
                _renderTargetsBundle.Add(_glareDownsampleBuffer0.renderTargets[0]);
                _renderTargetsBundle.Add(_glareDownsampleBuffer1.renderTargets[0]);
                _renderTargetsBundle.Add(_glareDownsampleBuffer2.renderTargets[0]);
                _renderTargetsBundle.Add(_glareDownsampleBuffer3.renderTargets[0]);
            }

            PrepareDraw
            (   
                (int)ShaderRenderPass.Presample,
                null, //_presampleComputeVariants, 
                true, _useLensFlare, _useGlare,
                _renderContext[0].renderDimension
            );

            if(_useComputeShaders)
                SetTexture(PipelineProperties.ShaderProperties.bloomTargetTex, _bloomDownsampleBuffer.renderTargets[0]);

            if(_settings.GetWorkflow() == Workflow.Selective)
                SetTexture(PipelineProperties.ShaderProperties.sourceTex, _selectiveRenderTarget.renderTexture);
            else
                SetTexture(PipelineProperties.ShaderProperties.sourceTex, sourceFrameBuffer);
            
            //SetTexture(PipelineProperties.ShaderProperties.sourceTex, _settings.GetWorkflow() == Workflow.Threshold ? sourceFrameBuffer : _selectiveRenderTarget);

            if(_useLensFlare)
            {
                SetTexture(PipelineProperties.ShaderProperties.lensFlareColorRamp, _settings.GetLensFlareColorRamp() ? _settings.GetLensFlareColorRamp() : _resources.lensFlareColorRampDefault);
                if(_useComputeShaders)
                    SetTexture(PipelineProperties.ShaderProperties.lensFlareTargetTex, _lensFlareDownsampleBuffer.renderTargets[0]);
            }

            if(_useGlare)
            {
                if(_useComputeShaders)
                {
                    SetTexture(PipelineProperties.ShaderProperties.glare0TargetTex, _glareDownsampleBuffer0.renderTargets[0]);
                    SetTexture(PipelineProperties.ShaderProperties.glare1TargetTex, _glareDownsampleBuffer1.renderTargets[0]);
                    SetTexture(PipelineProperties.ShaderProperties.glare2TargetTex, _glareDownsampleBuffer2.renderTargets[0]);
                    SetTexture(PipelineProperties.ShaderProperties.glare3TargetTex, _glareDownsampleBuffer3.renderTargets[0]);
                }
            }
            Draw(_renderContext[0].renderDimension);

            if(_settings.GetWorkflow() == Workflow.Selective)
                RenderTexture.ReleaseTemporary(_selectiveRenderTarget.renderTexture);

            EndProfileSample(PipelineProperties.CommandBufferProperties.samplePreSample);
        }

        /// <summary>
        /// Downsample the glow map
        /// </summary>
        private void Downsample()
        {
            BeginProfileSample(PipelineProperties.CommandBufferProperties.sampleDownsample);

            bool enableBloom, enableLensFlare, enableGlare;
            for(int i = 0; i < _minIterations; i++)
            {
                enableBloom = i < _bloomIterations;
                enableLensFlare = _useLensFlare && i < _lensFlareIterations;
                enableGlare = _useGlare && i < _glareIterations;

                if(enableBloom)
                {
                    _bloomDownsampleBuffer.CreateTemporary(_renderContext, i + 1, _commandBuffer, _renderTextureFormat, _useComputeShaders, _renderPipeline);
                    _renderKeywordsBundle.Add(MaterialKeywords.Bloom);
                    _renderTargetsBundle.Add(_bloomDownsampleBuffer.renderTargets[i + 1]);
                }
                if(enableLensFlare)
                {
                    _lensFlareDownsampleBuffer.CreateTemporary(_renderContext, i + 1, _commandBuffer, _renderTextureFormat, _useComputeShaders, _renderPipeline);
                    _renderKeywordsBundle.Add(MaterialKeywords.LensFlare);
                    _renderTargetsBundle.Add(_lensFlareDownsampleBuffer.renderTargets[i + 1]);
                }
                if(enableGlare)
                {
                    _glareDownsampleBuffer0.CreateTemporary(_renderContext, i + 1, _commandBuffer, _renderTextureFormat, _useComputeShaders, _renderPipeline);
                    _glareDownsampleBuffer1.CreateTemporary(_renderContext, i + 1, _commandBuffer, _renderTextureFormat, _useComputeShaders, _renderPipeline);
                    _glareDownsampleBuffer2.CreateTemporary(_renderContext, i + 1, _commandBuffer, _renderTextureFormat, _useComputeShaders, _renderPipeline);
                    _glareDownsampleBuffer3.CreateTemporary(_renderContext, i + 1, _commandBuffer, _renderTextureFormat, _useComputeShaders, _renderPipeline);
                    _renderKeywordsBundle.Add(GetGlareKeyword(_settings.GetGlareStreaks()));
                    _renderTargetsBundle.Add(_glareDownsampleBuffer0.renderTargets[i + 1]);
                    _renderTargetsBundle.Add(_glareDownsampleBuffer1.renderTargets[i + 1]);
                    _renderTargetsBundle.Add(_glareDownsampleBuffer2.renderTargets[i + 1]);
                    _renderTargetsBundle.Add(_glareDownsampleBuffer3.renderTargets[i + 1]);
                }

                PrepareDraw
                (   
                    (int)ShaderRenderPass.Downsample,
                    null, //_downsampleComputeVariants,
                    enableBloom, enableLensFlare, enableGlare, 
                    _renderContext[i + 1].renderDimension
                );
                    
                if(enableBloom)
                {
                    
                    SetTexture(PipelineProperties.ShaderProperties.bloomTex, _bloomDownsampleBuffer.renderTargets[i]);
                    if(_useComputeShaders)
                        SetTexture(PipelineProperties.ShaderProperties.bloomTargetTex, _bloomDownsampleBuffer.renderTargets[i + 1]);
                }

                if(enableLensFlare)
                {
                    
                    SetTexture(PipelineProperties.ShaderProperties.lensFlareTex, _lensFlareDownsampleBuffer.renderTargets[i]);
                    if(_useComputeShaders)
                        SetTexture(PipelineProperties.ShaderProperties.lensFlareTargetTex, _lensFlareDownsampleBuffer.renderTargets[i + 1]);
                }

                if(enableGlare)
                {
                    SetTexture(PipelineProperties.ShaderProperties.glare0Tex, _glareDownsampleBuffer0.renderTargets[i]);
                    SetTexture(PipelineProperties.ShaderProperties.glare1Tex, _glareDownsampleBuffer1.renderTargets[i]);
                    SetTexture(PipelineProperties.ShaderProperties.glare2Tex, _glareDownsampleBuffer2.renderTargets[i]);
                    SetTexture(PipelineProperties.ShaderProperties.glare3Tex, _glareDownsampleBuffer3.renderTargets[i]);
                    if(_useComputeShaders)
                    {
                        SetTexture(PipelineProperties.ShaderProperties.glare0TargetTex, _glareDownsampleBuffer0.renderTargets[i + 1]);
                        SetTexture(PipelineProperties.ShaderProperties.glare1TargetTex, _glareDownsampleBuffer1.renderTargets[i + 1]);
                        SetTexture(PipelineProperties.ShaderProperties.glare2TargetTex, _glareDownsampleBuffer2.renderTargets[i + 1]);
                        SetTexture(PipelineProperties.ShaderProperties.glare3TargetTex, _glareDownsampleBuffer3.renderTargets[i + 1]);
                    }
                }

                Draw(_renderContext[i + 1].renderDimension);
            }
            EndProfileSample(PipelineProperties.CommandBufferProperties.sampleDownsample);
        }


        /// <summary>
        /// Upsample the glow map
        /// </summary>
        private void Upsample()
        {
            BeginProfileSample(PipelineProperties.CommandBufferProperties.sampleUpsample);

            bool enableBloom, enableLensFlare, enableGlare;
            for(int i = _minIterations; i > 0; i--)
            {   
                enableBloom = i <= _bloomIterations;
                enableLensFlare = _useLensFlare && i <= _lensFlareIterations;
                enableGlare = _useGlare && i <= _glareIterations;

                if(enableBloom)
                {
                    _bloomUpsampleBuffer.CreateTemporary(_renderContext, i - 1, _commandBuffer, _renderTextureFormat, _useComputeShaders, _renderPipeline);
                    _renderKeywordsBundle.Add(MaterialKeywords.Bloom);
                    _renderTargetsBundle.Add(_bloomUpsampleBuffer.renderTargets[i - 1]);
                }
                if(enableLensFlare)
                {
                    _lensFlareUpsampleBuffer.CreateTemporary(_renderContext, i - 1, _commandBuffer, _renderTextureFormat, _useComputeShaders, _renderPipeline);
                    _renderKeywordsBundle.Add(MaterialKeywords.LensFlare);
                    _renderTargetsBundle.Add(_lensFlareUpsampleBuffer.renderTargets[i - 1]);
                }
                if(enableGlare)
                {
                    if(_settings.GetGlareStreaks() >= 1)
                    {
                        _glareUpsampleBuffer0.CreateTemporary(_renderContext, i - 1, _commandBuffer, _renderTextureFormat, _useComputeShaders, _renderPipeline);
                        _renderTargetsBundle.Add(_glareUpsampleBuffer0.renderTargets[i - 1]);
                    }
                    if(_settings.GetGlareStreaks() >= 2)
                    {
                        _glareUpsampleBuffer1.CreateTemporary(_renderContext, i - 1, _commandBuffer, _renderTextureFormat, _useComputeShaders, _renderPipeline);
                        _renderTargetsBundle.Add(_glareUpsampleBuffer1.renderTargets[i - 1]);
                    }
                    if(_settings.GetGlareStreaks() >= 3)
                    {
                        _glareUpsampleBuffer2.CreateTemporary(_renderContext, i - 1, _commandBuffer, _renderTextureFormat, _useComputeShaders, _renderPipeline);
                        _renderTargetsBundle.Add(_glareUpsampleBuffer2.renderTargets[i - 1]);
                    }
                    if(_settings.GetGlareStreaks() >= 4)
                    {
                        _glareUpsampleBuffer3.CreateTemporary(_renderContext, i - 1, _commandBuffer, _renderTextureFormat, _useComputeShaders, _renderPipeline);
                        _renderTargetsBundle.Add(_glareUpsampleBuffer3.renderTargets[i - 1]);
                    }
                    _renderKeywordsBundle.Add(GetGlareKeyword(_settings.GetGlareStreaks()));
                }

                PrepareDraw
                (   
                    (int)ShaderRenderPass.Upsample, 
                    null, //_upsampleComputeVariants, 
                    enableBloom, enableLensFlare, enableGlare, 
                    _renderContext[i - 1].renderDimension
                );

                if(enableBloom)
                {
                    SetTexture(PipelineProperties.ShaderProperties.higherMipBloomTex, _bloomDownsampleBuffer.renderTargets[i - 1]);
                    SetTexture(PipelineProperties.ShaderProperties.bloomTex, (i >= _bloomIterations) ? _bloomDownsampleBuffer.renderTargets[i] : _bloomUpsampleBuffer.renderTargets[i]);
                    if(_useComputeShaders)
                        SetTexture(PipelineProperties.ShaderProperties.bloomTargetTex, _bloomUpsampleBuffer.renderTargets[i - 1]);
                }

                if(enableLensFlare)
                {
                    SetTexture(PipelineProperties.ShaderProperties.lensFlareTex, (i >= _lensFlareIterations) ? _lensFlareDownsampleBuffer.renderTargets[i] : _lensFlareUpsampleBuffer.renderTargets[i]);
                    if(_useComputeShaders)
                        SetTexture(PipelineProperties.ShaderProperties.lensFlareTargetTex, _lensFlareUpsampleBuffer.renderTargets[i - 1]);
                }

                if(enableGlare)
                {
                    if(_settings.GetGlareStreaks() >= 1)
                        SetTexture(PipelineProperties.ShaderProperties.glare0Tex, (i >= _glareIterations) ? _glareDownsampleBuffer0.renderTargets[i] : _glareUpsampleBuffer0.renderTargets[i]);
                    if(_settings.GetGlareStreaks() >= 2)
                        SetTexture(PipelineProperties.ShaderProperties.glare1Tex, (i >= _glareIterations) ? _glareDownsampleBuffer1.renderTargets[i] : _glareUpsampleBuffer1.renderTargets[i]);
                    if(_settings.GetGlareStreaks() >= 3)
                        SetTexture(PipelineProperties.ShaderProperties.glare2Tex, (i >= _glareIterations) ? _glareDownsampleBuffer2.renderTargets[i] : _glareUpsampleBuffer2.renderTargets[i]);
                    if(_settings.GetGlareStreaks() >= 4)
                        SetTexture(PipelineProperties.ShaderProperties.glare3Tex, (i >= _glareIterations) ? _glareDownsampleBuffer3.renderTargets[i] : _glareUpsampleBuffer3.renderTargets[i]);

                    if(_useComputeShaders)
                    {   
                        if(_settings.GetGlareStreaks() >= 1)
                            SetTexture(PipelineProperties.ShaderProperties.glare0TargetTex, _glareUpsampleBuffer0.renderTargets[i - 1]);
                        if(_settings.GetGlareStreaks() >= 2)
                            SetTexture(PipelineProperties.ShaderProperties.glare1TargetTex, _glareUpsampleBuffer1.renderTargets[i - 1]);
                        if(_settings.GetGlareStreaks() >= 3)
                            SetTexture(PipelineProperties.ShaderProperties.glare2TargetTex, _glareUpsampleBuffer2.renderTargets[i - 1]);
                        if(_settings.GetGlareStreaks() >= 4)
                            SetTexture(PipelineProperties.ShaderProperties.glare3TargetTex, _glareUpsampleBuffer3.renderTargets[i - 1]);
                    }
                }

                Draw(_renderContext[i - 1].renderDimension);

                if(enableBloom)
                {
                    if(i >= _bloomIterations)
                        _bloomDownsampleBuffer.ClearTemporary(_commandBuffer, i, _renderPipeline);
                    else
                    {
                        _bloomDownsampleBuffer.ClearTemporary(_commandBuffer, i, _renderPipeline);
                        _bloomUpsampleBuffer.ClearTemporary(_commandBuffer, i, _renderPipeline);
                    }
                }
                if(enableLensFlare)
                {
                    if(i >= _lensFlareIterations)
                        _lensFlareDownsampleBuffer.ClearTemporary(_commandBuffer, i, _renderPipeline);
                    else
                    {
                        _lensFlareDownsampleBuffer.ClearTemporary(_commandBuffer, i, _renderPipeline);
                        _lensFlareUpsampleBuffer.ClearTemporary(_commandBuffer, i, _renderPipeline);
                    }
                }
                if(enableGlare)
                {
                    if(i >= _glareIterations)
                    {
                        _glareDownsampleBuffer0.ClearTemporary(_commandBuffer, i, _renderPipeline);
                        _glareDownsampleBuffer1.ClearTemporary(_commandBuffer, i, _renderPipeline);
                        _glareDownsampleBuffer2.ClearTemporary(_commandBuffer, i, _renderPipeline);
                        _glareDownsampleBuffer3.ClearTemporary(_commandBuffer, i, _renderPipeline);
                    }
                    else
                    {
                        _glareDownsampleBuffer0.ClearTemporary(_commandBuffer, i, _renderPipeline);
                        _glareDownsampleBuffer1.ClearTemporary(_commandBuffer, i, _renderPipeline);
                        _glareDownsampleBuffer2.ClearTemporary(_commandBuffer, i, _renderPipeline);
                        _glareDownsampleBuffer3.ClearTemporary(_commandBuffer, i, _renderPipeline);

                        if(_settings.GetGlareStreaks() >= 1)
                            _glareUpsampleBuffer0.ClearTemporary(_commandBuffer, i, _renderPipeline);
                        if(_settings.GetGlareStreaks() >= 2)
                            _glareUpsampleBuffer1.ClearTemporary(_commandBuffer, i, _renderPipeline);
                        if(_settings.GetGlareStreaks() >= 3)
                            _glareUpsampleBuffer2.ClearTemporary(_commandBuffer, i, _renderPipeline);
                        if(_settings.GetGlareStreaks() >= 4)
                            _glareUpsampleBuffer3.ClearTemporary(_commandBuffer, i, _renderPipeline);
                    }
                }
            }

            _bloomDownsampleBuffer.ClearTemporary(_commandBuffer, 0, _renderPipeline);
            if(_useLensFlare)
                _lensFlareDownsampleBuffer.ClearTemporary(_commandBuffer, 0, _renderPipeline);
            if(_useGlare)
            {
                _glareDownsampleBuffer0.ClearTemporary(_commandBuffer, 0, _renderPipeline);
                _glareDownsampleBuffer1.ClearTemporary(_commandBuffer, 0, _renderPipeline);
                _glareDownsampleBuffer2.ClearTemporary(_commandBuffer, 0, _renderPipeline);
                _glareDownsampleBuffer3.ClearTemporary(_commandBuffer, 0, _renderPipeline);
            }

            EndProfileSample(PipelineProperties.CommandBufferProperties.sampleUpsample);
        }

        /// <summary>
        /// Precomposite of the glow map
        /// </summary>
        private void Composite()
        {
            BeginProfileSample(PipelineProperties.CommandBufferProperties.sampleComposite);

            int renderpass;
            
            switch(_debugView)
            {
                case DebugView.RawBloom:
                    _renderKeywordsBundle.Add(MaterialKeywords.DebugRawBloom);
                    renderpass = (int)ShaderRenderPass.Debug;
                break;
                case DebugView.RawLensFlare:
                    _renderKeywordsBundle.Add(MaterialKeywords.DebugRawLensFlare);
                    renderpass = (int)ShaderRenderPass.Debug;
                break;
                case DebugView.RawGlare:
                    _renderKeywordsBundle.Add(MaterialKeywords.DebugRawGlare);
                    renderpass = (int)ShaderRenderPass.Debug;
                break;
                case DebugView.Bloom:
                    _renderKeywordsBundle.Add(MaterialKeywords.DebugBloom);
                    renderpass = (int)ShaderRenderPass.Debug;
                break;
                case DebugView.LensFlare:
                    _renderKeywordsBundle.Add(MaterialKeywords.DebugLensFlare);
                    renderpass = (int)ShaderRenderPass.Debug;
                break;
                case DebugView.Glare:
                    _renderKeywordsBundle.Add(MaterialKeywords.DebugGlare);
                    _renderKeywordsBundle.Add(GetGlareKeyword(_settings.GetGlareStreaks()));
                    renderpass = (int)ShaderRenderPass.Debug;
                break;
                case DebugView.Composite:
                    if(_settings.GetWorkflow() == Workflow.Natural)
                        _renderKeywordsBundle.Add(MaterialKeywords.Natural);
                    if(_useLensSurface)
                    {
                        _renderKeywordsBundle.Add(MaterialKeywords.LensSurface);
                    }
                    if(_useLensFlare)
                    {
                        _renderKeywordsBundle.Add(MaterialKeywords.LensFlare);
                    }
                    if(_useGlare)
                    {
                        _renderKeywordsBundle.Add(GetGlareKeyword(_settings.GetGlareStreaks()));
                    }
                    _renderKeywordsBundle.Add(MaterialKeywords.DebugComposite);
                    renderpass = (int)ShaderRenderPass.Debug;
                break;
                default:
                    if(_settings.GetWorkflow() == Workflow.Natural)
                        _renderKeywordsBundle.Add(MaterialKeywords.Natural);
                    if(_useLensSurface)
                    {
                        _renderKeywordsBundle.Add(MaterialKeywords.LensSurface);
                    }
                    if(_useLensFlare)
                    {
                        _renderKeywordsBundle.Add(MaterialKeywords.LensFlare);
                    }
                    if(_useGlare)
                    {
                        _renderKeywordsBundle.Add(GetGlareKeyword(_settings.GetGlareStreaks()));
                    }
                    renderpass = (int)ShaderRenderPass.Composite;
                break;
            }

            if(_settings.GetWorkflow() == Workflow.Natural)
                _renderKeywordsBundle.Add(MaterialKeywords.Natural);

            PrepareDraw
            (   
                renderpass,
                _sourceContext[0].renderDimension,
                true
            );

            if(_settings.GetWorkflow() == Workflow.Selective && (_debugView == DebugView.RawBloom || _debugView == DebugView.RawLensFlare || _debugView == DebugView.RawGlare))
                SetTexture(PipelineProperties.ShaderProperties.sourceTex, sourceFrameBuffer.renderTexture, true);
            else
            {
                SetTexture(PipelineProperties.ShaderProperties.sourceTex, _sourceFrameBuffer, true);
                SetTexture(PipelineProperties.ShaderProperties.bloomTex, _bloomUpsampleBuffer.renderTargets[0], true);
            }

            if(_useLensSurface)
            {
                SetTexture(PipelineProperties.ShaderProperties.lensSurfaceDirtTex, _settings.GetLensSurfaceDirtTexture() ? _settings.GetLensSurfaceDirtTexture() : _resources.lensSurfaceDirtTextureDefault, true);
                SetTexture(PipelineProperties.ShaderProperties.lensSurfaceDiffractionTex, _settings.GetLensSurfaceDiffractionTexture() ? _settings.GetLensSurfaceDiffractionTexture() : _resources.lensSurfaceDiffractionTextureDefault, true);
            }

            if(_useLensFlare)
            {
                SetTexture(PipelineProperties.ShaderProperties.lensFlareTex, _lensFlareUpsampleBuffer.renderTargets[0], true);
            }

            if(_useGlare)
            {
                if(_settings.GetGlareStreaks() >= 1)
                    SetTexture(PipelineProperties.ShaderProperties.glare0Tex, _glareUpsampleBuffer0.renderTargets[0], true);
                if(_settings.GetGlareStreaks() >= 2)
                    SetTexture(PipelineProperties.ShaderProperties.glare1Tex, _glareUpsampleBuffer1.renderTargets[0], true);
                if(_settings.GetGlareStreaks() >= 3)
                    SetTexture(PipelineProperties.ShaderProperties.glare2Tex, _glareUpsampleBuffer2.renderTargets[0], true);
                if(_settings.GetGlareStreaks() >= 4)
                    SetTexture(PipelineProperties.ShaderProperties.glare3Tex, _glareUpsampleBuffer3.renderTargets[0], true);
            }

            //Dont draw when using legacy render pipeline
            if(_finalBlit)
            {
                _renderTargetsBundle.Add(_destinationFrameBuffer);
                Draw(_sourceContext[0].renderDimension, true);
                AfterCompositeCleanup();
            }
            else
            {
                PipelineExtensions.SetKeyword(_shaderKeywords[(int)MaterialKeywords.LegacyBlit].name, true);
                _renderTargetsBundle.Clear();
            }

            EndProfileSample(PipelineProperties.CommandBufferProperties.sampleComposite);
        }

        /// <summary>
        /// This cleans up the final render step
        /// </summary>
        internal void AfterCompositeCleanup()
        {
            _bloomUpsampleBuffer.ClearTemporary(_commandBuffer, 0, _renderPipeline);
            if(_useLensFlare)
                _lensFlareUpsampleBuffer.ClearTemporary(_commandBuffer, 0, _renderPipeline);
            if(_useGlare)
            {
                if(_settings.GetGlareStreaks() >= 1)
                    _glareUpsampleBuffer0.ClearTemporary(_commandBuffer, 0, _renderPipeline);
                if(_settings.GetGlareStreaks() >= 2)
                    _glareUpsampleBuffer1.ClearTemporary(_commandBuffer, 0, _renderPipeline);
                if(_settings.GetGlareStreaks() >= 3)
                    _glareUpsampleBuffer2.ClearTemporary(_commandBuffer, 0, _renderPipeline);
                if(_settings.GetGlareStreaks() >= 4)
                    _glareUpsampleBuffer3.ClearTemporary(_commandBuffer, 0, _renderPipeline);
            }

            DisableDebugKeywords();
            DisableRenderKeywords();

            if(_renderPipeline == RenderPipeline.Legacy)
                PipelineExtensions.SetKeyword(_shaderKeywords[(int)MaterialKeywords.LegacyBlit].name, false);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////
        // Enum / structs used for rendering
        /////////////////////////////////////////////////////////////////////////////////////////////
        /// 
        /// <summary>
        /// Rendering passes for shaders
        /// </summary>
        internal enum ShaderRenderPass
        {
            //Copy = 0,
            Presample = 0,
            Downsample = 1,
            Upsample = 2,
            Composite = 3,
            Debug = 4
        }

        /// <summary>
        /// Material keywords represented in the keyword holder
        /// </summary>
        internal enum MaterialKeywords
        {
            Bloom = 0,
            LensSurface = 1,
            LensFlare = 2,
            Glare1 = 3,
            DebugRawBloom = 4,
            DebugRawLensFlare = 5,
            DebugRawGlare = 6,
            DebugBloom = 7,
            DebugLensFlare = 8,
            DebugGlare = 9,
            DebugComposite = 10,
            LegacyBlit = 11,
            RenderPriorityQuality = 12,
            Natural = 13,
            Glare2 = 14,
            Glare3 = 15,
            Glare4 = 16,
            Null = 17,
            RenderPriorityBalanced = 18,
            HQAntiFlickerFilter = 19
        }
        
        /// <summary>
        /// Keyword represented as with state
        /// </summary>
        internal struct Keyword
        {
            internal string name;
            internal bool enabled;

            internal Keyword(string name, bool enabled)
            {
                this.name = name;
                this.enabled = enabled;
            }
        }
    }
}