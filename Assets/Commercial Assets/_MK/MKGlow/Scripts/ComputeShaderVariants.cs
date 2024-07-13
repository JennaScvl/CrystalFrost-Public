//////////////////////////////////////////////////////
// MK Glow Compute Shader Variants	    		    //
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2021 All rights reserved.            //
//////////////////////////////////////////////////////
using System.Collections.Generic;

namespace MK.Glow
{
    /////////////////////////////////////////////////////////////////////////////////////////////
    // Compute shader variants
    /////////////////////////////////////////////////////////////////////////////////////////////
    internal sealed class ComputeShaderVariants
    {
        private Dictionary<KeywordState, int> variants = new System.Collections.Generic.Dictionary<KeywordState, int>();

        internal void GetVariantNumber(KeywordState features, out int index)
        {
            variants.TryGetValue(features, out index);
        } 

        internal static class KeywordValues
        {
            internal const int BLOOM = 1;
            internal const int LENS_SURFACE = 1;
            internal const int LENS_FLARE = 1;
            internal const int GLARE = 4;
            internal const int MK_NATURAL = 1;
            internal const int RENDER_PRIORITY = 2;
        }

        internal struct KeywordState
        {
            public int bloom;
            public int lensSurface;
            public int lensFlare;
            public int glare;
            public int natural;
            public int renderPriority;

            public KeywordState(int bloom, int lensSurface, int lensFlare, int glare, int natural, int renderPriority)
            {
                this.bloom = bloom;
                this.lensSurface = lensSurface;
                this.lensFlare = lensFlare;
                this.glare = glare;
                this.natural = natural;
                this.renderPriority = renderPriority;
            }
        }

        public ComputeShaderVariants(int offset)
        {
            int count = 0;

            for(int rp = 0; rp <=KeywordValues.RENDER_PRIORITY; rp++)
            {
                for(int n = 0; n <=KeywordValues.MK_NATURAL; n++)
                {
                    for(int g = 0; g <=KeywordValues.GLARE; g++)
                    {
                        for(int lf = 0; lf <=KeywordValues.LENS_FLARE; lf++)
                        {
                            for(int ls = 0; ls <=KeywordValues.LENS_SURFACE; ls++)
                            {
                                for(int b = 0; b <=KeywordValues.BLOOM; b++)
                                {
                                    variants.Add(new KeywordState(b, ls, lf, g, n, rp), count + offset);
                                    count++;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}