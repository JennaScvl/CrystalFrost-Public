//////////////////////////////////////////////////////
// MK Glow Common       	    	    	       	//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2021 All rights reserved.            //
//////////////////////////////////////////////////////
namespace MK.Glow
{
    /// <summary>
    /// Type of the glow, selective requires seperate shaders
    /// </summary>
    public enum Workflow
    {
        Threshold = 0,
        Selective = 1,
        Natural = 2
    }

    /// <summary>
    /// How anti flicker should perform
    /// </summary>
    public enum AntiFlickerMode
    {
        Balanced = 0,
        Strong = 1
    }

    /// <summary>
    /// Glow _settings.quality profile forrendering
    /// </summary>
    public enum Quality
    {
        Ultra = 1,
        High = 2,
        Medium = 4,
        Low = 8,
        VeryLow = 12
    }

    /// <summary>
    /// Debugging, Raw = Glowmap, default = pre ready, composite = Finalglow without Source image
    /// </summary>
    public enum DebugView
    {
        None = 0,
        RawBloom = 1,
        RawLensFlare = 2,
        RawGlare = 3,
        Bloom = 4,
        LensFlare = 5,
        Glare = 6,
        Composite = 7,
    }

    /// <summary>
    /// Defines the focus of the render pipeline
    /// </summary>
    public enum RenderPriority
    {
        Quality = 0,
        Balanced = 1,
        Performance = 2
    }

    /// <summary>
    /// Defines the style of the Lens Flare
    /// </summary>
    public enum LensFlareStyle
    {
        Custom = 0,
        Average = 1,
        MultiAverage = 2,
        Old = 3,
        OldFocused = 4,
        Distorted = 5
    }

    /// <summary>
    /// Defines the style of the glare
    /// </summary>
    public enum GlareStyle
    {
        Custom = 0,
        Line = 1,
        Tri = 2,
        Cross = 3,
        DistortedCross = 4,
        Star = 5,
        Flake = 6
    }

    /// <summary>
    /// Dimension struct for representing render context size
    /// </summary>
    internal struct RenderDimension : IDimension
    {
        public RenderDimension(int width, int height) : this()
        {
            this.width = width;
            this.height = height;
        }

        public int width { get; set; }
        public int height { get; set; }
        public RenderDimension renderDimension { get{ return this; } }
    }
    
    /// <summary>
    /// Defines which renderpipeline is used
    /// </summary>
    internal enum RenderPipeline
    {
        Legacy = 0,
        SRP = 1
    }
}
