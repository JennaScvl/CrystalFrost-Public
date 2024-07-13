//////////////////////////////////////////////////////
// MK Glow ICamera Data     	                    //
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2020 All rights reserved.            //
//////////////////////////////////////////////////////
using UnityEngine;

namespace MK.Glow
{
    internal interface ICameraData
    {
        int GetCameraWidth();
        int GetCameraHeight();
        bool GetStereoEnabled();
        float GetAspect();
        Matrix4x4 GetWorldToCameraMatrix();
        bool GetOverwriteDescriptor();
        UnityEngine.Rendering.TextureDimension GetOverwriteDimension();
        int GetOverwriteVolumeDepth();
        bool GetTargetTexture();
    }
}
