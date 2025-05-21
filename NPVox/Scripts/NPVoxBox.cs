using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Assertions;


[System.Serializable]
public class NPVoxBox
{
    public static NPVoxBox INVALID
    {
        get
        {
            return new NPVoxBox(VoxCoord.INVALID, VoxCoord.INVALID);
        }
    }

    [SerializeField]
    private VoxCoord leftDownBack;
    public VoxCoord LeftDownBack
    {
        get
        {
            return leftDownBack;
        }
        set
        {
            if (value.x > rightUpForward.x)
            {
                rightUpForward.x = value.x;
            }
            if (value.y > rightUpForward.y)
            {
                rightUpForward.y = value.y;
            }
            if (value.z > rightUpForward.z)
            {
                rightUpForward.z = value.z;
            }
            leftDownBack = value;
        }
    }

    [SerializeField]
    private VoxCoord rightUpForward;
    public VoxCoord RightUpForward
    {
        get
        {
            return rightUpForward;
        }
        set
        {
            if (value.x < leftDownBack.x)
            {
                leftDownBack.x = value.x;
            }
            if (value.y < leftDownBack.y)
            {
                leftDownBack.y = value.y;
            }
            if (value.z < leftDownBack.z)
            {
                leftDownBack.z = value.z;
            }
            rightUpForward = value;
        }
    }

    public VoxCoord LeftDownForward
    {
        get { return new VoxCoord(leftDownBack.x, leftDownBack.y, rightUpForward.z); }
        set
        {
            if (value.x > rightUpForward.x)
            {
                rightUpForward.x = value.x;
            }
            leftDownBack.x = value.x;

            if (value.y > rightUpForward.y)
            {
                rightUpForward.y = value.y;
            }
            leftDownBack.y = value.y;

            if (value.z < leftDownBack.z)
            {
                this.leftDownBack.z = value.z;
            }
            rightUpForward.z = value.z;
        }
    }
    public VoxCoord LeftUpBack
    {
        get { return new VoxCoord(leftDownBack.x, rightUpForward.y, leftDownBack.z); }
        set
        {
            if (value.x > rightUpForward.x)
            {
                rightUpForward.x = value.x;
            }
            leftDownBack.x = value.x;

            if (value.y < leftDownBack.y)
            {
                leftDownBack.y = value.y;
            }
            rightUpForward.y = value.y;

            if (value.z > rightUpForward.z)
            {
                this.rightUpForward.z = value.z;
            }
            leftDownBack.z = value.z;
        }
    }

    public VoxCoord LeftUpForward
    {
        get { return new VoxCoord(leftDownBack.x, rightUpForward.y, rightUpForward.z); }
        set
        {
            if (value.x > rightUpForward.x)
            {
                rightUpForward.x = value.x;
            }
            leftDownBack.x = value.x;

            if (value.y < leftDownBack.y)
            {
                leftDownBack.y = value.y;
            }
            rightUpForward.y = value.y;

            if (value.z < leftDownBack.z)
            {
                this.leftDownBack.z = value.z;
            }
            rightUpForward.z = value.z;
        }
    }
    public VoxCoord RightDownBack
    {
        get { return new VoxCoord(rightUpForward.x, leftDownBack.y, leftDownBack.z); }
        set
        {
            if (value.x < leftDownBack.x)
            {
                leftDownBack.x = value.x;
            }
            rightUpForward.x = value.x;

            if (value.y > rightUpForward.y)
            {
                rightUpForward.y = value.y;
            }
            leftDownBack.y = value.y;

            if (value.z > rightUpForward.z)
            {
                this.rightUpForward.z = value.z;
            }
            leftDownBack.z = value.z;
        }
    }
    public VoxCoord RightDownForward
    {
        get { return new VoxCoord(rightUpForward.x, leftDownBack.y, rightUpForward.z); }
        set
        {
            if (value.x < leftDownBack.x)
            {
                leftDownBack.x = value.x;
            }
            rightUpForward.x = value.x;

            if (value.y > rightUpForward.y)
            {
                rightUpForward.y = value.y;
            }
            leftDownBack.y = value.y;

            if (value.z < leftDownBack.z)
            {
                this.leftDownBack.z = value.z;
            }
            rightUpForward.z = value.z;
        }
    }
    public VoxCoord RightUpBack
    {
        get { return new VoxCoord(rightUpForward.x, rightUpForward.y, leftDownBack.z); }
        set
        {
            if (value.x < leftDownBack.x)
            {
                leftDownBack.x = value.x;
            }
            rightUpForward.x = value.x;

            if (value.y < leftDownBack.y)
            {
                leftDownBack.y = value.y;
            }
            rightUpForward.y = value.y;

            if (value.z > rightUpForward.z)
            {
                this.rightUpForward.z = value.z;
            }
            leftDownBack.z = value.z;
        }
    }


