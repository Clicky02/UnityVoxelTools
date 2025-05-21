using UnityEngine;
using System.Collections;

[System.Serializable]
public struct VoxCoord
{
    public static VoxCoord INVALID = new(127, 127, 127);

    public static VoxCoord ONE = new(1, 1, 1);
    public static VoxCoord ZERO = new(0, 0, 0);
    public static VoxCoord RIGHT = new(1, 0, 0);
    public static VoxCoord LEFT = new(-1, 0, 0);
    public static VoxCoord UP = new(0, 1, 0);
    public static VoxCoord DOWN = new(0, -1, 0);
    public static VoxCoord FORWARD = new(0, 0, 1);
    public static VoxCoord BACK = new(0, 0, -1);


    public sbyte x;
    public sbyte y;
    public sbyte z;


    public VoxCoord(sbyte x = 0, sbyte y = 0, sbyte z = 0)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public bool Valid
    {
        get
        {
            return
                this.x >= 0 && this.x <= 126 &&
                this.y >= 0 && this.y <= 126 &&
                this.z >= 0 && this.z <= 126;
        }
    }

    public override readonly string ToString()
    {
        return string.Format("VoxCoord({0},{1},{2})", x, y, z);
    }

    public static VoxCoord operator +(VoxCoord a, VoxCoord b)
    {
        return new VoxCoord((sbyte)(a.x + b.x), (sbyte)(a.y + b.y), (sbyte)(a.z + b.z));
    }

    public static VoxCoord operator -(VoxCoord a, VoxCoord b)
    {
        return new VoxCoord((sbyte)(a.x - b.x), (sbyte)(a.y - b.y), (sbyte)(a.z - b.z));
    }

    public float Length()
    {
        return Mathf.Sqrt((float)(x * x) + (float)(y * y) + (float)(z * z));
    }

    public static float Distance(VoxCoord a, VoxCoord b)
    {
        return (a - b).Length();
    }

    public VoxCoord WithX(sbyte x)
    {
        return new VoxCoord(x, y, z);
    }

    public VoxCoord WithY(sbyte y)
    {
        return new VoxCoord(x, y, z);
    }
    public VoxCoord WithZ(sbyte z)
    {
        return new VoxCoord(x, y, z);
    }

    public bool Equals(VoxCoord other)
    {
        return x == other.x && z == other.z && y == other.y;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }
}
