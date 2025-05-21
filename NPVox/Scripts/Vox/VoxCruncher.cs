using System.Collections.Generic;
using UnityEngine;

public enum VoxCruncherMode
{
    Stupid,
    Culled,
    Greedy,
}

public struct VoxVisibleFaces
{
    public bool left;
    public bool right;
    public bool bottom;
    public bool top;
    public bool back;
    public bool front;

    public VoxVisibleFaces(bool _left, bool _right, bool _bottom, bool _top, bool _back, bool _front)
    {
        left = _left;
        right = _right;
        bottom = _bottom;
        top = _top;
        back = _back;
        front = _front;
    }
}


public class CrunchedVoxel
{
    public VoxCoord begin;
    public VoxCoord end;

    public int material;
    public VoxVisibleFaces faces;

    public CrunchedVoxel(VoxCoord begin, VoxCoord end, int _material)
    {
        this.begin = begin;
        this.end = end;

        material = _material;

        faces.left = true;
        faces.right = true;
        faces.top = true;
        faces.bottom = true;
        faces.front = true;
        faces.back = true;
    }

    public CrunchedVoxel(VoxCoord begin, VoxCoord end, VoxVisibleFaces _faces, int _material)
    {
        this.begin = begin;
        this.end = end;

        material = _material;
        faces = _faces;
    }

    public CrunchedVoxel(sbyte begin_x, sbyte end_x, sbyte begin_y, sbyte end_y, sbyte begin_z, sbyte end_z, int _material)
    {
        begin.x = begin_x;
        begin.y = begin_y;
        begin.z = begin_z;

        end.x = end_x;
        end.y = end_y;
        end.z = end_z;

        material = _material;

        faces.left = true;
        faces.right = true;
        faces.top = true;
        faces.bottom = true;
        faces.front = true;
        faces.back = true;
    }

    public CrunchedVoxel(sbyte begin_x, sbyte end_x, sbyte begin_y, sbyte end_y, sbyte begin_z, sbyte end_z, VoxVisibleFaces _faces, int _material)
    {
        begin.x = begin_x;
        begin.y = begin_y;
        begin.z = begin_z;

        end.x = end_x;
        end.y = end_y;
        end.z = end_z;

        material = _material;
        faces = _faces;
    }
}


public interface ICrunchedVoxelStrategy
{
    VoxCrunchedShape CalcCrunchedShape(VoxShape shape, Color32[] palette);
}

public class CrunchedVoxelStupid : ICrunchedVoxelStrategy
{
    public VoxCrunchedShape CalcCrunchedShape(VoxShape shape, Color32[] palette)
    {
        var crunchedVoxel = new CrunchedVoxel[shape.NumVoxels];
        var faces = new VoxVisibleFaces(true, true, true, true, true, true);

        int n = 0;
        foreach (var coord in shape.EnumerateVoxels())
        {
            crunchedVoxel[n++] = new CrunchedVoxel(coord, coord, faces, shape[coord]);
        }

        return new VoxCrunchedShape(crunchedVoxel);
    }
}

public class CrunchedVoxelCulled : ICrunchedVoxelStrategy
{
    public static bool GetVisibleFaces(VoxShape shape, VoxCoord coord, int material, Color32[] palette, out VoxVisibleFaces faces)
    {
        VoxCoord bound = shape.Size;
        sbyte x = coord.x;
        sbyte y = coord.y;
        sbyte z = coord.z;

        byte[] instanceID = new byte[6] { 0, 0, 0, 0, 0, 0 };

        if (x >= 1) instanceID[0] = shape[(byte)(x - 1), y, z];
        if (y >= 1) instanceID[2] = shape[x, (byte)(y - 1), z];
        if (z >= 1) instanceID[4] = shape[x, y, (byte)(z - 1)];
        if (x <= bound.x) instanceID[1] = shape[(byte)(x + 1), y, z];
        if (y <= bound.y) instanceID[3] = shape[x, (byte)(y + 1), z];
        if (z <= bound.z) instanceID[5] = shape[x, y, (byte)(z + 1)];

        var alpha = palette[material].a;
        if (alpha < 255)
        {
            bool f1 = (instanceID[0] == 0) || palette[instanceID[0]].a != alpha;
            bool f2 = (instanceID[1] == 0) || palette[instanceID[1]].a != alpha;
            bool f3 = (instanceID[2] == 0) || palette[instanceID[2]].a != alpha;
            bool f4 = (instanceID[3] == 0) || palette[instanceID[3]].a != alpha;
            bool f5 = (instanceID[4] == 0) || palette[instanceID[4]].a != alpha;
            bool f6 = (instanceID[5] == 0) || palette[instanceID[5]].a != alpha;

            faces.left = f1;
            faces.right = f2;
            faces.bottom = f3;
            faces.top = f4;
            faces.front = f5;
            faces.back = f6;
        }
        else
        {
            bool f1 = (instanceID[0] == 0) || palette[instanceID[0]].a < 255;
            bool f2 = (instanceID[1] == 0) || palette[instanceID[1]].a < 255;
            bool f3 = (instanceID[2] == 0) || palette[instanceID[2]].a < 255;
            bool f4 = (instanceID[3] == 0) || palette[instanceID[3]].a < 255;
            bool f5 = (instanceID[4] == 0) || palette[instanceID[4]].a < 255;
            bool f6 = (instanceID[5] == 0) || palette[instanceID[5]].a < 255;

            faces.left = f1;
            faces.right = f2;
            faces.bottom = f3;
            faces.top = f4;
            faces.front = f5;
            faces.back = f6;
        }

        return faces.left | faces.right | faces.bottom | faces.top | faces.front | faces.back;
    }

