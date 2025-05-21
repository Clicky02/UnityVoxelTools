using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class VoxShape : ICloneable
{
    [SerializeField]
    private uint id;
    [SerializeField]
    private VoxCoord size;
    [SerializeField]
    protected byte[,,] voxels;
    [SerializeField]
    private int numVoxels;
    [SerializeField]
    private VoxTransform transform;

    private List<VoxCoord> voxelListCache = null;

    public VoxShape(VoxCoord size)
    {
        this.transform = VoxTransform.Default();
        this.numVoxels = -1;
        this.size = size;
        this.voxels = new byte[size.x, size.y, size.z];
        this.voxelListCache = null;
    }

    public virtual void CopyOver(VoxShape source)
    {
        this.numVoxels = source.numVoxels;
        this.size = source.size;
        this.voxels = (byte[,,])source.voxels.Clone();
        this.voxelListCache = null;
    }

    public object Clone()
    {
        VoxShape clone = new(size);
        clone.CopyOver(this);
        return clone;
    }

    public bool IsValid => this.numVoxels > 0;
    public VoxCoord Size => size;
    public sbyte SizeX => size.x;
    public sbyte SizeY => size.y;
    public sbyte SizeZ => size.z;

    public VoxTransform Transform
    {
        get => transform;
        set => transform = value;
    }

    public int NumVoxels
    {
        get => numVoxels;
        set => numVoxels = value;
    }



    public byte this[int x, int y, int z]
    {
        get => voxels[x, y, z];
        set => voxels[x, y, z] = value;
    }

    public byte this[VoxCoord c]
    {
        get => voxels[c.x, c.y, c.z];
        set => voxels[c.x, c.y, c.z] = value;
    }




    public void SetVoxel(VoxCoord coord, byte color)
    {
        if (IsInside(coord))
        {
            this[coord] = color;
        }
    }

    public byte GetVoxel(VoxCoord coord)
    {
        return this[coord];
    }

    public bool HasVoxel(VoxCoord coord)
    {
        return IsInside(coord) && this[coord] != 0;
    }

    public bool HasVoxelFast(VoxCoord coord)
    {
        return this[coord] != 0;
    }

    public void UnsetVoxel(VoxCoord coord)
    {
        if (IsInside(coord))
        {
            this[coord] = 0;
        }
    }

    public bool IsInside(VoxCoord coord)
    {
        return coord.Valid && coord.x < size.x && coord.y < size.y && coord.z < size.z;
    }

    public VoxCoord GetVoxCoord(int index)
    {
        int y = index / (size.x * size.z);
        int j = index % (size.x * size.z);
        int z = j / size.x;
        int x = j % Size.x;

        return new VoxCoord((sbyte)x, (sbyte)y, (sbyte)z);
    }

    public VoxCoord LoopCoord(VoxCoord coord, NPVoxFaces loop)
    {
        if (loop.Right != 0)
        {
            if (loop.Right < 0 && coord.x >= SizeX)
            {
                coord.x = (sbyte)(coord.x + loop.Right);
            }

            coord.x = (sbyte)(coord.x % SizeX);
        }
        if (loop.Left != 0)
        {
            if (loop.Left < 0 && coord.x < 0)
            {
                coord.x = (sbyte)(coord.x - loop.Left);
            }

            while (coord.x < 0)
                coord.x += SizeX;
        }
        if (loop.Up != 0)
        {
            if (loop.Up < 0 && coord.y >= SizeY)
            {
                coord.y = (sbyte)(coord.y + loop.Up);
            }

            coord.y = (sbyte)(coord.y % SizeY);
        }
        if (loop.Down != 0)
        {
            if (loop.Down < 0 && coord.y < 0)
            {
                coord.y = (sbyte)(coord.y - loop.Down);
            }

            while (coord.y < 0)
                coord.y += SizeY;
        }
        if (loop.Forward != 0)
        {
            if (loop.Forward < 0 && coord.z >= SizeZ)
            {
                coord.z = (sbyte)(coord.z + loop.Forward);
            }

            coord.z = (sbyte)(coord.z % SizeZ);
        }
        if (loop.Back != 0)
        {
            if (loop.Back < 0 && coord.z < 0)
            {
                coord.z = (sbyte)(coord.z - loop.Back);
            }

            while (coord.z < 0)
                coord.z += SizeZ;
        }
        return coord;
    }

    public IEnumerable<VoxCoord> Enumerate()
    {
        VoxCoord size = Size;

        for (sbyte x = 0; x < size.x; x++)
        {
            for (sbyte y = 0; y < size.y; y++)
            {
                for (sbyte z = 0; z < size.z; z++)
                {
                    yield return new VoxCoord(x, y, z);
                }
            }
        }
    }

    public void InvalidateVoxelCache()
    {
        if (voxelListCache != null)
        {
            voxelListCache = null;
        }
    }

    public IEnumerable<VoxCoord> EnumerateVoxels()
    {
        //        if (voxelListCache != null)
        //        {
        //            return voxelListCache;
        //        }
        //        voxelListCache = new List<NPVoxCoord>();
        VoxCoord size = Size;

        for (sbyte x = 0; x < size.x; x++)
        {
            for (sbyte y = 0; y < size.y; y++)
            {
                for (sbyte z = 0; z < size.z; z++)
                {
                    var coord = new VoxCoord(x, y, z);
                    if (HasVoxel(coord))
                    {
                        //                        voxelListCache.Add(coord);
                        yield return coord;
                    }
                }
            }
        }

        //        return voxelListCache;
    }

    public NPVoxBox Clamp(NPVoxBox box)
    {
        if (IsInside(box.LeftDownBack) && IsInside(box.RightUpForward))
        {
            return box;
        }
        else
        {
            return new NPVoxBox(Clamp(box.LeftDownBack), Clamp(box.RightUpForward));
        }
    }

    public VoxCoord Clamp(VoxCoord coord)
    {
        if (IsInside(coord))
        {
            return coord;
        }
        else
        {
            VoxCoord size = this.size;
            return new VoxCoord(
                (sbyte)(coord.x < 0 ? 0 : (coord.x >= size.x ? size.x - 1 : coord.x)),
                (sbyte)(coord.y < 0 ? 0 : (coord.y >= size.y ? size.y - 1 : coord.y)),
                (sbyte)(coord.z < 0 ? 0 : (coord.z >= size.z ? size.z - 1 : coord.z))
            );
        }
    }

    public NPVoxBox BoundingBox
    {
        get { return new NPVoxBox(VoxCoord.ZERO, Size - VoxCoord.ONE); }
    }

    public NPVoxBox BoundingBoxMinimal
    {
        get
        {
            NPVoxBox box = null;
            foreach (VoxCoord coord in this.EnumerateVoxels())
            {
                if (this[coord] == 0) { continue; }

                if (box == null)
                {
                    box = new NPVoxBox(coord, coord);
                }
                else
                {
                    box.EnlargeToInclude(coord);
                }
            }
            return box;
        }
    }

    public void RecalculateNumVoxels(bool withWarning = false)
    {
        InvalidateVoxelCache();
        int numVoxels = 0;
        foreach (VoxCoord coord in this.EnumerateVoxels())
        {
            numVoxels++;
        }
        if (withWarning && this.numVoxels != numVoxels)
        {
            Debug.LogWarning("NumVoxels was wrong: " + this.numVoxels + " Correct: " + numVoxels);
        }
        this.numVoxels = numVoxels;
    }
}
