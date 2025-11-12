using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable, VolumeComponentMenu("Custom/Sobel Outline")]
public class SobelOutlinePP : VolumeComponent, IPostProcessComponent
{
    [Header("Settings")]
    public ColorParameter outlineColor = new(Color.black, true, true, true);
    public FloatParameter outlineThickness = new ClampedFloatParameter(0.5f, 0f, 1.5f);
    public FloatParameter depthMultiplier = new ClampedFloatParameter(1f, 0f, 1f);
    public FloatParameter depthBias = new ClampedFloatParameter(0.5f, 0.1f, 1f);
    public FloatParameter normalMultiplier = new ClampedFloatParameter(0.5f, 0f, 1.5f);
    public FloatParameter normalBias = new ClampedFloatParameter(1f, 0f, 100f);

    public bool IsActive() => true;
    public bool IsTileCompatible() => true;
}