    public sbyte Left
    {
        get { return leftDownBack.x; }
        set
        {
            if (value > rightUpForward.x)
            {
                rightUpForward.x = value;
            }
            leftDownBack.x = value;
        }
    }

    public sbyte Right
    {
        get { return rightUpForward.x; }
        set
        {
            if (value < leftDownBack.x)
            {
                leftDownBack.x = value;
            }
            rightUpForward.x = value;
        }
    }

    public sbyte Down
    {
        get { return leftDownBack.y; }
        set
        {
            if (value > rightUpForward.y)
            {
                rightUpForward.y = value;
            }
            leftDownBack.y = value;
        }
    }

    public sbyte Up
    {
        get { return rightUpForward.y; }
        set
        {
            if (value < leftDownBack.y)
            {
                leftDownBack.y = value;
            }
            rightUpForward.y = value;
        }
    }

    public sbyte Back
    {
        get { return leftDownBack.z; }
        set
        {
            if (value > rightUpForward.z)
            {
                rightUpForward.z = value;
            }
            leftDownBack.z = value;
        }
    }

    public sbyte Forward
    {
        get { return rightUpForward.z; }
        set
        {
            if (value < leftDownBack.z)
            {
                leftDownBack.z = value;
            }
            rightUpForward.z = value;
        }
    }
    public VoxCoord Size
    {
        get
        {
            return new VoxCoord(
                (sbyte)(rightUpForward.x - leftDownBack.x + 1),
                (sbyte)(rightUpForward.y - leftDownBack.y + 1),
                (sbyte)(rightUpForward.z - leftDownBack.z + 1)
            );
        }
    }

    public VoxCoord Center
    {
        get
        {
            VoxCoord size = Size;
            Assert.IsTrue(size.x % 2 == 1, "Center is not representable in NPVoxCoords for this Box");
            Assert.IsTrue(size.y % 2 == 1, "Center is not representable in NPVoxCoords for this Box");
            Assert.IsTrue(size.z % 2 == 1, "Center is not representable in NPVoxCoords for this Box");

            return leftDownBack + new VoxCoord(
                (sbyte)(size.x / 2),
                (sbyte)(size.y / 2),
                (sbyte)(size.z / 2)
            );
        }
    }
    public VoxCoord RoundedCenter
    {
        get
        {
            VoxCoord size = Size;
            return leftDownBack + new VoxCoord(
                (sbyte)(Mathf.Round(((float)size.x - 1f) / 2f)),
                (sbyte)(Mathf.Round(((float)size.y - 1f) / 2f)),
                (sbyte)(Mathf.Round(((float)size.z - 1f) / 2f))
            );
        }
    }

    public NPVoxBox(VoxCoord leftDownBack, VoxCoord rightUpForward)
    {
        //        if ( rightUpForward.X < leftDownBack.X )
        //        {
        //            Debug.Log( "WTf" );
        //        }
        Assert.IsTrue(rightUpForward.x >= leftDownBack.x, rightUpForward.x + " is < than " + leftDownBack.x);
        Assert.IsTrue(rightUpForward.y >= leftDownBack.y);
        Assert.IsTrue(rightUpForward.z >= leftDownBack.z);
        this.leftDownBack = leftDownBack;
        this.rightUpForward = rightUpForward;
    }

