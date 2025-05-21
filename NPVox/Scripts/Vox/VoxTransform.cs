using System;
using UnityEngine;

[Serializable]
public struct VoxTransform : ICloneable
{
    [SerializeField]
    public Vector3 translation;
    [SerializeField]
    public Vector3 rotation;
    [SerializeField]
    public Vector3 scale;

    public static VoxTransform Default()
    {
        return new VoxTransform
        {
            translation = Vector3.zero,
            rotation = Vector3.zero,
            scale = Vector3.one
        };
    }

    public object Clone()
    {
        return new VoxTransform
        {
            translation = new Vector3(translation.x, translation.y, translation.z),
            rotation = new Vector3(rotation.x, rotation.y, rotation.z),
            scale = new Vector3(scale.x, scale.y, scale.z)
        };
    }
}