    public VoxCrunchedShape CalcCrunchedShape(VoxShape shape, Color32[] palette)
    {
        var crunchers = new List<CrunchedVoxel>();
        foreach (var coord in shape.EnumerateVoxels())
        {
            var c = shape[coord];
            if (!GetVisibleFaces(shape, coord, c, palette, out VoxVisibleFaces faces))
                continue;
            crunchers.Add(new CrunchedVoxel(coord, coord, faces, c));
        }

        var array = new CrunchedVoxel[crunchers.Count];
        int i = 0;
        foreach (var it in crunchers)
            array[i++] = it;

        return new VoxCrunchedShape(array);
    }
}

public class CrunchedVoxelGreedy : ICrunchedVoxelStrategy
{
    public VoxCrunchedShape CalcCrunchedShape(VoxShape shape, Color32[] palette)
    {
        var crunchers = new List<CrunchedVoxel>();
        var dims = new int[] { shape.SizeX, shape.SizeY, shape.SizeZ };

        var alloc = System.Math.Max(dims[0], System.Math.Max(dims[1], dims[2]));
        var mask = new int[alloc * alloc];

        for (var d = 0; d < 3; ++d)
        {
            var u = (d + 1) % 3;
            var v = (d + 2) % 3;

            var x = new int[3] { 0, 0, 0 };
            var q = new int[3] { 0, 0, 0 };

            q[d] = 1;

            var faces = new VoxVisibleFaces(false, false, false, false, false, false);

            for (x[d] = -1; x[d] < dims[d];)
            {
                var n = 0;

                for (x[v] = 0; x[v] < dims[v]; ++x[v])
                {
                    for (x[u] = 0; x[u] < dims[u]; ++x[u])
                    {
                        var a = x[d] >= 0 ? shape[x[0], x[1], x[2]] : 0;
                        var b = x[d] < dims[d] - 1 ? shape[x[0] + q[0], x[1] + q[1], x[2] + q[2]] : 0;
                        if (a != b)
                        {
                            if (a == 0)
                                mask[n++] = b;
                            else if (b == 0)
                                mask[n++] = -a;
                            else
                                mask[n++] = -b;
                        }
                        else
                        {
                            mask[n++] = 0;
                        }
                    }
                }

                ++x[d];

                n = 0;

                for (var j = 0; j < dims[v]; ++j)
                {
                    for (var i = 0; i < dims[u];)
                    {
                        var c = mask[n];
                        if (c == 0)
                        {
                            ++i; ++n;
                            continue;
                        }

                        var w = 1;
                        var h = 1;
                        var k = 0;

                        for (; (i + w) < dims[u] && c == mask[n + w]; ++w) { }

                        var done = false;
                        for (; (j + h) < dims[v]; ++h)
                        {
                            for (k = 0; k < w; ++k)
                            {
                                if (c != mask[n + k + h * dims[u]])
                                {
                                    done = true;
                                    break;
                                }
                            }

                            if (done)
                                break;
                        }

                        x[u] = i; x[v] = j;

                        var du = new int[3] { 0, 0, 0 };
                        var dv = new int[3] { 0, 0, 0 };

                        du[u] = w;
                        dv[v] = h;

                        var v1 = new Vector3Int(x[0], x[1], x[2]);
                        var v2 = new Vector3Int(x[0] + du[0] + dv[0], x[1] + du[1] + dv[1], x[2] + du[2] + dv[2]);

                        v2.x = System.Math.Max(v2.x - 1, 0);
                        v2.y = System.Math.Max(v2.y - 1, 0);
                        v2.z = System.Math.Max(v2.z - 1, 0);

                        if (c > 0)
                        {
                            faces.front = d == 2;
                            faces.back = false;
                            faces.left = d == 0;
                            faces.right = false;
                            faces.top = false;
                            faces.bottom = d == 1;
                        }
                        else
                        {
                            c = -c;
                            faces.front = false;
                            faces.back = d == 2;
                            faces.left = false;
                            faces.right = d == 0;
                            faces.top = d == 1;
                            faces.bottom = false;
                        }

                        crunchers.Add(new CrunchedVoxel((sbyte)v1.x, (sbyte)v2.x, (sbyte)v1.y, (sbyte)v2.y, (sbyte)v1.z, (sbyte)v2.z, faces, c));

                        for (var l = 0; l < h; ++l)
                        {
                            for (k = 0; k < w; ++k)
                                mask[n + k + l * dims[u]] = 0;
                        }

                        i += w; n += w;
                    }
                }
            }
        }

        var array = new CrunchedVoxel[crunchers.Count];
        int count = 0;
        foreach (var it in crunchers)
            array[count++] = it;

        return new VoxCrunchedShape(array);
    }
}

public class VOXPolygonCruncher
{
    public static VoxCrunchedShape CalcCrunchedShape(VoxShape chunk, Color32[] palette, VoxCruncherMode mode)
    {
        return mode switch
        {
            VoxCruncherMode.Stupid => new CrunchedVoxelStupid().CalcCrunchedShape(chunk, palette),
            VoxCruncherMode.Culled => new CrunchedVoxelCulled().CalcCrunchedShape(chunk, palette),
            VoxCruncherMode.Greedy => new CrunchedVoxelGreedy().CalcCrunchedShape(chunk, palette),
            _ => null,
        };
    }
}