    public static NPVoxBox FromCenterSize(VoxCoord center, VoxCoord size)
    {
        Assert.IsTrue(size.x % 2 == 1, "Center is not representable in NPVoxCoords for this Box");
        Assert.IsTrue(size.y % 2 == 1, "Center is not representable in NPVoxCoords for this Box");
        Assert.IsTrue(size.z % 2 == 1, "Center is not representable in NPVoxCoords for this Box");
        VoxCoord SizeHalf = new VoxCoord(
            (sbyte)(size.x / 2),
            (sbyte)(size.y / 2),
            (sbyte)(size.z / 2)
        );
        return new NPVoxBox(center - SizeHalf, center + SizeHalf);
    }

    public bool Contains(VoxCoord coord)
    {
        return
            coord.x >= leftDownBack.x && coord.x <= rightUpForward.x &&
            coord.y >= leftDownBack.y && coord.y <= rightUpForward.y &&
            coord.z >= leftDownBack.z && coord.z <= rightUpForward.z;
    }


    public override bool Equals(System.Object other)
    {
        if (other is not NPVoxBox o)
        {
            return false;
        }
        return o.LeftDownBack.Equals(this.leftDownBack) && o.RightUpForward.Equals(this.rightUpForward);
    }

    public override int GetHashCode()
    {
        throw new System.Exception("not implemented");
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
                    yield return new VoxCoord((sbyte)(leftDownBack.x + x), (sbyte)(leftDownBack.y + y), (sbyte)(leftDownBack.z + z));
                }
            }
        }
    }

    public void EnlargeToInclude(VoxCoord coord)
    {
        if (coord.x < this.Left)
        {
            this.Left = coord.x;
        }
        if (coord.y < this.Down)
        {
            this.Down = coord.y;
        }
        if (coord.z < this.Back)
        {
            this.Back = coord.z;
        }
        if (coord.x > this.Right)
        {
            this.Right = coord.x;
        }
        if (coord.y > this.Up)
        {
            this.Up = coord.y;
        }
        if (coord.z > this.Forward)
        {
            this.Forward = coord.z;
        }
    }

    public void Clamp(NPVoxBox max)
    {
        if (Left < max.Left) Left = max.Left;
        if (Right > max.Right) Right = max.Right;
        if (Down < max.Down) Down = max.Down;
        if (Up > max.Up) Up = max.Up;
        if (Back < max.Back) Back = max.Back;
        if (Forward > max.Forward) Forward = max.Forward;
    }

    public NPVoxBox Clone()
    {
        return new NPVoxBox(LeftDownBack, RightUpForward);
    }

    public Vector3 SaveCenter
    {
        get
        {
            VoxCoord size = Size;
            return new Vector3(
                leftDownBack.x + ((float)size.x - 1f) / 2f,
                leftDownBack.y + ((float)size.y - 1f) / 2f,
                leftDownBack.z + ((float)size.z - 1f) / 2f
            );
        }
        set
        {
            VoxCoord size = Size;
            VoxCoord newLeftDownBack = new VoxCoord(
                (sbyte)(Mathf.Round(value.x - ((float)size.x - 1f) / 2f)),
                (sbyte)(Mathf.Round(value.y - ((float)size.y - 1f) / 2f)),
                (sbyte)(Mathf.Round(value.z - ((float)size.z - 1f) / 2f))
            );
            VoxCoord delta = newLeftDownBack - leftDownBack;
            leftDownBack = newLeftDownBack;
            rightUpForward = rightUpForward + delta;
        }
    }

    public static NPVoxBox Union(NPVoxBox x, NPVoxBox y)
    {
        NPVoxBox box = new NPVoxBox(x.leftDownBack, x.rightUpForward);
        box.EnlargeToInclude(y.LeftDownBack);
        box.EnlargeToInclude(y.RightUpForward);
        return box;
    }
}