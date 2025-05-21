using System.Collections.Generic;
using UnityEngine;

public class VoxCrunchedShape
{
    public CrunchedVoxel[] voxels;

    public VoxCrunchedShape(CrunchedVoxel[] array)
    {
        voxels = array;
    }
}

public class VoxCrunchedModelData : ScriptableObject
{
    public Dictionary<int, VoxCrunchedShape> voxels;
}
